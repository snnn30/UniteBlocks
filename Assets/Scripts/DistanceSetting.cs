﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Score
{
    [CreateAssetMenu(menuName = "ScriptableObject/DistanceSetting")]
    public class DistanceSetting : ScriptableObject
    {
        public float decreasePerSecond;
        public uint initialValue;
        public float timeToReach;
        public float acceleration;
        public float reflexTime;
        public float reflexTimeScale;
    }
}
