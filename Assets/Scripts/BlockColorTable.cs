// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using UnityEngine;

namespace UniteBlocks
{
    [CreateAssetMenu(menuName = "ScriptableObject/BlockColorTable")]
    public class BlockColorTable : ScriptableObject
    {
        public List<Color> Colors => m_Colors;

        [SerializeField]
        private List<Color> m_Colors;
    }
}
