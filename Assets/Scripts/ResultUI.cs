// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

namespace UniteBlocks
{
    public class ResultUI : MonoBehaviour
    {
        public int HighScore
        {
            get { return b_HighScore; }
            set
            {
                b_HighScore = value;
                m_HighScoreValue.text = value.ToString();
            }
        }

        public int CurrentScore
        {
            get { return b_CurrentScore; }
            set
            {
                b_CurrentScore = value;
                m_CurrentScoreValue.text = value.ToString();
            }
        }

        [SerializeField]
        private TextMeshProUGUI m_HighScoreValue;

        [SerializeField]
        private TextMeshProUGUI m_CurrentScoreValue;

        [SerializeField]
        private TextMeshProUGUI m_PressAnyKey;

        private int b_HighScore;
        private int b_CurrentScore;

        public void SetVisilityPressAnyKey(bool visible)
        {
            m_PressAnyKey.gameObject.SetActive(visible);
        }
    }
}
