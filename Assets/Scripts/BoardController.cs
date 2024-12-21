// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    const int BOARD_WIDTH = 6;
    const int BOARD_HEIGHT = 14;
    public static readonly Vector2Int START_POS = new Vector2Int(2, 12);
    // public static readonly int MAX_HEIGHT = 11;
    Vector2Int?[,] _coord = new Vector2Int?[BOARD_WIDTH, BOARD_HEIGHT];
    Puyo[,] _origins = new Puyo[BOARD_WIDTH, BOARD_HEIGHT];

    [SerializeField] Puyo _prefabPuyo;
    [SerializeField] GameObject _puyosContainer;
    [SerializeField] GameManager _gameManager;
    [SerializeField] float _dropSpeed = 0.5f;
    [SerializeField] float _rotateTime = 1;


    void ClearAll()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                _coord[x, y] = null;
                if (_origins == null) { continue; }
                Destroy(_origins[x, y]);
                _origins[x, y] = null;
            }
        }
    }

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
    public bool CanSettle(Vector2Int pos, Puyo puyo)
    {
        for (int x = pos.x; x < pos.x + puyo.Shape.x; x++)
        {
            for (int y = pos.y; y < pos.y + puyo.Shape.y; y++)
            {
                var target = new Vector2Int(x, y);
                if (!IsValid(target)) { return false; }
                if (_coord[x, y] != null) { return false; }
            }
        }
        return true;
    }
    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValid(pos)) { return false; }
        if (_coord[pos.x, pos.y] != null) { return false; }
        return true;
    }

    /// <summary>
    /// アイテムを置く
    /// </summary>
    public void Settle(Vector2Int pos, Puyo puyo)
    {
        if (!CanSettle(pos, puyo))
        {
            Debug.LogError("セット先が無効");
            return;
        }

        puyo.transform.parent = _puyosContainer.transform;
        _origins[pos.x, pos.y] = puyo;

        for (int x = pos.x; x < pos.x + puyo.Shape.x; x++)
        {
            for (int y = pos.y; y < pos.y + puyo.Shape.y; y++)
            {
                _coord[x, y] = pos;
            }
        }
    }

    /// <summary>
    /// アイテムを削除
    /// </summary>
    void Delete(Vector2Int pos)
    {
        if (_origins[pos.x, pos.y] == null)
        {
            Debug.LogError("削除対象が存在しない");
            return;
        }

        var puyo = _origins[pos.x, pos.y];

        for (int x = pos.x; x < pos.x + puyo.Shape.x; x++)
        {
            for (int y = pos.y; y < pos.y + puyo.Shape.y; y++)
            {
                _coord[x, y] = null;
            }
        }

        _origins[pos.x, pos.y] = null;
    }

    /// <summary>
    /// ぷよが着地した後の諸々の処理
    /// </summary>
    /// <returns>ゲームオーバーしたかどうか</returns>
    public async UniTask<bool> DropToBottom()
    {
        List<Tween> activeTweens = new List<Tween>();

        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_origins[x, y] == null) { continue; }

                var puyo = _origins[x, y];
                int targetHeight = y;

                // 先に自身を消す事で自身に干渉しないようにする
                Delete(new Vector2Int(x, y));

                // 下の行を検査
                for (int j = y - 1; j >= 0; j--)
                {
                    if (!CanSettle(new Vector2Int(x, j), puyo)) { break; }
                    targetHeight--;
                }

                Settle(new Vector2Int(x, targetHeight), puyo);

                if (targetHeight == y) { continue; }

                var tween = puyo.transform
                    .DOLocalMoveY(targetHeight, _dropSpeed)
                    .SetEase(Ease.OutBounce);
                activeTweens.Add(tween);
                tween.OnKill(() => activeTweens.Remove(tween));
            }
        }

        while (activeTweens.Count != 0)
        {
            await UniTask.Yield();
        }

        await Combine();

        return CheckGameOver();
    }

    bool CheckGameOver()
    {
        if (_coord[START_POS.x, START_POS.y] == null) { return false; }
        _gameManager.OnGameOver();
        return true;
    }

    async UniTask Combine()
    {
        List<Tween> tweens = new List<Tween>();

        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                if (_origins[x, y] == null) { continue; }
                var puyo = _origins[x, y];

                Vector2Int targetShape = puyo.Shape;
                List<Vector2Int> deletePuyos = new List<Vector2Int>();

                // x,yを左下、i,jを右上の座標としたときの長方形を考える
                for (int i = x + puyo.Shape.x - 1; i < BOARD_WIDTH; i++)
                {
                    for (int j = y + puyo.Shape.y - 1; j < BOARD_HEIGHT; j++)
                    {
                        if (!CheckInRange(x, y, i, j, ref deletePuyos)) { continue; }
                        targetShape = new Vector2Int(i - x + 1, j - y + 1);
                    }
                }

                if (targetShape.x < 2 || targetShape.y < 2) { continue; }
                if (targetShape == puyo.Shape) { continue; }

                foreach (var pos in deletePuyos)
                {
                    Destroy(_origins[pos.x, pos.y].gameObject);
                    Delete(pos);
                }

                puyo.Shape = targetShape;
                for (int i = x; i < x + targetShape.x; i++)
                {
                    for (int j = y; j < y + targetShape.y; j++)
                    {
                        _coord[i, j] = new Vector2Int(x, y);
                    }
                }

                Vector3 center = new Vector3(x + (float)puyo.Shape.x / 2, y + (float)puyo.Shape.y / 2, 0);
                center += transform.position;
                float prex = 0f;

                var tween = DOTween.To(
                    () => 0f,
                    x =>
                    {
                        var y = x - prex;
                        puyo.gameObject.transform.RotateAround(center, Vector3.up, y);
                        prex = x;
                    },
                    360f,
                    _rotateTime
                    ).SetEase(Ease.OutBack);
                tweens.Add(tween);
                tween.OnKill(() => tweens.Remove(tween));



                // その範囲内がぷよで埋まっており、
                // その範囲内のぷよが全部x0,y0のぷよと同じタイプであり、
                // その範囲からはみ出ていなければtrue
                // 高さか幅が1ならパス　通すとdeletePuyosが変化してしまう
                bool CheckInRange(int x0, int y0, int x1, int y1, ref List<Vector2Int> deletePuyos)
                {
                    if (x0 == x1 || y0 == y1) { return false; }
                    PuyoType type = _origins[x0, y0].PuyoType;
                    List<Vector2Int> origins = new List<Vector2Int>();

                    for (int i = x0; i <= x1; i++)
                    {
                        for (int j = y0; j <= y1; j++)
                        {
                            if (_coord[i, j] == null) { return false; }
                            Vector2Int pos = (Vector2Int)_coord[i, j];
                            if (origins.Contains(pos)) { continue; }
                            origins.Add(pos);
                        }
                    }

                    foreach (Vector2Int pos in origins)
                    {
                        Puyo target = _origins[pos.x, pos.y];
                        if (target.PuyoType != type) { return false; }
                        if (pos.x < x0 || pos.y < y0) { return false; }
                        if (pos.x + target.Shape.x - 1 > x1 || pos.y + target.Shape.y - 1 > y1) { return false; }
                    }

                    origins.Remove(new Vector2Int(x0, y0));
                    deletePuyos = origins;
                    return true;
                }


            }
        }

        while (tweens.Count != 0)
        {
            await UniTask.Yield();
        }
    }

}
