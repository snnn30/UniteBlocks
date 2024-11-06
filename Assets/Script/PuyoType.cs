// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

public enum PuyoType
{
    Invalid,
    Green,
    Red,
    Yellow,
    Blue,
    Purple,
    Cyan,
};

[CreateAssetMenu(menuName = "ScriptableObject/PuyoColorTable")]
public class PuyoColorTable : ScriptableObject
{
    [SerializeField]
    SerializableDictionary<PuyoType, Color> _puyoColoers = null;
    public SerializableDictionary<PuyoType, Color> PuyoColoers => _puyoColoers;
}
