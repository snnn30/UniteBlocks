// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

namespace Utility
{
    public class TextBlink : MonoBehaviour
    {
        [SerializeField]
        private float m_Cycle;

        private TextMeshProUGUI m_Text;
        private float m_Time;

        private void Awake()
        {
            m_Text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            m_Time += Time.unscaledDeltaTime;
            m_Time %= m_Cycle;

            float val = m_Time / m_Cycle;
            val = Mathf.Lerp(0f, Mathf.PI * 2f, val);
            m_Text.alpha = (1f + Mathf.Sin(val)) / 2f;
        }
    }
}
