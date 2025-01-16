// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UniteBlocks
{
    public class DistanceManager : MonoBehaviour
    {
        public float Value
        {
            get { return b_Value; }
            set
            {
                if (m_IsStopping) { return; }

                if (value <= 0.0f)
                {
                    m_IsStopping = true;
                    m_GameManager.GameOver();
                    return;
                }

                b_Value = value;
                m_DistanceUI.Value = (uint)value;

                float reflexThreshold = m_DistanceSetting.ReflexTime * m_DecreasePerSecond;
                if (value < reflexThreshold)
                {
                    float v = Mathf.InverseLerp(reflexThreshold, 0f, value);
                    m_TimeScale = Mathf.Lerp(1f, m_DistanceSetting.ReflexTimeScale, v);
                    m_ChromaticAberration.intensity.value = v;
                    m_LensDistortion.intensity.value = Mathf.Lerp(m_LensDistortionInitialIntensity, -m_LensDistortionInitialIntensity, v);
                }
                else
                {
                    ResetPostProcess();
                }
            }
        }

        [SerializeField]
        private DistanceUI m_DistanceUI;

        [SerializeField]
        private DistanceSetting m_DistanceSetting;

        [SerializeField]
        private GameManager m_GameManager;

        [SerializeField]
        private Volume m_Volume;

        private ChromaticAberration m_ChromaticAberration;
        private LensDistortion m_LensDistortion;
        private float m_DecreasePerSecond;
        private float m_LensDistortionInitialIntensity;
        private float m_TimeScale = 1f;
        private bool m_IsStopping = false;

        private float b_Value;

        private void Awake()
        {
            if (!m_Volume.profile.TryGet<ChromaticAberration>(out m_ChromaticAberration))
            {
                Debug.LogWarning("ChromaticAberrationがない");
            }
            if (!m_Volume.profile.TryGet<LensDistortion>(out m_LensDistortion))
            {
                Debug.LogWarning("LensDistortionがない");
            }

            m_LensDistortionInitialIntensity = m_LensDistortion.intensity.value;
        }

        private void Start()
        {
            m_DistanceUI.Threshold = (uint)(m_DistanceSetting.DecreasePerSecond * m_DistanceSetting.TimeToReach);
            Value = m_DistanceSetting.InitialValue;
            m_DecreasePerSecond = m_DistanceSetting.DecreasePerSecond;
        }

        private void Update()
        {
            if (!m_GameManager.IsGaugeIncreasing) { return; }
            m_DecreasePerSecond += m_DistanceSetting.Acceleration * Time.deltaTime * m_TimeScale;
            m_DistanceUI.Threshold = (uint)(m_DecreasePerSecond * m_DistanceSetting.TimeToReach);
            Value -= m_DecreasePerSecond * Time.deltaTime * m_TimeScale;
        }

        public void ResetPostProcess()
        {
            m_ChromaticAberration.intensity.value = 0.0f;
            m_LensDistortion.intensity.value = m_LensDistortionInitialIntensity;
        }
    }
}
