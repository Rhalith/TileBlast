using System;
using Scripts.Event;
using Scripts.Event.Events;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movementText;
        [SerializeField] private float scoreAnimationDuration = 0.5f;
        [SerializeField] private float movementAnimationDuration = 0.3f;
        [SerializeField] private float movementScaleFactor = 1.2f;
        [SerializeField] private string mainMenuSceneName = "LevelSelection";
        
        private float _actualScore;
        private float _currentMovementTextScale;

        private void Start()
        {
            _currentMovementTextScale = movementText.transform.localScale.x;
        }

        private void OnEnable()
        {
            EventBus<ChangeScoreTextEvent>.AddListener(ChangeScoreText);
            EventBus<ChangeMovementTextEvent>.AddListener(ChangeMovementText);
        }

        private void OnDisable()
        {
            EventBus<ChangeScoreTextEvent>.RemoveListener(ChangeScoreText);
            EventBus<ChangeMovementTextEvent>.RemoveListener(ChangeMovementText);
        }
        
        public void BackToMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void ChangeScoreText(object sender, ChangeScoreTextEvent e)
        {
            if (e.IsInitial)
            {
                _actualScore = e.ScoreChange;
                scoreText.text = e.ScoreChange.ToString();
                return;
            }
            DOTween.Kill(scoreText, true);

            _actualScore = Mathf.Max(0, _actualScore - e.ScoreChange);

            DOTween.To(() => float.Parse(scoreText.text), x =>
                {
                    scoreText.text = Mathf.FloorToInt(x).ToString();
                }, _actualScore, scoreAnimationDuration)
                .SetEase(Ease.OutQuad)
                .SetId(scoreText);
        }

        private void ChangeMovementText(object sender, ChangeMovementTextEvent e)
        {
            movementText.text = "Moves: " + e.MovementCount;
            if (e.IsInitial) return;
            movementText.transform.DOScale(_currentMovementTextScale * movementScaleFactor, movementAnimationDuration / 2)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => movementText.transform.DOScale(_currentMovementTextScale, movementAnimationDuration / 2)
                    .SetEase(Ease.InQuad));
        }
    }
}
