// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _atkSlider;
    [SerializeField] List<EnemyData> _enemyDataList;

    EnemyData _enemyData;

    void SetEnamy(int index)
    {
        _enemyData = _enemyDataList[index];
        _hpSlider.maxValue = _enemyData.Hp;
        _hpSlider.value = _enemyData.Hp;
        _atkSlider.maxValue = _enemyData.AtkTime;
        _atkSlider.value = 0;
        GetComponent<SpriteRenderer>().sprite = _enemyData.Sprite;
    }

    private void Awake()
    {
        SetEnamy(0);
    }

    private void Update()
    {
        _atkSlider.value += Time.deltaTime;
        if (_atkSlider.value == _enemyData.AtkTime)
        {
            Debug.Log("attack");
            _atkSlider.value = 0;
        }
    }
}
