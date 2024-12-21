// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerRotate : MonoBehaviour
    {
        [SerializeField] float _rotateDelay = 0.12f;

        PlayerInput _input;
        PlayerState _state;
        CancellationTokenSource _cancellationTokenSource;


        void Rotate(float value)
        {
            if (_state.IsBomb) { return; }

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
            var parentPuyo = (Puyo)_state.Items[0];
            var childPuyo = (Puyo)_state.Items[1];

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
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

        }

        private void OnDestroy()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
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
}
