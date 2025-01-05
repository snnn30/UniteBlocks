// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class ChainGauge : MonoBehaviour
    {
        [SerializeField] Slider[] _sliders;
        float _value;

        Slider[] Sliders => _sliders;
        public float Value
        {
            get { return _value; }
            set
            {
                if (value < 0) value = 0;
                if (value > MaxValue) value = MaxValue;

                _value = value;

                for (int i = 0; i < Sliders.Length; i++)
                {
                    if (_value >= i + 1)
                    {
                        Sliders[i].value = Sliders[i].maxValue;
                    }
                    else
                    {
                        Sliders[i].value = _value - i;
                    }

                }

            }
        }
        public int MaxValue => Sliders.Length;



        private void Awake()
        {
            foreach (var slider in _sliders)
            {
                slider.maxValue = 1f;
                slider.minValue = 0f;
            }
            Value = 0f;
        }

        /*
        //テスト用
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) { Value += 0.3f; }
            if (Input.GetKeyDown(KeyCode.N)) { Value -= 1f; }
        }
        */
    }

}
