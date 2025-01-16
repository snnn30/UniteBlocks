// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UniteBlocks
{
    public class ScoreData : MonoBehaviour
    {
        public uint Value
        {
            get { return b_Value; }
            set
            {
                b_Value = value;
                m_ValueUI.text = b_Value.ToString();
            }
        }

        [SerializeField]
        private TextMeshProUGUI m_ConstUI;

        [SerializeField]
        private TextMeshProUGUI m_ValueUI;

        [SerializeField]
        private uint m_InitialValue;

        private uint b_Value;
        private bool m_IsOperating = false;
        private bool m_IsVisible = false;

        private void Awake()
        {
            Value = m_InitialValue;
            SetVisible(false);
        }

        public async UniTask SetValue(uint targetValue, float seconds = 0f, float scale = 1f, Ease ease = Ease.Linear)
        {
            if (m_IsOperating) { Debug.LogWarning("無効な状態"); return; }
            if (!m_IsVisible) { Debug.LogWarning("表示されていない"); return; }

            m_IsOperating = true;

            var numTween = DOTween.To(
                () => Value,
                x =>
                {
                    Value = x;
                },
                targetValue,
                seconds
                ).SetEase(ease);

            Vector3 originalScale = m_ValueUI.transform.localScale;
            var scaleTween = DOTween.Sequence()
              .Append(m_ValueUI.transform.DOScale(originalScale * scale, seconds / 2f).SetEase(Ease.InOutQuad))
              .Append(m_ValueUI.transform.DOScale(originalScale, seconds / 2f).SetEase(Ease.InOutQuad));

            await numTween;
            await scaleTween;

            m_IsOperating = false;
        }

        public void SetVisible(bool visible)
        {
            if (m_ConstUI != null) { m_ConstUI.gameObject.SetActive(visible); }
            m_ValueUI.gameObject.SetActive(visible);
            m_IsVisible = visible;
        }
    }
}
