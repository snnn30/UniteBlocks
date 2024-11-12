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

    PlayerInput _input;
    PlayerState _state;
    CancellationTokenSource _moveCTS;


    void Move(float value, float duration)
    {
        var isRight = (value < 0) ? false : true;
        _state.ShiftX(isRight, duration);
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
    }

    private void OnEnable()
    {
        _input.actions["Move"].started += OnMoveStarted;
        _input.actions["Move"].performed += OnMovePerformed;
        _input.actions["Move"].canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        _input.actions["Move"].started -= OnMoveStarted;
        _input.actions["Move"].performed -= OnMovePerformed;
        _input.actions["Move"].canceled -= OnMoveCanceled;
    }
}
