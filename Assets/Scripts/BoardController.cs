// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UniteBlocks
{
    public class BoardController : MonoBehaviour
    {
        public static readonly Vector2Int START_POS = new Vector2Int(2, 12);

        private const int BOARD_WIDTH = 6;
        private const int BOARD_HEIGHT = 14;

        private Vector2Int?[,] m_Coord = new Vector2Int?[BOARD_WIDTH, BOARD_HEIGHT];
        private Block[,] m_Origins = new Block[BOARD_WIDTH, BOARD_HEIGHT];

        [SerializeField]
        private GameObject m_BlocksContainer;

        [SerializeField]
        private ScoreManager m_ScoreManager;

        [SerializeField]
        private PointSetting m_PointSetting;

        [SerializeField]
        private float m_DropTime = 0.5f;

        [SerializeField]
        private float m_RotateTime = 1.2f;

        [SerializeField]
        private float m_DissolveTime = 0.28f;

        /// <summary>
        /// そのマスが有効かどうかを検証
        /// </summary>
        bool IsValid(Vector2Int pos)
        {
            if (pos.x < 0 || BOARD_WIDTH <= pos.x) { return false; }
            if (pos.y < 0 || BOARD_HEIGHT <= pos.y) { return false; }
            return true;
        }

        /// <summary>
        /// アイテムがおけるかを検証
        /// </summary>
        public bool CanSettle(Vector2Int pos, Vector2Int? shape = null)
        {
            shape ??= Vector2Int.one;
            for (int x = pos.x; x < pos.x + shape.Value.x; x++)
            {
                for (int y = pos.y; y < pos.y + shape.Value.y; y++)
                {
                    var target = new Vector2Int(x, y);
                    if (!IsValid(target)) { return false; }
                    if (m_Coord[x, y] != null) { return false; }
                }
            }
            return true;
        }

        /// <summary>
        /// アイテムを置く
        /// </summary>
        public void Settle(Vector2Int pos, Block block)
        {
            if (!CanSettle(pos, block.Shape))
            {
                Debug.LogError("セット先が無効");
                return;
            }

            block.transform.parent = m_BlocksContainer.transform;
            m_Origins[pos.x, pos.y] = block;

            for (int x = pos.x; x < pos.x + block.Shape.x; x++)
            {
                for (int y = pos.y; y < pos.y + block.Shape.y; y++)
                {
                    m_Coord[x, y] = pos;
                }
            }
        }

        /// <summary>
        /// アイテムを削除
        /// </summary>
        void Delete(Vector2Int pos)
        {
            if (m_Origins[pos.x, pos.y] == null)
            {
                Debug.LogError("削除対象が存在しない");
                return;
            }

            var block = m_Origins[pos.x, pos.y];

            for (int x = pos.x; x < pos.x + block.Shape.x; x++)
            {
                for (int y = pos.y; y < pos.y + block.Shape.y; y++)
                {
                    m_Coord[x, y] = null;
                }
            }

            m_Origins[pos.x, pos.y] = null;
        }

        /// <summary>
        /// ぷよが着地した後の諸々の処理
        /// </summary>
        /// <returns>ゲームオーバーしたかどうか</returns>
        public async UniTask<bool> DropToBottom()
        {
            List<MotionHandle> handles = new List<MotionHandle>();

            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    if (m_Origins[x, y] == null) { continue; }

                    var block = m_Origins[x, y];
                    int targetHeight = y;

                    // 先に自身を消す事で自身に干渉しないようにする
                    Delete(new Vector2Int(x, y));

                    // 下の行を検査
                    for (int j = y - 1; j >= 0; j--)
                    {
                        if (!CanSettle(new Vector2Int(x, j), block.Shape)) { break; }
                        targetHeight--;
                    }

                    Settle(new Vector2Int(x, targetHeight), block);

                    if (targetHeight == y) { continue; }

                    var handle = LMotion.Create(block.transform.localPosition.y, targetHeight, m_DropTime)
                        .WithEase(Ease.OutBounce)
                        .BindToLocalPositionY(block.transform)
                        .AddTo(this);
                    handles.Add(handle);
                }
            }

            foreach (var handle in handles)
            {
                await handle;
            }
            await Combine();
            return CheckGameOver();
        }

        bool CheckGameOver()
        {
            if (m_Coord[START_POS.x, START_POS.y] == null) { return false; }
            GameManager.Instance.GameOver();
            return true;
        }

        /// <summary>
        /// 塊を作成
        /// </summary>
        async UniTask Combine()
        {
            List<MotionHandle> handles = new List<MotionHandle>();

            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
                    if (m_Origins[x, y] == null) { continue; }
                    var block = m_Origins[x, y];

                    Vector2Int targetShape = block.Shape;
                    List<Vector2Int> deleteBlocks = new List<Vector2Int>();

                    // x,yを左下、i,jを右上の座標としたときの長方形を考える
                    for (int i = x + block.Shape.x - 1; i < BOARD_WIDTH; i++)
                    {
                        for (int j = y + block.Shape.y - 1; j < BOARD_HEIGHT; j++)
                        {
                            if (!CheckInRange(x, y, i, j, ref deleteBlocks)) { continue; }
                            targetShape = new Vector2Int(i - x + 1, j - y + 1);
                        }
                    }

                    if (targetShape.x < 2 || targetShape.y < 2) { continue; }
                    if (targetShape == block.Shape) { continue; }

                    foreach (var pos in deleteBlocks)
                    {
                        Destroy(m_Origins[pos.x, pos.y].gameObject);
                        Delete(pos);
                    }

                    block.Shape = targetShape;
                    for (int i = x; i < x + targetShape.x; i++)
                    {
                        for (int j = y; j < y + targetShape.y; j++)
                        {
                            m_Coord[i, j] = new Vector2Int(x, y);
                        }
                    }

                    Vector3 center = new Vector3(x + (float)block.Shape.x / 2f, y + (float)block.Shape.y / 2f, 0);
                    center += transform.position;
                    float prex = 0f;

                    var handle = LMotion.Create(0f, 360f, m_RotateTime)
                        .WithEase(Ease.OutBack)
                        .Bind(x =>
                        {
                            var y = x - prex;
                            block.gameObject.transform.RotateAround(center, Vector3.up, y);
                            prex = x;
                        })
                        .AddTo(this);
                    handles.Add(handle);
                }
            }

            foreach (var handle in handles)
            {
                await handle;
            }
        }

        /// <summary>
        /// 塊を作れるかどうかをチェック
        /// </summary>
        /// <param name="x0">左下座標</param>
        /// <param name="y0">左下座標</param>
        /// <param name="x1">右上座標</param>
        /// <param name="y1">右上座標</param>
        /// <param name="deleteBlocks">塊を作る際の削除対象</param>
        /// <returns>塊を作れるかどうか</returns>
        bool CheckInRange(int x0, int y0, int x1, int y1, ref List<Vector2Int> deleteBlocks)
        {
            // 幅か高さが1の場合はパス
            if (x0 == x1 || y0 == y1) { return false; }
            Color type = m_Origins[x0, y0].Color;
            List<Vector2Int> origins = new List<Vector2Int>();

            // 範囲内のブロックをリストに格納
            for (int i = x0; i <= x1; i++)
            {
                for (int j = y0; j <= y1; j++)
                {
                    if (m_Coord[i, j] == null) { return false; }
                    Vector2Int pos = (Vector2Int)m_Coord[i, j];
                    if (origins.Contains(pos)) { continue; }
                    origins.Add(pos);
                }
            }

            // 同じ色か、範囲内に収まっているかをチェック
            foreach (Vector2Int pos in origins)
            {
                Block target = m_Origins[pos.x, pos.y];
                if (target.Color != type) { return false; }
                if (pos.x < x0 || pos.y < y0) { return false; }
                if (pos.x + target.Shape.x - 1 > x1 || pos.y + target.Shape.y - 1 > y1) { return false; }
            }

            // 削除対象から原点を削除
            origins.Remove(new Vector2Int(x0, y0));
            deleteBlocks = origins;
            return true;
        }

        // posはBombが置かれた位置
        public async UniTask Explode(Vector2Int pos)
        {
            pos += Vector2Int.down;
            if (!IsValid(pos)) { return; }
            if (m_Coord[pos.x, pos.y] == null) { return; }

            Vector2Int origin = (Vector2Int)m_Coord[pos.x, pos.y];
            Color type = m_Origins[origin.x, origin.y].Color;

            // 時系列順に消す対象のまとまりが格納される
            List<List<Vector2Int>> origins = new List<List<Vector2Int>>();
            origins.Add(new List<Vector2Int> { origin });

            while (true)
            {
                var latestList = origins[origins.Count - 1];
                List<Vector2Int> addList = new List<Vector2Int>();

                foreach (var latest in latestList)
                {
                    GetNextExplosionTargets(latest);
                }

                // そのブロックと隣接していてまだ削除予定が無いブロックをリストアップしaddListに追加
                void GetNextExplosionTargets(Vector2Int origin)
                {
                    List<Vector2Int> inspectionPositions = GetTouchingBlocks(origin);

                    foreach (var target in inspectionPositions)
                    {
                        if (!IsValid(target)) { continue; }
                        if (m_Coord[target.x, target.y] == null) { continue; }
                        Vector2Int targetOrigin = (Vector2Int)m_Coord[target.x, target.y];

                        if (m_Origins[targetOrigin.x, targetOrigin.y].Color != type) continue;
                        bool isContinue = false;
                        foreach (var list in origins)
                        {
                            if (list.Contains(targetOrigin))
                            {
                                isContinue = true;
                                break;
                            }
                        }
                        if (isContinue) { continue; }
                        if (addList.Contains(targetOrigin)) { continue; }

                        addList.Add(targetOrigin);
                    }
                }

                if (addList.Count == 0) { break; }
                origins.Add(addList);
            }

            m_ScoreManager.SetVisible(true);
            List<MotionHandle> handles = new List<MotionHandle>();
            foreach (var list in origins)
            {
                int points = 0;
                int multiplier = 0;
                foreach (var target in list)
                {
                    var width = m_Origins[target.x, target.y].Shape.x;
                    var height = m_Origins[target.x, target.y].Shape.y;
                    points += m_PointSetting.PointPerTile * width * height;
                    if (width != 1 || height != 1)
                    {
                        multiplier += width * height;
                    }

                    var handle = LMotion.Create(m_Origins[target.x, target.y].transform.localScale, Vector3.zero, m_DissolveTime)
                        .WithEase(Ease.OutExpo)
                        .WithOnComplete(() =>
                        {
                            Destroy(m_Origins[target.x, target.y].gameObject);
                            Delete(target);
                        })
                        .BindToLocalScale(m_Origins[target.x, target.y].transform)
                        .AddTo(this);
                    handles.Add(handle);
                }
                await m_ScoreManager.AddScoreAddition(points);
                await m_ScoreManager.AddScoreMultiplication(multiplier);
            }

            foreach (var handle in handles)
            {
                await handle;
            }
            await DropToBottom();

            await UniTask.WaitForSeconds(0.3f);

            await m_ScoreManager.ResolveMultiplication();
            await UniTask.WaitForSeconds(0.2f);
            await m_ScoreManager.ResolveAddition();
            m_ScoreManager.SetVisible(false);
        }

        /// <summary>
        /// 斜めを含まない隣接しているブロックをリストアップ
        /// </summary>
        List<Vector2Int> GetTouchingBlocks(Vector2Int origin)
        {
            List<Vector2Int> outList = new List<Vector2Int>();
            Block block = m_Origins[origin.x, origin.y];

            for (int i = origin.x - 1; i < origin.x + block.Shape.x + 1; i++)
            {
                for (int j = origin.y - 1; j < origin.y + block.Shape.y + 1; j++)
                {
                    if (!IsValid(new Vector2Int(i, j))) { continue; }
                    if (m_Coord[i, j] == null) { continue; }
                    if (i == origin.x - 1 && j == origin.y - 1 ||
                        i == origin.x - 1 && j == origin.y + block.Shape.y ||
                        i == origin.x + block.Shape.x && j == origin.y - 1 ||
                        i == origin.x + block.Shape.x && j == origin.y + block.Shape.y) { continue; }
                    if (outList.Contains((Vector2Int)m_Coord[i, j])) { continue; }
                    outList.Add((Vector2Int)m_Coord[i, j]);
                }
            }

            return outList;
        }
    }


}
