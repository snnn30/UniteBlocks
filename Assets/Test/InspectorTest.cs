// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

public class InspectorTest : MonoBehaviour
{
    [SerializeField] float _value;
    [SerializeField] Color _color;
    SpriteRenderer _renderer;
    float Value
    {
        get { return _value; }
        set
        {
            _value = value;
            this.transform.localPosition = new Vector3(_value, 0, 0);
        }
    }
    Color Color
    {
        get { return Color; }
        set
        {
            _color = value;
            _renderer.color = value;
        }
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        Value = _value;
        Color = _color;
    }
}
