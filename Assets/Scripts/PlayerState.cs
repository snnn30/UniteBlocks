// Licensed to the .NET Foundation under one or more agreements.
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
    [SerializeField] GameManager _gameManager;

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

    public Vector2Int Position { get; set; }
    public Direction Rotation { get; set; }
    public bool IsAcceptingInput { get; private set; } = true;
    public List<Tween> ActiveTweens { get; set; } = new List<Tween>();
    public PuyoController[] PuyoControllers => _puyoControllers;

    public async UniTask GroundingProcess()
    {
        IsAcceptingInput = false;
        while (ActiveTweens.Count != 0)
        {
            await UniTask.Yield();
        }

        var childPos = CalcChildPuyoPos(Position, Rotation);
        if ((Position.x == BoardController.START_POS.x && Position.y == BoardController.MAX_HEIGHT)
            || (childPos.x == BoardController.START_POS.x && childPos.y == BoardController.MAX_HEIGHT))
        {
            _gameManager.OnGameOver();
        }

        // PlayerDrop.Drop()で発生する若干のずれを修正
        transform.localPosition = new Vector3(Position.x, Position.y, 0);

        _boardController.Settle(Position, _puyoControllers[0]);
        _boardController.Settle(CalcChildPuyoPos(Position, Rotation), _puyoControllers[1]);
        await _boardController.DropToBottom();
        ChangeOperationPuyos();
        IsAcceptingInput = true;

        return;
    }

    public bool CanSet(Vector2Int pos, Direction rot)
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
        var initialPos = BoardController.START_POS;
        Position = initialPos;
        this.transform.localPosition = new Vector3(Position.x, Position.y, 0);

        _puyoControllers = _waitingPuyos.GetNextPuyos();
        _puyoControllers[0].transform.position = this.transform.position;
        _puyoControllers[1].transform.position = this.transform.position + Vector3.up;
        _puyoControllers[0].transform.parent = this.transform;
        _puyoControllers[1].transform.parent = this.transform;
    }



    private void Start()
    {
        ChangeOperationPuyos();
    }
}
