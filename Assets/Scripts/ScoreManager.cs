// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace UniteBlocks
{
    public class ScoreManager : MonoBehaviour
    {
        public bool IsOperating { get; private set; } = false;

        [SerializeField]
        private ScoreData m_Score;

        [SerializeField]
        private ScoreData m_ScoreAddition;

        [SerializeField]
        private ScoreData m_ScoreMultiplication;

        [SerializeField]
        private DistanceManager m_DistanceManager;

        [SerializeField]
        private GameManager m_GameManager;

        [SerializeField]
        private ResultUI m_ResultUI;

        [SerializeField]
        private PlayerInput m_Input;

        [SerializeField]
        private Volume m_Volume;

        [SerializeField, Range(1f, 2f)]
        private float m_ScaleAtAddition = 1.2f;

        [SerializeField, Range(0f, 5f)]
        private float m_TimeToResolve = 1.4f;

        [SerializeField, Range(0f, 5f)]
        private float m_TimeToAdd = 0.4f;

        public void Start()
        {
            m_Score.SetVisible(true);
            m_GameManager.OnGameOver.Subscribe(_ =>
            {
                SaveScore().Forget();
            }).AddTo(this);
        }

        public async UniTask AddScoreAddition(int value)
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;
            await m_ScoreAddition.SetValue(m_ScoreAddition.Value + value, m_TimeToAdd, m_ScaleAtAddition);
            IsOperating = false;
        }

        public async UniTask AddScoreMultiplication(int value)
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;
            await m_ScoreMultiplication.SetValue(m_ScoreMultiplication.Value + value, m_TimeToAdd, m_ScaleAtAddition);
            IsOperating = false;
        }

        public void SetVisible(bool visible)
        {
            m_ScoreAddition.SetVisible(visible);
            m_ScoreMultiplication.SetVisible(visible);
        }

        public async UniTask ResolveAddition()
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;

            int targetScore = m_Score.Value + m_ScoreAddition.Value;
            if (targetScore > 999999999) { targetScore = 999999999; }

            var scoreTween = m_Score.SetValue(targetScore, m_TimeToResolve);
            var scoreAddedTween = m_ScoreAddition.SetValue(0, m_TimeToResolve);
            var distanceTween = DOTween.To(
                () => m_DistanceManager.Value,
                x =>
                {
                    m_DistanceManager.Value = x;
                },
                m_DistanceManager.Value + m_ScoreAddition.Value,
                m_TimeToResolve
                );

            await scoreAddedTween;
            await scoreTween;
            await distanceTween;

            // この方法でゲームオーバーにするとDotweenで警告が出るが問題なく動作する
            // かなりレアなバグらしく原因不明とのこと
            // テストしたところなぜかm_Score.Valueが999999999を超えていた　問題大ありやないか
            if (targetScore == 999999999)
            {
                m_GameManager.GameOver();
            }

            IsOperating = false;
        }

        public async UniTask ResolveMultiplication()
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            if (m_ScoreMultiplication.Value == 1) { return; }
            IsOperating = true;

            var scoreAddedTween = m_ScoreAddition.SetValue(m_ScoreAddition.Value * m_ScoreMultiplication.Value, m_TimeToResolve, 1f);
            var scoreMultiplicationTween = m_ScoreMultiplication.SetValue(1, m_TimeToResolve, 1f);

            await scoreAddedTween;
            await scoreMultiplicationTween;

            IsOperating = false;
        }

        public async UniTask SaveScore()
        {
            m_DistanceManager.ResetPostProcess();

            if (m_Score.Value > PlayerPrefs.GetInt("HighScore"))
            {
                PlayerPrefs.SetInt("HighScore", m_Score.Value);
            }

            m_ResultUI.HighScore = PlayerPrefs.GetInt("HighScore");
            m_ResultUI.CurrentScore = m_Score.Value;
            m_ResultUI.gameObject.SetActive(true);
            m_ResultUI.SetVisilityPressAnyKey(false);

            await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
            m_ResultUI.SetVisilityPressAnyKey(true);
            m_Input.actions["AnyKey"].performed += OnAnyKeyPerformed;

            void OnAnyKeyPerformed(InputAction.CallbackContext context)
            {
                m_Input.actions["AnyKey"].performed -= OnAnyKeyPerformed;
                m_GameManager.Restart();
            }
        }


        // テスト用

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) { m_Score.Value = 999999000; }
        }


    }
}
