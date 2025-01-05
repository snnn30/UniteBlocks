// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Board;
using Cysharp.Threading.Tasks;
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
            float moveAmout = Time.deltaTime / _dropDelay;
            Vector2Int targetPos = new Vector2Int(_state.Position.x, Mathf.FloorToInt(transform.localPosition.y - moveAmout));

            if (!_state.IsAcceptingInput) { return; }

            if (!_state.CanSet(targetPos, _state.Rotation))
            {
                _waitingBomb.IsBoosting = false;
                _waitingBomb.IsGaugeIncreesing = false;
                await _state.GroundingProcess();
                _waitingBomb.IsGaugeIncreesing = true;
                await StartDrop();
                return;
            }

            transform.localPosition -= new Vector3(0, moveAmout, 0);
            if (transform.localPosition.y < _state.Position.y)
            {
                _state.Position = targetPos;
            }

            return;
        }

        void OnDropPerformed(InputAction.CallbackContext context)
        {
            _waitingBomb.IsBoosting = true;
            _dropDelay = _setting.ManualDropDelay;
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
            await UniTask.WaitForSeconds(_setting.StagnationTime, cancellationToken: _cancellationTokenSource.Token);

            DropContinuous(_cancellationTokenSource.Token).Forget();
            async UniTask DropContinuous(CancellationToken token)
            {
                while (true)
                {
                    await Drop();
                    await UniTask.Yield(cancellationToken: token);
                }
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
