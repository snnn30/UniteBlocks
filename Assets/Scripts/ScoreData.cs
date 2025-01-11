// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Score
{
    public class ScoreData : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _constUI;
        [SerializeField] TextMeshProUGUI _valueUI;
        [SerializeField] uint _initialValue;
        uint _value;

        bool IsValid { get; set; } = true;
        bool IsVisible { get; set; } = false;
        TextMeshProUGUI ConstUI => _constUI;
        TextMeshProUGUI ValueUI => _valueUI;
        uint InitialValue => _initialValue;
        public uint Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _valueUI.text = _value.ToString();
            }
        }


        public void Awake()
        {
            Value = InitialValue;
            SetVisible(false);
        }



        public async UniTask SetValue(uint targetValue, float seconds = 0f, float scale = 1f, Ease ease = Ease.Linear)
        {
            if (!IsValid) { Debug.LogWarning("無効な状態"); return; }
            if (!IsVisible) { Debug.LogWarning("表示されていない"); return; }

            IsValid = false;

            var numTween = DOTween.To(
                () => Value,
                x =>
                {
                    Value = x;
                },
                targetValue,
                seconds
                ).SetEase(ease);

            Vector3 originalScale = ValueUI.transform.localScale;
            var scaleTween = DOTween.Sequence()
              .Append(ValueUI.transform.DOScale(originalScale * scale, seconds / 2f).SetEase(Ease.InOutQuad))
              .Append(ValueUI.transform.DOScale(originalScale, seconds / 2f).SetEase(Ease.InOutQuad));

            await numTween;
            await scaleTween;

            IsValid = true;
        }

        public void SetVisible(bool visible)
        {
            if (ConstUI != null) { ConstUI.gameObject.SetActive(visible); }
            ValueUI.gameObject.SetActive(visible);
            IsVisible = visible;
        }
    }
}
