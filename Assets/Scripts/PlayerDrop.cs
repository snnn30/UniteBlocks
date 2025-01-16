// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniteBlocks
{
    public class PlayerDrop : MonoBehaviour
    {
        [SerializeField]
        WaitingBomb m_WaitingBomb;

        [SerializeField]
        GameManager m_GameManager;

        float m_DropDelay;
        PlayerInput m_Input;
        PlayerState m_State;
        CancellationTokenSource m_CancellationTokenSource;
        PlayerSetting m_Setting;

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_State = GetComponent<PlayerState>();
            m_Setting = m_State.PlayerSetting;
            m_DropDelay = m_Setting.AutoDropDelay;
        }

        private void Start()
        {
            StartDrop().Forget();
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
            m_Input.actions["Drop"].performed += OnDropPerformed;
            m_Input.actions["Drop"].canceled += OnDropCanceled;
        }

        private void OnDisable()
        {
            m_Input.actions["Drop"].performed -= OnDropPerformed;
            m_Input.actions["Drop"].canceled -= OnDropCanceled;
        }

        async UniTask Drop()
        {
            Vector2Int targetPos = m_State.Position + Vector2Int.down;

            if (!m_State.IsAcceptingInput) { return; }

            if (!m_State.CanSet(targetPos, m_State.Rotation))
            {
                m_GameManager.IsGaugeIncreasing = false;
                await m_State.GroundingProcess();
                await StartDrop();
                return;
            }

            Vector3 vec3 = Vector3.down;
            Tween tween = this.transform
                .DOBlendableLocalMoveBy(vec3, m_DropDelay / 5)
                .SetEase(Ease.InOutQuad);
            m_State.ActiveTweens.Add(tween);
            _ = tween.OnKill(() => m_State.ActiveTweens.Remove(tween));

            m_State.Position = targetPos;
        }

        void OnDropPerformed(InputAction.CallbackContext context)
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
                m_CancellationTokenSource = null;
            }
            m_CancellationTokenSource = new CancellationTokenSource();

            if (m_State.IsAcceptingInput) { m_GameManager.IsGaugeIncreasing = true; }
            m_WaitingBomb.IsBoosting = true;
            m_DropDelay = m_Setting.ManualDropDelay;

            DropContinuous(m_CancellationTokenSource.Token).Forget();
        }

        void OnDropCanceled(InputAction.CallbackContext context)
        {
            m_WaitingBomb.IsBoosting = false;
            m_DropDelay = m_Setting.AutoDropDelay;
        }

        async UniTask StartDrop()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
                m_CancellationTokenSource = null;
            }
            m_CancellationTokenSource = new CancellationTokenSource();
            m_GameManager.IsGaugeIncreasing = false;

            await UniTask.WaitForSeconds(m_Setting.StagnationTime, cancellationToken: m_CancellationTokenSource.Token);

            if (m_State.IsAcceptingInput) { m_GameManager.IsGaugeIncreasing = true; }
            DropContinuous(m_CancellationTokenSource.Token).Forget();
        }

        async UniTask DropContinuous(CancellationToken token)
        {
            while (true)
            {
                await Drop();
                await UniTask.WaitForSeconds(m_DropDelay, cancellationToken: token);
            }
        }
    }
}
