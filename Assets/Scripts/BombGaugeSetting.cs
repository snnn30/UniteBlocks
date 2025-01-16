// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace UniteBlocks
{
    [CreateAssetMenu(menuName = "ScriptableObject/BombGaugeSetting")]
    public class BombGaugeSetting : ScriptableObject
    {
        [SerializeField] float m_IncreasePerSec;
        [SerializeField, Range(1f, 5f)] float m_BoostRatio;

        public float IncreasePerSec => m_IncreasePerSec;
        public float BoostRatio => m_BoostRatio;
    }
}
