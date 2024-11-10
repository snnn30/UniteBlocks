// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2];
    [SerializeField] BoardController _boardController;

    enum RotState
    {
        Up, Right, Down, Left,
    }

    static readonly Dictionary<RotState, Vector2Int> rotateDic = new Dictionary<RotState, Vector2Int>()
    {
        [RotState.Down] = Vector2Int.down,
        [RotState.Up] = Vector2Int.up,
        [RotState.Left] = Vector2Int.left,
        [RotState.Right] = Vector2Int.right,
    };

    Vector2Int Position { get; set; }
    RotState Rotation { get; set; }


    public bool SetPosition(Vector2Int direction, float duration)
    {
        if (!(direction.x == 0 ^ direction.y == 0)) return false;

        var targetPos = Position + direction;

        if (!CanSet(targetPos, Rotation)) { return false; }

        Vector3 relativeParentVec = new Vector3(direction.x, direction.y, 0);

        this.transform.DOBlendableLocalMoveBy(relativeParentVec, duration);

        Position = targetPos;
        return true;
    }

    public bool SetRotation(bool isRight, float duration)
    {
        RotState targetRot;
        Vector3 value;
        if (isRight)
        {
            targetRot = (RotState)((int)(Rotation + 1) % 4);
            value = new Vector3(0, 0, -90);
        }
        else
        {
            targetRot = (RotState)((int)(Rotation + 3) % 4);
            value = new Vector3(0, 0, 90);
        }

        if (!CanSet(Position, targetRot)) { return false; }
        this.transform.DOBlendableLocalRotateBy(value, duration);

        Rotation = targetRot;
        return true;
    }

    bool CanSet(Vector2Int pos, RotState rot)
    {
        if (!_boardController.CanSettle(pos)) { return false; }
        if (!_boardController.CanSettle(CalcChildPuyoPos(pos, rot))) { return false; }
        return true;
    }

    static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
    {
        return pos + rotateDic[rot];
    }

    private void Awake()
    {
        Rotation = RotState.Up;
        Position = new Vector2Int(2, 12);
        this.transform.localPosition = new Vector3(Position.x, Position.y, 0);
        var childPos = CalcChildPuyoPos(Position, Rotation);
        _puyoControllers[1].transform.localPosition = new Vector3(0, 1, 0);
    }

    private void Start()
    {
        _puyoControllers[0].PuyoType = PuyoType.Green;
        _puyoControllers[1].PuyoType = PuyoType.Red;
    }
}
