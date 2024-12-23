// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] int hp;
    [SerializeField] int atk;
    [SerializeField] float atkTime;
    [SerializeField] Sprite sprite;

    public int Hp => hp;
    public int Atk => atk;
    public float AtkTime => atkTime;
    public Sprite Sprite => sprite;
}
