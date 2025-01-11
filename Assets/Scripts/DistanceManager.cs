// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Manager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Score
{
    public class DistanceManager : MonoBehaviour
    {
        [SerializeField] DistanceUI r_distanceUI;
        [SerializeField] DistanceSetting r_distanceSetting;
        [SerializeField] GameManager r_gameManager;
        [SerializeField] Volume r_volume;
        float b_value;
        float w_decreasePerSecond;
        ChromaticAberration w_chromaticAberration;
        LensDistortion w_lensDistortion;
        float r_lensDistortionInitialIntinsity;

        public float Value
        {
            get { return b_value; }
            set
            {
                if (value <= 0.0f)
                {
                    value = 0.0f;
                    r_gameManager.OnGameOver();
                    return;
                }

                float reflexThreshold = r_distanceSetting.reflexTime * w_decreasePerSecond;
                if (value < reflexThreshold)
                {
                    float v = Mathf.InverseLerp(reflexThreshold, 0f, value);
                    Time.timeScale = Mathf.Lerp(1f, r_distanceSetting.reflexTimeScale, v);
                    w_chromaticAberration.intensity.value = v;
                    w_lensDistortion.intensity.value = Mathf.Lerp(r_lensDistortionInitialIntinsity, -r_lensDistortionInitialIntinsity, v);
                }
                else
                {
                    if (Time.timeScale != 0f) { Time.timeScale = 1f; }
                    w_chromaticAberration.intensity.value = 0.0f;
                    w_lensDistortion.intensity.value = r_lensDistortionInitialIntinsity;
                }

                b_value = value;
                r_distanceUI.Value = (uint)value;
            }
        }



        private void Awake()
        {
            if (!r_volume.profile.TryGet<ChromaticAberration>(out w_chromaticAberration))
            {
                Debug.LogWarning("ChromaticAberrationがない");
            }
            if (!r_volume.profile.TryGet<LensDistortion>(out w_lensDistortion))
            {
                Debug.LogWarning("LensDistortionがない");
            }

            r_lensDistortionInitialIntinsity = w_lensDistortion.intensity.value;
        }

        private void Start()
        {
            r_distanceUI.Threshold = (uint)(r_distanceSetting.decreasePerSecond * r_distanceSetting.timeToReach);
            Value = r_distanceSetting.initialValue;
            w_decreasePerSecond = r_distanceSetting.decreasePerSecond;
        }

        private void Update()
        {
            if (!r_gameManager.IsGaugeIncreasing) { return; }
            w_decreasePerSecond += r_distanceSetting.acceleration * Time.deltaTime;
            r_distanceUI.Threshold = (uint)(w_decreasePerSecond * r_distanceSetting.timeToReach);
            Value -= w_decreasePerSecond * Time.deltaTime;
        }
    }
}
