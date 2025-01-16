// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class ChainGauge : MonoBehaviour
    {
        public float Value
        {
            get { return b_Value; }
            set
            {
                if (value < 0) value = 0;
                if (value > MaxValue) value = MaxValue;

                b_Value = value;

                for (int i = 0; i < m_Sliders.Length; i++)
                {
                    if (b_Value >= i + 1)
                    {
                        m_Sliders[i].value = m_Sliders[i].maxValue;
                    }
                    else
                    {
                        m_Sliders[i].value = b_Value - i;
                    }
                }
            }
        }

        public int MaxValue => m_Sliders.Length;

        [SerializeField]
        private Slider[] m_Sliders;

        private float b_Value;

        private void Awake()
        {
            foreach (var slider in m_Sliders)
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
