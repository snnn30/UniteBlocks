// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace UniteBlocks
{
    public class ScoreData : MonoBehaviour
    {
        public int Value
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
        private int m_InitialValue;

        private int b_Value;
        private bool m_IsOperating = false;
        private bool m_IsVisible = false;

        private void Awake()
        {
            Value = m_InitialValue;
            SetVisible(false);
        }

        public async UniTask SetValue(int targetValue, float seconds = 0f, float scale = 1f, Ease ease = Ease.Linear)
        {
            if (m_IsOperating) { Debug.LogWarning("無効な状態"); return; }
            if (!m_IsVisible) { Debug.LogWarning("表示されていない"); return; }

            m_IsOperating = true;

            // int型のままだと誤差が出る 補完の過程で積み重なっている？
            var numHandle = LMotion.Create((long)Value, (long)targetValue, seconds)
                .WithEase(ease)
                .Bind(x => Value = (int)x)
                .AddTo(this);

            Vector3 originalScale = m_ValueUI.transform.localScale;

            await LMotion.Create(m_ValueUI.transform.localScale, originalScale * scale, seconds * 0.5f).
                WithEase(Ease.InOutQuad).
                BindToLocalScale(m_ValueUI.transform)
                .AddTo(this);
            await LMotion.Create(m_ValueUI.transform.localScale, originalScale, seconds * 0.5f)
                .WithEase(Ease.InOutQuad)
                .BindToLocalScale(m_ValueUI.transform)
                .AddTo(this);
            await numHandle;

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
