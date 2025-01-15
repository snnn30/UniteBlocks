// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _countUI;
    [SerializeField] int _initialCount;
    [SerializeField] float _scale;
    [SerializeField] float _timePer1Count;
    float _count;
    float Count
    {
        get { return _count; }
        set
        {
            _count = value;
            _countUI.text = value.ToString();
        }
    }



    private void Awake()
    {
        Count = _initialCount;
    }



    public async UniTask CountDown()
    {
        Count = _initialCount;
        while (Count > 0)
        {
            var originalScale = _countUI.rectTransform.localScale;
            var scaleTween = _countUI.rectTransform.DOScale(originalScale * _scale, _timePer1Count).SetEase(Ease.OutCubic).SetUpdate(true);
            var alphaTween = DOTween.To(
                () => 1f,
                x =>
                {
                    _countUI.alpha = x;
                },
                0f,
                _timePer1Count
                ).SetEase(Ease.InCubic).SetUpdate(true);

            await scaleTween;
            await alphaTween;

            _countUI.rectTransform.localScale = originalScale;
            Count--;
        }
    }
}
