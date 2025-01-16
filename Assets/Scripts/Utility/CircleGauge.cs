// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class CircleGauge : MonoBehaviour
    {
        public float Value
        {
            get { return b_Value; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                b_Value = value;
                m_RectTransform.localScale = new Vector3(value, value, 1);
                m_Circle.color = m_Gradient.Evaluate(value);
            }
        }

        [SerializeField]
        private Image m_Circle;

        [SerializeField]
        private Gradient m_Gradient;

        private RectTransform m_RectTransform;

        private float b_Value;

        private void Awake()
        {
            m_RectTransform = m_Circle.GetComponent<RectTransform>();
        }
    }
}
