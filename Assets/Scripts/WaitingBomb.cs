// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace UniteBlocks
{
    public class WaitingBomb : MonoBehaviour
    {
        public bool IsActive
        {
            get { return b_IsActive; }
            private set
            {
                b_IsActive = value;
                if (value)
                {
                    SetActiveColor();
                }
                else
                {
                    SetInactiveColor();
                }
            }
        }

        public bool IsBoosting { get; set; } = false;

        [SerializeField]
        PlayerInput m_Input;

        [SerializeField]
        ChainGauge m_Gauge;

        [SerializeField]
        BombGaugeSetting m_GaugeSetting;

        [SerializeField]
        PlayerSetting m_PlayerSetting;

        [SerializeField]
        GameManager m_GameManager;

        Material m_Material;

        bool b_IsActive;

        private void Awake()
        {
            m_Material = GetComponent<SpriteRenderer>().material;
            m_Gauge.Value = 0f;
            IsActive = false;
        }

        private void OnEnable()
        {
            m_Input.actions["Swap"].performed += OnSwapPerformed;
        }

        private void OnDisable()
        {
            m_Input.actions["Swap"].performed -= OnSwapPerformed;
        }

        private void Update()
        {
            if (!m_GameManager.IsGaugeIncreasing) { return; }
            float boost = 1f;
            if (IsBoosting)
            {
                boost *= m_PlayerSetting.AutoDropDelay / m_PlayerSetting.ManualDropDelay;
                boost *= m_GaugeSetting.BoostRatio;
            }
            m_Gauge.Value += Time.deltaTime * m_GaugeSetting.IncreasePerSec * boost;
        }

        void OnSwapPerformed(InputAction.CallbackContext context)
        {
            if (IsActive)
            {
                IsActive = false;
            }
            else if (m_Gauge.Value >= 1f)
            {
                IsActive = true;
            }
        }

        public void UseGauge()
        {
            m_Gauge.Value -= 1f;
            IsActive = false;
        }

        void SetActiveColor()
        {
            Color beforeColor = m_Material.color;
            Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 1f);
            m_Material.color = afterColor;
        }

        void SetInactiveColor()
        {
            Color beforeColor = m_Material.color;
            Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 0.3f);
            m_Material.color = afterColor;
        }
    }
}
