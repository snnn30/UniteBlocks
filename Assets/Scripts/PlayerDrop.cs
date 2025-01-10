// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Board;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerDrop : MonoBehaviour
    {
        [SerializeField] WaitingBomb _waitingBomb;
        float _dropDelay;
        PlayerInput _input;
        PlayerState _state;
        CancellationTokenSource _cancellationTokenSource;
        PlayerSetting _setting;


        async UniTask Drop()
        {
            Vector2Int targetPos = _state.Position + Vector2Int.down;

            if (!_state.IsAcceptingInput) { return; }

            if (!_state.CanSet(targetPos, _state.Rotation))
            {
                _waitingBomb.IsGaugeIncreasing = false;
                await _state.GroundingProcess();
                await StartDrop();
                return;
            }

            Vector3 vec3 = Vector3.down;
            Tween tween = this.transform
                .DOBlendableLocalMoveBy(vec3, _dropDelay / 5)
                .SetEase(Ease.InOutQuad);
            _state.ActiveTweens.Add(tween);
            _ = tween.OnKill(() => _state.ActiveTweens.Remove(tween));

            _state.Position = targetPos;
        }

        void OnDropPerformed(InputAction.CallbackContext context)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _cancellationTokenSource = new CancellationTokenSource();

            _waitingBomb.IsBoosting = true;
            _dropDelay = _setting.ManualDropDelay;

            DropContinuous(_cancellationTokenSource.Token).Forget();
        }

        void OnDropCanceled(InputAction.CallbackContext context)
        {
            _waitingBomb.IsBoosting = false;
            _dropDelay = _setting.AutoDropDelay;
        }


        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _state = GetComponent<PlayerState>();
            _setting = _state.PlayerSetting;
            _dropDelay = _setting.AutoDropDelay;
        }

        async UniTask StartDrop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _waitingBomb.IsGaugeIncreasing = false;
            try
            {
                await UniTask.WaitForSeconds(_setting.StagnationTime, cancellationToken: _cancellationTokenSource.Token);
            }
            finally
            {
                _waitingBomb.IsGaugeIncreasing = true;
            }
            DropContinuous(_cancellationTokenSource.Token).Forget();
        }

        async UniTask DropContinuous(CancellationToken token)
        {
            while (true)
            {
                await Drop();
                await UniTask.WaitForSeconds(_dropDelay, cancellationToken: token);
            }
        }

        private void Start()
        {
            StartDrop().Forget();
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

        private void OnEnable()
        {
            _input.actions["Drop"].performed += OnDropPerformed;
            _input.actions["Drop"].canceled += OnDropCanceled;
        }

        private void OnDisable()
        {
            _input.actions["Drop"].performed -= OnDropPerformed;
            _input.actions["Drop"].canceled -= OnDropCanceled;
        }
    }
}
