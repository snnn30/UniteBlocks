﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    PuyoController[] _puyoControllers = new PuyoController[2];
    [SerializeField] BoardController _boardController;
    [SerializeField] WaitingPuyos _waitingPuyos;
    bool isAcceptingInput = true;
    private List<Tween> activeTweens = new List<Tween>();

    public enum Direction
    {
        Up, Right, Down, Left,
    }

    static readonly Dictionary<Direction, Vector2Int> directionVec = new Dictionary<Direction, Vector2Int>()
    {
        [Direction.Down] = Vector2Int.down,
        [Direction.Up] = Vector2Int.up,
        [Direction.Left] = Vector2Int.left,
        [Direction.Right] = Vector2Int.right,
    };

    Vector2Int Position { get; set; }
    Direction Rotation { get; set; }


    public async void SetPosition(Direction direction, float duration)
    {
        if (!isAcceptingInput) { return; }

        var vec2 = directionVec[direction];
        var targetPos = Position + vec2;

        if (direction == Direction.Down && !CanSet(targetPos, Rotation))
        {
            isAcceptingInput = false;
            while (activeTweens.Count != 0)
            {
                await UniTask.Yield();
            }
            _boardController.Settle(Position, _puyoControllers[0]);
            _boardController.Settle(CalcChildPuyoPos(Position, Rotation), _puyoControllers[1]);
            await _boardController.DropToBottom();
            ChangeOperationPuyos();
            isAcceptingInput = true;
            return;
        }

        if (!CanSet(targetPos, Rotation)) { return; }

        Vector3 vec3 = new Vector3(vec2.x, vec2.y, 0);

        Tween tween;

        switch (direction)
        {
            case Direction.Left:
            case Direction.Right:
                tween = this.transform
                    .DOBlendableLocalMoveBy(vec3, duration)
                    .SetEase(Ease.OutQuart);
                activeTweens.Add(tween);
                tween.OnKill(() => activeTweens.Remove(tween));
                break;
            case Direction.Down:
                tween = this.transform
                    .DOBlendableLocalMoveBy(vec3, duration)
                    .SetEase(Ease.Linear);
                activeTweens.Add(tween);
                tween.OnKill(() => activeTweens.Remove(tween));
                break;
            case Direction.Up:
                return;
        }

        Position = targetPos;
        return;
    }

    public bool SetRotation(bool isRight, float duration)
    {
        if (!isAcceptingInput) return false;

        Direction targetRot;
        Vector3 value;
        if (isRight)
        {
            targetRot = (Direction)((int)(Rotation + 1) % 4);
            value = new Vector3(0, 0, -90);
        }
        else
        {
            targetRot = (Direction)((int)(Rotation + 3) % 4);
            value = new Vector3(0, 0, 90);
        }

        if (!CanSet(Position, targetRot)) { return false; }

        var tween = this.transform
            .DOBlendableLocalRotateBy(value, duration)
            .SetEase(Ease.OutQuart);
        activeTweens.Add(tween);
        tween.OnKill(() => activeTweens.Remove(tween));

        Rotation = targetRot;
        return true;
    }

    bool CanSet(Vector2Int pos, Direction rot)
    {
        if (!_boardController.CanSettle(pos)) { return false; }
        if (!_boardController.CanSettle(CalcChildPuyoPos(pos, rot))) { return false; }
        return true;
    }

    static Vector2Int CalcChildPuyoPos(Vector2Int pos, Direction rot)
    {
        return pos + directionVec[rot];
    }

    void ChangeOperationPuyos()
    {
        Rotation = Direction.Up;
        Position = new Vector2Int(2, 12);
        this.transform.localPosition = new Vector3(Position.x, Position.y, 0);

        _puyoControllers = _waitingPuyos.GetNextPuyos();
        _puyoControllers[0].transform.position = this.transform.position;
        _puyoControllers[1].transform.position = this.transform.position + Vector3.up;
        _puyoControllers[0].transform.parent = this.transform;
        _puyoControllers[1].transform.parent = this.transform;
    }

    private void Awake()
    {
        foreach (Transform n in this.transform)
        {
            GameObject.Destroy(n.gameObject);
        }
    }

    private void Start()
    {
        ChangeOperationPuyos();
    }
}
