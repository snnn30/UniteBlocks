// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace UniteBlocks
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_Instance;

        public static GameManager Instance => s_Instance;

        public bool IsGaugeIncreasing { get; set; } = false;
        public IObservable<Unit> OnGameOver => m_Subject;

        [SerializeField]
        private StartUI m_StartUI;

        [SerializeField]
        private CountDownUI m_CountDownUI;

        [SerializeField]
        private Image m_PauseUI;

        [SerializeField]
        private PlayerInput m_Input;

        private float m_TimeScale;
        private Subject<Unit> m_Subject = new Subject<Unit>();

        private bool b_IsTimeStopping;

        private void Awake()
        {
            m_StartUI.gameObject.SetActive(true);
            Time.timeScale = 0;
            s_Instance = this;
        }

        private async void OnEnable()
        {
            // リスタート時の誤操作を防ぐ
            m_StartUI.SetVisilityPressAnyKey(false);
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
            m_StartUI.SetVisilityPressAnyKey(true);
            m_Input.actions["AnyKey"].performed += GameStart;
        }

        async void GameStart(InputAction.CallbackContext context)
        {
            m_Input.actions["AnyKey"].performed -= GameStart;
            m_StartUI.gameObject.SetActive(false);

            m_CountDownUI.gameObject.SetActive(true);
            await m_CountDownUI.CountDown();
            m_CountDownUI.gameObject.SetActive(false);

            Time.timeScale = 1;
            IsGaugeIncreasing = true;

            m_Input.actions["Pause"].performed += OnPause;
        }

        public void GameOver()
        {
            Time.timeScale = 0;
            m_Subject.OnNext(Unit.Default);

            m_Input.actions["Pause"].performed -= OnPause;
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private async void OnPause(InputAction.CallbackContext context)
        {
            if (Time.timeScale != 0)
            {
                m_TimeScale = Time.timeScale;
                m_PauseUI.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                m_PauseUI.gameObject.SetActive(false);
                m_CountDownUI.gameObject.SetActive(true);

                m_Input.actions["Pause"].performed -= OnPause;
                await m_CountDownUI.CountDown();
                m_Input.actions["Pause"].performed += OnPause;

                m_CountDownUI.gameObject.SetActive(false);

                Time.timeScale = m_TimeScale;
                IsGaugeIncreasing = true;
            }

        }
    }
}
