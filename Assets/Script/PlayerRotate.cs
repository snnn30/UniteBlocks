// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] float _rotateDelay = 0.12f;

    PlayerInput _input;
    PlayerState _state;
    CancellationTokenSource _cancellationTokenSource;

    bool Rotate(float value)
    {
        var isRight = (value < 0) ? false : true;
        // var intVal = (value < 0) ? 3 : 1;
        // RotState rot = (RotState)((int)(_state.Rotation + intVal) % 4);
        return _state.SetRotation(isRight, _rotateDelay);
    }

    void OnRotateStarted(InputAction.CallbackContext callbackContext)
    {
        Rotate(callbackContext.ReadValue<float>());
    }

    void OnRotatePerformed(InputAction.CallbackContext context)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        RotateContinuous(_cancellationTokenSource.Token).Forget();
        async UniTask RotateContinuous(CancellationToken token)
        {
            while (true)
            {
                Rotate(context.ReadValue<float>());
                await UniTask.Delay(TimeSpan.FromSeconds(_rotateDelay), cancellationToken: token);
            }
        }
    }

    void OnRotateCanceled(InputAction.CallbackContext context)
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
        _input.actions["Rotate"].started += OnRotateStarted;
        _input.actions["Rotate"].performed += OnRotatePerformed;
        _input.actions["Rotate"].canceled += OnRotateCanceled;
    }

    private void OnDisable()
    {
        _input.actions["Rotate"].started -= OnRotateStarted;
        _input.actions["Rotate"].performed -= OnRotatePerformed;
        _input.actions["Rotate"].canceled -= OnRotateCanceled;
    }
}
