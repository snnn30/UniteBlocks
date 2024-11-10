// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] float _moveDelay = 0.12f;
    [SerializeField] float _autoDropDelay = 0.36f;
    [SerializeField] float _manualDropDelay = 0.24f;

    float _dropDelay;
    PlayerInput _input;
    PlayerState _state;
    CancellationTokenSource _moveCTS;


    Vector2Int CalcMovePos(float direction)
    {
        var normalizedDir = (direction < 0) ? Vector2Int.left : Vector2Int.right;
        return _state.Position + normalizedDir;
    }

    bool Move(float direction, float duration)
    {
        return _state.SetPosition(CalcMovePos(direction), duration);
    }

    void OnMoveStarted(InputAction.CallbackContext context)
    {
        Move(context.ReadValue<float>(), _moveDelay);
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveCTS = new CancellationTokenSource();
        MoveContinuous(_moveCTS.Token).Forget();
        async UniTask MoveContinuous(CancellationToken token)
        {
            while (true)
            {
                Move(context.ReadValue<float>(), _moveDelay);
                await UniTask.Delay(TimeSpan.FromSeconds(_moveDelay), cancellationToken: token);
            }
        }
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (_moveCTS != null)
        {
            _moveCTS.Cancel();
            _moveCTS.Dispose();
            _moveCTS = null;
        }

    }


    bool Drop()
    {
        return _state.SetPosition(_state.Position + Vector2Int.down, _dropDelay);
    }

    void StartAutoDrop()
    {
        _dropDelay = _autoDropDelay;
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        DropContinuous(token).Forget();
        async UniTask DropContinuous(CancellationToken token)
        {
            while (true)
            {
                Drop();
                await UniTask.Delay(TimeSpan.FromSeconds(_dropDelay), cancellationToken: token);
            }
        }
    }

    void OnDropPerformed(InputAction.CallbackContext context)
    {
        _dropDelay = _manualDropDelay;
    }

    void OnDropCanceled(InputAction.CallbackContext context)
    {
        _dropDelay = _autoDropDelay;
    }


    private void OnDestroy()
    {
        if (_moveCTS != null)
        {
            _moveCTS.Cancel();
            _moveCTS.Dispose();
            _moveCTS = null;
        }
    }

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _state = GetComponent<PlayerState>();
        StartAutoDrop();
    }

    private void OnEnable()
    {
        _input.actions["Move"].started += OnMoveStarted;
        _input.actions["Move"].performed += OnMovePerformed;
        _input.actions["Move"].canceled += OnMoveCanceled;

        _input.actions["Drop"].performed += OnDropPerformed;
        _input.actions["Drop"].canceled += OnDropCanceled;
    }

    private void OnDisable()
    {
        _input.actions["Move"].started -= OnMoveStarted;
        _input.actions["Move"].performed -= OnMovePerformed;
        _input.actions["Move"].canceled -= OnMoveCanceled;

        _input.actions["Drop"].performed -= OnDropPerformed;
        _input.actions["Drop"].canceled -= OnDropCanceled;
    }
}
