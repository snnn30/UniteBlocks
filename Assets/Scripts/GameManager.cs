// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Image _startImage;

        Image StartImage => _startImage;
        InputAction AnyKeyAction { get; } = new InputAction(
                "AnyKey",
                InputActionType.Button,
                "<Keyboard>/anyKey"
                );



        private void Awake()
        {
            StartImage.gameObject.SetActive(true);
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
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
            AnyKeyAction.Enable();
        }

        private void OnDisable()
        {
            AnyKeyAction.Disable();
        }

        void OnAnyKey(InputAction.CallbackContext context)
        {
            StartImage.gameObject.SetActive(false);
            Time.timeScale = 1;
            AnyKeyAction.Disable();
        }



        public void OnGameOver()
        {
            Time.timeScale = 0;
            DOTween.KillAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}
