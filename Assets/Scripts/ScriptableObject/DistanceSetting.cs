// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace UniteBlocks
{
    [CreateAssetMenu(menuName = "ScriptableObject/DistanceSetting")]
    public class DistanceSetting : ScriptableObject
    {
        public float DecreasePerSecond => m_DecreasePerSecond;
        public int InitialValue => m_InitialValue;
        public float TimeToReach => m_TimeToReach;
        public float Acceleration => m_Acceleration;
        public float ReflexTime => m_ReflexTime;
        public float ReflexTimeScale => m_ReflexTimeScale;

        [SerializeField]
        private float m_DecreasePerSecond;

        [SerializeField]
        private int m_InitialValue;

        [SerializeField]
        private float m_TimeToReach;

        [SerializeField]
        private float m_Acceleration;

        [SerializeField]
        private float m_ReflexTime;

        [SerializeField]
        private float m_ReflexTimeScale;
    }
}
