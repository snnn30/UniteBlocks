// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;
using Utility;

namespace Score
{
    public class DistanceUI : MonoBehaviour
    {
        [SerializeField] CircleGauge _circle;
        [SerializeField] TextMeshProUGUI _text;
        [SerializeField] float _minScale;
        uint _value;

        public uint Threshold { get; set; }
        public uint Value
        {
            get { return _value; }
            set
            {
                if (value < 0) { value = 0; }
                _value = value;
                _text.text = value.ToString();

                if (value > Threshold) { value = Threshold; }
                float scale = Mathf.InverseLerp(Threshold, 0, value);
                _circle.Value = Mathf.Lerp(_minScale, 1, scale);
            }
        }
    }
}
