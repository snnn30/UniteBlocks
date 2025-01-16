// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniteBlocks
{
    public class PlayerMove : MonoBehaviour
    {
        private PlayerInput m_Input;
        private PlayerState m_State;
        private CancellationTokenSource m_CancellationTokenSource;
        private PlayerSetting _setting;

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_State = GetComponent<PlayerState>();
            _setting = m_State.PlayerSetting;
        }

        private void OnDestroy()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        private void OnEnable()
        {
            m_Input.actions["Move"].started += OnMoveStarted;
            m_Input.actions["Move"].performed += OnMovePerformed;
            m_Input.actions["Move"].canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            m_Input.actions["Move"].started -= OnMoveStarted;
            m_Input.actions["Move"].performed -= OnMovePerformed;
            m_Input.actions["Move"].canceled -= OnMoveCanceled;
        }

        void Move(float value)
        {
            var direction = (value < 0) ? Vector2Int.left : Vector2Int.right;
            var targetPos = m_State.Position + direction;

            if (!m_State.IsAcceptingInput) { return; }
            if (!m_State.CanSet(targetPos, m_State.Rotation)) { return; }


            Vector3 vec3 = new Vector3(direction.x, direction.y, 0);

            Tween tween = this.transform
                .DOBlendableLocalMoveBy(vec3, _setting.MoveDelay)
                .SetEase(Ease.OutQuart);
            m_State.ActiveTweens.Add(tween);
            tween.OnKill(() => m_State.ActiveTweens.Remove(tween));

            m_State.Position = targetPos;
            return;
        }

        void OnMoveStarted(InputAction.CallbackContext context)
        {
            Move(context.ReadValue<float>());
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            m_CancellationTokenSource = new CancellationTokenSource();
            MoveContinuous(m_CancellationTokenSource.Token).Forget();
            async UniTask MoveContinuous(CancellationToken token)
            {
                while (true)
                {
                    Move(context.ReadValue<float>());
                    await UniTask.Delay(TimeSpan.FromSeconds(_setting.MoveDelay), cancellationToken: token);
                }
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
                m_CancellationTokenSource = null;
            }

        }
    }
}
