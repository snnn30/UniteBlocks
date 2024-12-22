// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Board
{
    public class WaitingBomb : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _gauge;
        [SerializeField] PlayerInput _input;
        [SerializeField] int _activationCount = 5;

        int _count;
        Material _material;

        public bool IsActive { get; private set; } = false;
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                if (_count > _activationCount) { _count = _activationCount; }
                if (_count < 0) { _count = 0; }

                if (_count != _activationCount)
                {
                    SetInactiveColor();
                    IsActive = false;
                }
                _gauge.material.SetFloat("_Value", (float)_count / _activationCount);
            }
        }



        void SetActiveColor()
        {
            Color beforeColor = _material.color;
            Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 1f);
            _material.color = afterColor;
        }

        void SetInactiveColor()
        {
            Color beforeColor = _material.color;
            Color afterColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 0.3f);
            _material.color = afterColor;
        }

        void OnSwapPerformed(InputAction.CallbackContext context)
        {
            if (IsActive)
            {
                SetInactiveColor();
                IsActive = false;
            }
            else if (Count == _activationCount)
            {
                SetActiveColor();
                IsActive = true;
            }
        }

        /*
        // テスト用
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) { Count++; }
            if (Input.GetKeyDown(KeyCode.M)) { Count = 0; }
        }
        */


        private void Awake()
        {
            _material = GetComponent<SpriteRenderer>().material;
            Count = 0;
            IsActive = false;
            SetInactiveColor();
        }


        private void OnEnable()
        {
            _input.actions["Swap"].performed += OnSwapPerformed;
        }

        private void OnDisable()
        {
            _input.actions["Swap"].performed -= OnSwapPerformed;
        }

    }
}
