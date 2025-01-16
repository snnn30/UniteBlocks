// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniteBlocks
{
    public class PlayerRotate : MonoBehaviour
    {
        PlayerInput m_Input;
        PlayerState m_State;
        CancellationTokenSource m_CancellationTokenSource;
        PlayerSetting m_Setting;

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_State = GetComponent<PlayerState>();
            m_Setting = m_State.PlayerSetting;
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
            m_Input.actions["Rotate"].performed += OnRotatePerformed;
            m_Input.actions["Rotate"].canceled += OnRotateCanceled;
        }

        private void OnDisable()
        {
            m_Input.actions["Rotate"].performed -= OnRotatePerformed;
            m_Input.actions["Rotate"].canceled -= OnRotateCanceled;
        }

        bool Rotate(float value)
        {
            if (m_State.IsBomb) { return true; }

            var isRight = (value < 0) ? false : true;
            Direction targetRot;
            float targetAmount;

            if (isRight)
            {
                targetRot = (Direction)((int)(m_State.Rotation + 1) % 4);
                targetAmount = -90;
            }
            else
            {
                targetRot = (Direction)((int)(m_State.Rotation + 3) % 4);
                targetAmount = 90;
            }

            if (!m_State.IsAcceptingInput) { return false; }
            if (!m_State.CanSet(m_State.Position, targetRot)) { return false; }

            float currentAngle = 0;
            var parentPuyo = (Block)m_State.Items[0];
            var childPuyo = (Block)m_State.Items[1];

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
                m_Setting.RotateDelay
                ).SetEase(Ease.OutQuad);

            m_State.ActiveTweens.Add(tween);
            tween.OnKill(() => m_State.ActiveTweens.Remove(tween));

            m_State.Rotation = targetRot;
            return true;
        }

        void OnRotatePerformed(InputAction.CallbackContext context)
        {
            m_CancellationTokenSource = new CancellationTokenSource();
            RotateContinuous(m_CancellationTokenSource.Token).Forget();
            async UniTask RotateContinuous(CancellationToken token)
            {
                while (true)
                {
                    if (Rotate(context.ReadValue<float>())) { break; }
                    await UniTask.Yield(cancellationToken: token);
                }
            }
        }

        void OnRotateCanceled(InputAction.CallbackContext context)
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
