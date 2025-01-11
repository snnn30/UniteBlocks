// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Manager;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace Board
{
    public class WaitingBomb : MonoBehaviour
    {
        [SerializeField] PlayerInput _input;
        [SerializeField] ChainGauge _gauge;
        [SerializeField] BombGaugeSetting _gaugeSetting;
        [SerializeField] PlayerSetting _playerSetting;
        [SerializeField] GameManager _gameManager;
        Material _material;
        bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                _isActive = value;
                if (value)
                {
                    SetActiveColor();
                    void SetActiveColor()
                    {
                        Color beforeColor = _material.color;
                        Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 1f);
                        _material.color = afterColor;
                    }
                }
                else
                {
                    SetInactiveColor();
                    void SetInactiveColor()
                    {
                        Color beforeColor = _material.color;
                        Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 0.3f);
                        _material.color = afterColor;
                    }
                }
            }
        }

        public bool IsBoosting { get; set; } = false;



        void OnSwapPerformed(InputAction.CallbackContext context)
        {
            if (IsActive)
            {
                IsActive = false;
            }
            else if (_gauge.Value >= 1f)
            {
                IsActive = true;
            }
        }

        public void UseGauge()
        {
            _gauge.Value -= 1f;
            IsActive = false;
        }



        private void Awake()
        {
            _material = GetComponent<SpriteRenderer>().material;
            _gauge.Value = 0f;
            IsActive = false;
        }

        private void OnEnable()
        {
            _input.actions["Swap"].performed += OnSwapPerformed;
        }

        private void OnDisable()
        {
            _input.actions["Swap"].performed -= OnSwapPerformed;
        }

        private void Update()
        {
            if (!_gameManager.IsGaugeIncreasing) { return; }
            float boost = 1f;
            if (IsBoosting)
            {
                boost *= _playerSetting.AutoDropDelay / _playerSetting.ManualDropDelay;
                boost *= _gaugeSetting.BoostRatio;
            }
            _gauge.Value += Time.deltaTime * _gaugeSetting.IncreasePerSec * boost;
        }

    }
}
