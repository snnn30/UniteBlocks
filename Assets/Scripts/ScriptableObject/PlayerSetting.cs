// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace UniteBlocks
{
    [CreateAssetMenu(menuName = "ScriptableObject/PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        public float AutoDropDelay => m_AutoDropDelay;
        public float ManualDropDelay => m_ManualDropDelay;
        public float StagnationTime => m_StagnationTime;
        public float MoveDelay => m_MoveDelay;
        public float RotateDelay => m_RotateDelay;

        [SerializeField, Range(0f, 5f)]
        private float m_AutoDropDelay = 0.48f;

        [SerializeField, Range(0f, 5f)]
        private float m_ManualDropDelay = 0.06f;

        [SerializeField, Range(0f, 5f)]
        private float m_StagnationTime = 0.5f;

        [SerializeField, Range(0f, 5f)]
        private float m_MoveDelay = 0.08f;

        [SerializeField, Range(0f, 5f)]
        private float m_RotateDelay = 0.12f;
    }
}
