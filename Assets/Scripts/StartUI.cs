// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI r_pressAnyKey;

    public void SetVisilityPressAnyKey(bool visible)
    {
        r_pressAnyKey.gameObject.SetActive(visible);
    }
}
