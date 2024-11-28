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
    public static readonly Vector2Int START_POS = new Vector2Int(2, 13);
    public static readonly int MAX_HEIGHT = 11;
    PuyoController[,] _puyos = new PuyoController[BOARD_HEIGHT, BOARD_WIDTH];

    [SerializeField] PuyoController _prefabPuyo;
    [SerializeField] GameObject _puyosContainer;
    [SerializeField] float _dropSpeed = 0.5f;

    void ClearAll()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_puyos[y, x] != null)
                {
                    Destroy(_puyos[y, x]);
                }
                _puyos[y, x] = null;
            }
        }
    }

    private void Awake()
    {
        ClearAll();
    }

    static bool IsValidated(Vector2Int pos)
    {
        if (!(0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT))
        {
            return false;
        }

        if (pos.x != START_POS.x && pos.y > MAX_HEIGHT)
        {
            return false;
        }

        return true;
    }

    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValidated(pos))
            return false;
        return _puyos[pos.y, pos.x] == null;
    }

    public bool Settle(Vector2Int pos, PuyoType puyoType)
    {
        if (!CanSettle(pos))
            return false;

        Vector3 worldPos = transform.position + new Vector3(pos.x, pos.y, 0f);
        _puyos[pos.y, pos.x] = Instantiate(_prefabPuyo, worldPos, transform.rotation, _puyosContainer.transform);
        _puyos[pos.y, pos.x].PuyoType = puyoType;

        return true;
    }

    public bool Settle(Vector2Int pos, PuyoController puyoController)
    {
        if (!CanSettle(pos))
            return false;

        puyoController.transform.parent = _puyosContainer.transform;

        Vector3 worldPos = transform.position + new Vector3(pos.x, pos.y, 0f);
        _puyos[pos.y, pos.x] = puyoController;

        return true;
    }

    public async UniTask DropToBottom()
    {
        List<Tween> activeTweens = new List<Tween>();
        PuyoController[,] puyos = new PuyoController[BOARD_HEIGHT, BOARD_WIDTH];

        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_puyos[y, x] == null) continue;

                int p = y;
                for (int i = y; i >= 0; i--)
                {
                    if (_puyos[i, x] == null)
                    {
                        p = i;
                    }
                }

                if (p == y)
                {
                    puyos[p, x] = _puyos[y, x];
                    continue;
                }

                var tween = _puyos[y, x].transform
                    .DOLocalMoveY(p, _dropSpeed)
                    .SetEase(Ease.OutBounce);
                activeTweens.Add(tween);
                tween.OnKill(() => activeTweens.Remove(tween));

                puyos[p, x] = _puyos[y, x];

            }
        }

        _puyos = puyos;

        while (activeTweens.Count != 0)
        {
            await UniTask.Yield();
        }

        return;
    }



}
