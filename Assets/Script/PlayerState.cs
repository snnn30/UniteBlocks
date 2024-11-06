// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2];
    [SerializeField] BoardController _boardController;

    ReactiveProperty<Vector2Int> _positionRP = new ReactiveProperty<Vector2Int>();

    ReactiveProperty<RotState> _rotationRP = new ReactiveProperty<RotState>();
    public enum RotState
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

    public Vector2Int Position => _positionRP.Value;
    public RotState Rotation => _rotationRP.Value;



    public bool SetPosition(Vector2Int targetPos)
    {
        if (!CanSet(targetPos, _rotationRP.Value)) { return false; }
        _positionRP.Value = targetPos;
        return true;
    }

    public bool SetRotation(RotState targetRot)
    {
        if (!CanSet(_positionRP.Value, targetRot)) { return false; }
        _rotationRP.Value = targetRot;
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

    void PosUpdate()
    {
        _puyoControllers[0].SetPos(_positionRP.Value);
        var childPos = CalcChildPuyoPos(_positionRP.Value, _rotationRP.Value);
        _puyoControllers[1].SetPos(childPos);
    }

    private void Awake()
    {
        _rotationRP.Value = RotState.Up;
        _positionRP.Value = new Vector2Int(2, 12);

        _positionRP.Subscribe(pos =>
        {
            PosUpdate();
        });
        _rotationRP.Subscribe(rot =>
        {
            PosUpdate();
        });
    }

    private void Start()
    {
        _puyoControllers[0].PuyoType = PuyoType.Green;
        _puyoControllers[1].PuyoType = PuyoType.Red;
    }
}
