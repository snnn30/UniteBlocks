// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMove : MonoBehaviour
    {
        [SerializeField] float _moveDelay = 0.12f;

        PlayerInput _input;
        PlayerState _state;
        CancellationTokenSource _moveCTS;


        void Move(float value)
        {
            var direction = (value < 0) ? Vector2Int.left : Vector2Int.right;
            var targetPos = _state.Position + direction;

            if (!_state.IsAcceptingInput) { return; }
            if (!_state.CanSet(targetPos, _state.Rotation)) { return; }


            Vector3 vec3 = new Vector3(direction.x, direction.y, 0);

            Tween tween = this.transform
                .DOBlendableLocalMoveBy(vec3, _moveDelay)
                .SetEase(Ease.OutQuart);
            _state.ActiveTweens.Add(tween);
            tween.OnKill(() => _state.ActiveTweens.Remove(tween));

            _state.Position = targetPos;
            return;
        }

        void OnMoveStarted(InputAction.CallbackContext context)
        {
            Move(context.ReadValue<float>());
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveCTS = new CancellationTokenSource();
            MoveContinuous(_moveCTS.Token).Forget();
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
}
