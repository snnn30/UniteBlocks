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


namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] StartUI _startUI;
        [SerializeField] CountDownUI _countdownUI;
        StartUI StartUI => _startUI;
        CountDownUI CountDownUI => _countdownUI;
        InputAction AnyKeyAction { get; } = new InputAction(
                "AnyKey",
                InputActionType.Button,
                "<Keyboard>/anyKey"
                );
        bool b_isTimeStopping;
        private Subject<Unit> _subject = new Subject<Unit>();

        public bool IsGaugeIncreasing { get; set; } = false;
        public IObservable<Unit> OnGameOver => _subject;


        private void Awake()
        {
            StartUI.gameObject.SetActive(true);
            Time.timeScale = 0;

            AnyKeyAction.performed += OnAnyKey;
        }

        private void OnDestroy()
        {
            AnyKeyAction.performed -= OnAnyKey;
            AnyKeyAction.Dispose();
        }

        private async void OnEnable()
        {
            // リスタート時の誤操作を防ぐ
            StartUI.SetVisilityPressAnyKey(false);
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
            StartUI.SetVisilityPressAnyKey(true);
            AnyKeyAction.Enable();
        }

        private void OnDisable()
        {
            AnyKeyAction.Disable();
        }

        async void OnAnyKey(InputAction.CallbackContext context)
        {
            AnyKeyAction.Disable();
            StartUI.gameObject.SetActive(false);

            CountDownUI.gameObject.SetActive(true);
            await CountDownUI.CountDown();
            CountDownUI.gameObject.SetActive(false);

            Time.timeScale = 1;
            IsGaugeIncreasing = true;
        }



        public void GameOver()
        {
            DOTween.Clear(true);
            Time.timeScale = 0;
            _subject.OnNext(Unit.Default);
        }

        public void Restart()
        {
            DOTween.Clear(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}
