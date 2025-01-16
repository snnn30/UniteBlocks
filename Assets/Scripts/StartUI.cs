// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

namespace UniteBlocks
{
    public class StartUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_PressAnyKey;

        public void SetVisilityPressAnyKey(bool visible)
        {
            m_PressAnyKey.gameObject.SetActive(visible);
        }
    }
}
