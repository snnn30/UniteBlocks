// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Score
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] ScoreData _score;
        [SerializeField] ScoreData _scoreAddition;
        [SerializeField] ScoreData _scoreMultiplication;
        [SerializeField] DistanceManager _distanceManager;
        [SerializeField] GameManager _gameManager;
        [SerializeField] ResultUI _resultUI;
        [SerializeField] PlayerInput _input;
        [SerializeField] Volume _volume;
        [SerializeField, Range(1f, 2f)] float _scaleAtAddition = 1.2f;
        [SerializeField, Range(0f, 5f)] float _timeToResolve = 2f;
        [SerializeField, Range(0f, 5f)] float _timeToAdd = 0.4f;

        bool IsOperating { get; set; } = false;
        ScoreData Score => _score;
        ScoreData ScoreAddition => _scoreAddition;
        ScoreData ScoreMultiplication => _scoreMultiplication;
        ResultUI ResultUI => _resultUI;
        float ScaleAtAddition => _scaleAtAddition;
        float TimeToResolve => _timeToResolve;
        float TimeToAdd => _timeToAdd;



        public void Start()
        {
            Score.SetVisible(true);
            _gameManager.OnGameOver.Subscribe(_ =>
            {
                SaveScore().Forget();
            }).AddTo(this);
        }



        public async UniTask AddScoreAddition(uint value)
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;
            await ScoreAddition.SetValue(ScoreAddition.Value + value, TimeToAdd, ScaleAtAddition);
            IsOperating = false;
        }

        public async UniTask AddScoreMultiplication(uint value)
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;
            await ScoreMultiplication.SetValue(ScoreMultiplication.Value + value, TimeToAdd, ScaleAtAddition);
            IsOperating = false;
        }

        public void SetVisible(bool visible)
        {
            ScoreAddition.SetVisible(visible);
            ScoreMultiplication.SetVisible(visible);
        }

        public async UniTask ResolveAddition()
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            IsOperating = true;

            var scoreTween = Score.SetValue(Score.Value + ScoreAddition.Value, TimeToResolve);
            var scoreAddedTween = ScoreAddition.SetValue(0, TimeToResolve);
            var distanceTween = DOTween.To(
                () => _distanceManager.Value,
                x =>
                {
                    _distanceManager.Value = x;
                },
                _distanceManager.Value + ScoreAddition.Value,
                TimeToResolve
                );

            await scoreAddedTween;
            await scoreTween;
            await distanceTween;

            IsOperating = false;
        }

        public async UniTask ResolveMultiplication()
        {
            if (IsOperating) { Debug.LogWarning("操作中"); return; }
            if (ScoreMultiplication.Value == 1) { return; }
            IsOperating = true;

            var scoreAddedTween = ScoreAddition.SetValue(ScoreAddition.Value * ScoreMultiplication.Value, TimeToResolve, 1f);
            var scoreMultiplicationTween = ScoreMultiplication.SetValue(1, TimeToResolve, 1f);

            await scoreAddedTween;
            await scoreMultiplicationTween;

            IsOperating = false;
        }

        public async UniTask SaveScore()
        {
            _distanceManager.ResetPostProcess();

            if (Score.Value > (uint)PlayerPrefs.GetInt("HighScore1") + (uint)PlayerPrefs.GetInt("HighScore2"))
            {
                if (Score.Value > int.MaxValue)
                {
                    PlayerPrefs.SetInt("HighScore1", int.MaxValue);
                    PlayerPrefs.SetInt("HighScore2", (int)(Score.Value - int.MaxValue));
                }
                else
                {
                    PlayerPrefs.SetInt("HighScore1", (int)Score.Value);
                }
            }

            ResultUI.HighScore = (uint)PlayerPrefs.GetInt("HighScore1") + (uint)PlayerPrefs.GetInt("HighScore2");
            ResultUI.CurrentScore = Score.Value;
            ResultUI.gameObject.SetActive(true);
            ResultUI.SetVisilityPressAnyKey(false);

            await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
            ResultUI.SetVisilityPressAnyKey(true);
            _input.actions["AnyKey"].performed += OnAnyKeyPerformed;

            void OnAnyKeyPerformed(InputAction.CallbackContext context)
            {
                _input.actions["AnyKey"].performed -= OnAnyKeyPerformed;
                _gameManager.Restart();
            }
        }

        /*
        // テスト用
        bool sw = true;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) { AddScoreAddition(100).Forget(); }
            if (Input.GetKeyDown(KeyCode.B)) { AddScoreMultiplication(4).Forget(); }
            if (Input.GetKeyDown(KeyCode.M)) { SetVisible(sw); sw = !sw; }
            if (Input.GetKeyDown(KeyCode.C)) { ResolveAddition().Forget(); }
            if (Input.GetKeyDown(KeyCode.V)) { ResolveMultiplication().Forget(); }
        }
        */
    }
}
