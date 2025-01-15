// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] StartUI _startUI;
        [SerializeField] CountDownUI _countdownUI;
        [SerializeField] Image _pauseUI;
        [SerializeField] PlayerInput _input;
        StartUI StartUI => _startUI;
        CountDownUI CountDownUI => _countdownUI;
        Image PauseUI => _pauseUI;
        bool b_isTimeStopping;
        float _timeScale;
        private Subject<Unit> _subject = new Subject<Unit>();

        public bool IsGaugeIncreasing { get; set; } = false;
        public IObservable<Unit> OnGameOver => _subject;


        private void Awake()
        {
            StartUI.gameObject.SetActive(true);
            Time.timeScale = 0;

            _input.actions["AnyKey"].performed += GameStart;
        }

        private async void OnEnable()
        {
            // リスタート時の誤操作を防ぐ
            StartUI.SetVisilityPressAnyKey(false);
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
            StartUI.SetVisilityPressAnyKey(true);
        }



        async void GameStart(InputAction.CallbackContext context)
        {
            _input.actions["AnyKey"].performed -= GameStart;
            StartUI.gameObject.SetActive(false);

            CountDownUI.gameObject.SetActive(true);
            await CountDownUI.CountDown();
            CountDownUI.gameObject.SetActive(false);

            Time.timeScale = 1;
            IsGaugeIncreasing = true;

            _input.actions["Pause"].performed += OnPause;
        }

        public void GameOver()
        {
            DOTween.Clear(true);
            Time.timeScale = 0;
            _subject.OnNext(Unit.Default);

            _input.actions["Pause"].performed -= OnPause;
        }

        public void Restart()
        {
            DOTween.Clear(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private async void OnPause(InputAction.CallbackContext context)
        {
            if (Time.timeScale != 0)
            {
                _timeScale = Time.timeScale;
                _pauseUI.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                _pauseUI.gameObject.SetActive(false);
                CountDownUI.gameObject.SetActive(true);

                _input.actions["Pause"].performed -= OnPause;
                await CountDownUI.CountDown();
                _input.actions["Pause"].performed += OnPause;

                CountDownUI.gameObject.SetActive(false);

                Time.timeScale = _timeScale;
                IsGaugeIncreasing = true;
            }

        }
    }
}
