// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "ScriptableObject/PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        [SerializeField, Range(0f, 5f)] float _autoDropDelay = 0.48f;
        [SerializeField, Range(0f, 5f)] float _manualDropDelay = 0.06f;
        [SerializeField, Range(0f, 5f)] float _stagnationTime = 0.5f;
        [SerializeField, Range(0f, 5f)] float _moveDelay = 0.08f;
        [SerializeField, Range(0f, 5f)] float _rotateDelay = 0.12f;

        public float AutoDropDelay => _autoDropDelay;
        public float ManualDropDelay => _manualDropDelay;
        public float StagnationTime => _stagnationTime;
        public float MoveDelay => _moveDelay;
        public float RotateDelay => _rotateDelay;
    }
}
