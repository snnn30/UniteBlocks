// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;


namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Puyo : MonoBehaviour, Item
    {
        [SerializeField] PuyoColorTable _puyoColorTable;

        SpriteRenderer _renderer;

        PuyoType _type;
        public PuyoType PuyoType
        {
            set
            {
                _type = value;
                _renderer.color = _puyoColorTable.PuyoColoers[_type];
            }
            get { return _type; }
        }

        Vector2Int _shape;
        public Vector2Int Shape
        {
            set
            {
                _shape = value;
                _renderer.size = value;
            }
            get { return _shape; }
        }




        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            PuyoType = PuyoType.Invalid;
            Shape = new Vector2Int(1, 1);
        }

    }
}
