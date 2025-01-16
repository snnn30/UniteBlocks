// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;
using Utility;

namespace UniteBlocks
{
    public class DistanceUI : MonoBehaviour
    {
        public int Threshold { get; set; }
        public int Value
        {
            get { return b_Value; }
            set
            {
                if (value < 0) { value = 0; }
                b_Value = value;
                m_Text.text = value.ToString();

                if (value > Threshold) { value = Threshold; }
                float scale = Mathf.InverseLerp(Threshold, 0, value);
                m_Circle.Value = Mathf.Lerp(m_MinScale, 1, scale);
            }
        }

        [SerializeField]
        private CircleGauge m_Circle;

        [SerializeField]
        private TextMeshProUGUI m_Text;

        [SerializeField]
        private float m_MinScale;

        private int b_Value;
    }
}
