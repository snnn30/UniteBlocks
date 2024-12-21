// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    Item[] _items = new Item[2];
    bool _isBomb;
    [SerializeField] BoardController _boardController;
    [SerializeField] WaitingItems _waitingItems;

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
    public Item[] Items => _items;
    public bool IsBomb => _isBomb;

    public async UniTask GroundingProcess()
    {
        IsAcceptingInput = false;
        while (ActiveTweens.Count != 0)
        {
            await UniTask.Yield();
        }

        // Drop()で発生する若干のずれを修正
        transform.localPosition = new Vector3(Position.x, Position.y, 0);

        if (IsBomb) { return; }

        _boardController.Settle(Position, (Puyo)_items[0]);
        _boardController.Settle(CalcChildPuyoPos(Position, Rotation), (Puyo)_items[1]);

        bool gameOver = await _boardController.DropToBottom();
        if (gameOver) { return; }

        ChangeOperationPuyos();
        IsAcceptingInput = true;

        return;
    }

    public bool CanSet(Vector2Int pos, Direction rot)
    {
        if (!_boardController.CanSettle(pos)) { return false; }
        if (IsBomb) { return true; }
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

        (_items, _isBomb) = _waitingItems.GetNextItems();
        if (_isBomb)
        {
            Bomb bomb = ((Bomb)_items[0]);
            bomb.transform.position = this.transform.position;
            bomb.transform.parent = this.transform;
        }
        else
        {
            Puyo parent = (Puyo)_items[0];
            Puyo child = (Puyo)_items[1];
            parent.transform.position = this.transform.position;
            child.transform.position = this.transform.position + Vector3.up;
            parent.transform.parent = this.transform;
            child.transform.parent = this.transform;
        }
    }



    private void Start()
    {
        ChangeOperationPuyos();
    }
}
