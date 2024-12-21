﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Image _startImage;

        InputAction _anyKeyAction;

        private void Awake()
        {
            _startImage.gameObject.SetActive(true);
            Time.timeScale = 0;

            _anyKeyAction = new InputAction(
                "AnyKey",
                InputActionType.Button,
                "<Keyboard>/anyKey"
                );

            _anyKeyAction.performed += OnAnyKey;
        }

        private void OnDestroy()
        {
            _anyKeyAction.performed -= OnAnyKey;
            _anyKeyAction.Dispose();
        }

        private async void OnEnable()
        {
            // リスタート時の誤操作を防ぐ
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
            _anyKeyAction.Enable();
        }

        private void OnDisable()
        {
            _anyKeyAction.Disable();
        }

        void OnAnyKey(InputAction.CallbackContext context)
        {
            _startImage.gameObject.SetActive(false);
            Time.timeScale = 1;
            _anyKeyAction.Disable();
        }

        public void OnGameOver()
        {
            Time.timeScale = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}
