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
    CancellationTokenSource _cancellationTokenSource;


    Vector2Int CalcMovePos(float direction)
    {
        var normalizedDir = (direction < 0) ? Vector2Int.left : Vector2Int.right;
        return _state.Position + normalizedDir;
    }

    bool Move(float direction)
    {
        return _state.SetPosition(CalcMovePos(direction));
    }



    void OnMoveStarted(InputAction.CallbackContext context)
    {
        Move(context.ReadValue<float>());
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        MoveContinuous(_cancellationTokenSource.Token).Forget();
        async UniTask MoveContinuous(CancellationToken token)
        {
            while (true)
            {
                Move(context.ReadValue<float>());
                await UniTask.Delay(TimeSpan.FromSeconds(_moveDelay), cancellationToken: token);
            }
        }
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
        }

    }



    private void OnDestroy()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
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
