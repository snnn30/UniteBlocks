// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerState;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] float _rotateDelay = 0.12f;

    PlayerInput _input;
    PlayerState _state;
    CancellationTokenSource _cancellationTokenSource;


    void Rotate(float value)
    {
        var isRight = (value < 0) ? false : true;
        Direction targetRot;
        float targetAmount;

        if (isRight)
        {
            targetRot = (Direction)((int)(_state.Rotation + 1) % 4);
            targetAmount = -90;
        }
        else
        {
            targetRot = (Direction)((int)(_state.Rotation + 3) % 4);
            targetAmount = 90;
        }

        if (!_state.IsAcceptingInput) { return; }
        if (!_state.CanSet(_state.Position, targetRot)) { return; }

        float currentAngle = 0;
        var parentPuyo = _state.PuyoControllers[0];
        var childPuyo = _state.PuyoControllers[1];

        var tween = DOTween.To(
            () => 0f,
            x =>
            {
                var rotateAmount = x - currentAngle;
                var quaternion = Quaternion.AngleAxis(rotateAmount, Vector3.forward);
                childPuyo.transform.localPosition -= parentPuyo.transform.localPosition;
                childPuyo.transform.localPosition = quaternion * childPuyo.transform.localPosition;
                childPuyo.transform.localPosition += parentPuyo.transform.localPosition;
                currentAngle = x;
            },
            targetAmount,
            _rotateDelay
            ).SetEase(Ease.OutQuad);

        _state.ActiveTweens.Add(tween);
        tween.OnKill(() => _state.ActiveTweens.Remove(tween));

        _state.Rotation = targetRot;
        return;
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
