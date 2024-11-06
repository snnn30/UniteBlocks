// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class PuyoController : MonoBehaviour
{
    [SerializeField] PuyoColorTable _puyoColorTable;

    SpriteRenderer _renderer;

    PuyoType _type = PuyoType.Invalid;
    public PuyoType PuyoType
    {
        set
        {
            _type = value;
            _renderer.color = _puyoColorTable.PuyoColoers[_type];
        }
        get { return _type; }
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void SetPos(Vector2 pos)
    {
        this.transform.localPosition = pos;
    }
}
