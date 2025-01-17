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
    public class CountDownUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_CountUI;

        [SerializeField]
        private int m_InitialCount;

        [SerializeField]
        private float m_Scale;

        [SerializeField]
        private float m_TimePer1Count;

        private float Count
        {
            get { return b_Count; }
            set
            {
                b_Count = value;
                m_CountUI.text = value.ToString();
            }
        }

        private float b_Count;

        private void Awake()
        {
            Count = m_InitialCount;
        }

        public async UniTask CountDown()
        {
            Count = m_InitialCount;
            while (Count > 0)
            {
                var originalScale = m_CountUI.rectTransform.localScale;

                var scaleHandle = LMotion.Create(m_CountUI.rectTransform.localScale, originalScale * m_Scale, m_TimePer1Count)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToLocalScale(m_CountUI.rectTransform)
                    .AddTo(this);

                var alphaHandle = LMotion.Create(1f, 0f, m_TimePer1Count)
                    .WithEase(Ease.InCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(x => m_CountUI.alpha = x)
                    .AddTo(this);

                await scaleHandle;
                await alphaHandle;

                m_CountUI.rectTransform.localScale = originalScale;
                Count--;
            }
        }
    }
}
