// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using TMPro;
using UnityEngine;

public class TextBlink : MonoBehaviour
{
    TextMeshProUGUI r_text;
    [SerializeField] float r_cycle;
    float _time;


    private void Awake()
    {
        r_text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _time += Time.unscaledDeltaTime;
        _time %= r_cycle;

        float val = _time / r_cycle;
        val = Mathf.Lerp(0f, Mathf.PI * 2f, val);
        r_text.alpha = (1f + Mathf.Sin(val)) / 2f;


    }
}
