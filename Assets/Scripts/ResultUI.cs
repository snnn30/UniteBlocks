// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

namespace Score
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI r_highScoreValue;
        [SerializeField] TextMeshProUGUI r_currentScoreValue;
        [SerializeField] TextMeshProUGUI r_pressAnyKey;
        uint b_highScore;
        uint b_currentScore;

        public uint HighScore
        {
            get { return b_highScore; }
            set
            {
                b_highScore = value;
                r_highScoreValue.text = value.ToString();
            }
        }
        public uint CurrentScore
        {
            get { return b_currentScore; }
            set
            {
                b_currentScore = value;
                r_currentScoreValue.text = value.ToString();
            }
        }

        public void SetVisilityPressAnyKey(bool visible)
        {
            r_pressAnyKey.gameObject.SetActive(visible);
        }
    }
}
