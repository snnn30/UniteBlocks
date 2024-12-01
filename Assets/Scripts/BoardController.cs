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
    PuyoController[,] _origins = new PuyoController[BOARD_WIDTH, BOARD_HEIGHT];

    [SerializeField] PuyoController _prefabPuyo;
    [SerializeField] GameObject _puyosContainer;
    [SerializeField] float _dropSpeed = 0.5f;
    [SerializeField] GameManager _gameManager;

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
    public bool CanSettle(Vector2Int pos, PuyoController puyo)
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
    public void Settle(Vector2Int pos, PuyoController puyo)
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

        Combine();

        return CheckGameOver();
    }

    bool CheckGameOver()
    {
        if (_coord[START_POS.x, START_POS.y] == null) { return false; }
        _gameManager.OnGameOver();
        return true;
    }

    void Combine()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_origins[x, y] == null) { continue; }
                var puyo = _origins[x, y];

                if (puyo.Shape == Vector2Int.one)
                {
                    var x1 = x + 1;
                    var y1 = y + 1;
                    Vector2Int[] targets = new Vector2Int[3];
                    targets[0] = new Vector2Int(x1, y);
                    targets[1] = new Vector2Int(x, y1);
                    targets[2] = new Vector2Int(x1, y1);

                    if (x1 == BOARD_WIDTH || y1 == BOARD_HEIGHT)
                    {
                        continue;
                    }

                    bool canCombine = true;
                    foreach (var target in targets)
                    {
                        if (_origins[target.x, target.y] == null
                            || _origins[target.x, target.y].Shape != Vector2Int.one
                            || _origins[target.x, target.y].PuyoType != puyo.PuyoType)
                        {
                            canCombine = false;
                            break;
                        }
                    }
                    if (!canCombine) { continue; }

                    foreach (var target in targets)
                    {
                        Destroy(_origins[target.x, target.y].gameObject);
                        Delete(new Vector2Int(target.x, target.y));
                        _coord[target.x, target.y] = new Vector2Int(x, y);
                    }
                    puyo.Shape = new Vector2Int(2, 2);

                }


                /*
                // 一個右の列から検査 i,jは_puyos上の座標
                for (int i = x + puyo.Shape.x; i < BOARD_WIDTH; i++)
                {
                    List<Vector2Int> deletePuyos = new List<Vector2Int>();
                    bool skip = false;

                    for (int j = y; j < y + puyo.Shape.y; j++)
                    {
                        deletePuyos.Add(new Vector2Int(i, j));
                        if (_puyos[i, j] == null
                            || _puyos[i, j].PuyoType != puyo.PuyoType
                            || _puyos[i, j] == puyo)
                        {
                            skip = true;
                        }
                    }
                    if (skip) { break; }

                    foreach (Vector2Int delPos in deletePuyos)
                    {
                        Destroy(_puyos[delPos.x, delPos.y]);
                        _puyos[delPos.x, delPos.y] = _puyos[x, y];
                    }
                    puyo.Shape = new Vector2Int(puyo.Shape.x + 1, puyo.Shape.y);
                }
                */
            }
        }
    }

}
