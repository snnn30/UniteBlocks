// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Utility
{
    public class UIShake : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector3 originalPosition;
        [SerializeField] private float shakeAmount = 5f;
        [SerializeField] private float shakeSpeed = 10f;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.localPosition;
        }

        private void Update()
        {
            float shakeX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * shakeAmount;
            float shakeY = Mathf.PerlinNoise(0, Time.time * shakeSpeed) * shakeAmount;
            rectTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
        }
    }
}
