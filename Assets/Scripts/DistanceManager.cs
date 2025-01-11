// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Manager;
using UnityEngine;

namespace Score
{
    public class DistanceManager : MonoBehaviour
    {
        [SerializeField] DistanceUI _distanceUI;
        [SerializeField] DistanceSetting _distanceSetting;
        [SerializeField] GameManager _gameManager;
        float _value;

        float Value
        {
            get { return _value; }
            set
            {
                if (value < 0.0f) { value = 0.0f; }
                _value = value;
                _distanceUI.Value = (uint)value;
            }
        }

        private void Start()
        {
            _distanceUI.Threshold = (uint)(_distanceSetting.decreasePerSecond * _distanceSetting.timeToReach);
            Value = _distanceSetting.initialValue;
        }

        private void Update()
        {
            if (!_gameManager.IsGaugeIncreasing) { return; }
            Value -= _distanceSetting.decreasePerSecond * Time.deltaTime;
        }
    }
}
