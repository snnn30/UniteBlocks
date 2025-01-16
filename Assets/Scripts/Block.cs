// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;


namespace UniteBlocks
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Block : MonoBehaviour, Item
    {
        public Color Color
        {
            set
            {
                if (!m_BlockColorTable.Colors.Contains(value))
                {
                    Debug.LogError("登録されていない色");
                }
                b_Color = value;
                m_Renderer.color = value;
            }
            get { return b_Color; }
        }

        public Vector2Int Shape
        {
            set
            {
                b_Shape = value;
                m_Renderer.size = value;
            }
            get { return b_Shape; }
        }

        [SerializeField]
        BlockColorTable m_BlockColorTable;

        SpriteRenderer m_Renderer;

        Color b_Color;
        Vector2Int b_Shape;



        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            Color = m_BlockColorTable.Colors[0];
            Shape = new Vector2Int(1, 1);
        }

    }
}
