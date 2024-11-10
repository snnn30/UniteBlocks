// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

public class BoardController : MonoBehaviour
{
    const int BOARD_WIDTH = 6;
    const int BOARD_HEIGHT = 14;
    PuyoController[,] _puyos = new PuyoController[BOARD_HEIGHT, BOARD_WIDTH];

    [SerializeField] PuyoController _prefabPuyo;
    [SerializeField] GameObject _puyosContainer;

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
        return 0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT;
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



}
