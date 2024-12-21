// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using Utility;

namespace Items
{
    public enum PuyoType
    {
        Invalid,
        Green,
        Red,
        Yellow,
        Blue,
    };

    [CreateAssetMenu(menuName = "ScriptableObject/PuyoColorTable")]
    public class PuyoColorTable : ScriptableObject
    {
        [SerializeField]
        SerializableDictionary<PuyoType, Color> _puyoColoers = null;
        public SerializableDictionary<PuyoType, Color> PuyoColoers => _puyoColoers;
    }
}
