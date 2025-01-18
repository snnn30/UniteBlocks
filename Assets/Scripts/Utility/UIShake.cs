// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Utility
{
    public class UIShake : MonoBehaviour
    {
        private RectTransform m_RectTransform;
        private Vector3 m_OriginalPosition;

        [SerializeField]
        private float m_ShakeAmount = 3f;

        [SerializeField]
        private float m_ShakeSpeed = 5f;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_OriginalPosition = m_RectTransform.localPosition;
        }

        private void Update()
        {
            float shakeX = Mathf.PerlinNoise(Time.time * m_ShakeSpeed, 0) * m_ShakeAmount;
            float shakeY = Mathf.PerlinNoise(0, Time.time * m_ShakeSpeed) * m_ShakeAmount;
            m_RectTransform.localPosition = m_OriginalPosition + new Vector3(shakeX, shakeY, 0);
        }

    }
}
