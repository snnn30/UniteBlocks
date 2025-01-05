// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Board
{
    [CreateAssetMenu(menuName = "ScriptableObject/BombGaugeSetting")]
    public class BombGaugeSetting : ScriptableObject
    {
        [SerializeField] float _increasePerSec;
        [SerializeField, Range(1f, 5f)] float _boostRatio;

        public float IncreasePerSec => _increasePerSec;
        public float BoostRatio => _boostRatio;
    }
}
