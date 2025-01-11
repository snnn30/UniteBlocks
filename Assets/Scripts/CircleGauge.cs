// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class CircleGauge : MonoBehaviour
    {
        [SerializeField] Image _circle;
        [SerializeField] Gradient _gradient;
        RectTransform _rect;
        float _value;

        public float Value
        {
            get { return _value; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _value = value;
                _rect.localScale = new Vector3(value, value, 1);
                _circle.color = _gradient.Evaluate(value);
            }
        }

        private void Awake()
        {
            _rect = _circle.GetComponent<RectTransform>();
        }
    }
}
