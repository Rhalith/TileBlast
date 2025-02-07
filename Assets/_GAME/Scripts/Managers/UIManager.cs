using Scripts.Event;
using Scripts.Event.Events;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages UI elements such as score and movement text, including their animations and scene transitions.
    /// Listens to game events to update the UI accordingly.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Elements")]
        [Tooltip("Text component that displays the current score.")]
        [SerializeField] private TMP_Text scoreText;

        [Tooltip("Text component that displays the number of remaining moves.")]
        [SerializeField] private TMP_Text movementText;

        [Header("Animation Settings")]
        [Tooltip("Duration of the score update animation.")]
        [SerializeField] private float scoreAnimationDuration = 0.5f;

        [Tooltip("Duration of the movement text scale animation.")]
        [SerializeField] private float movementAnimationDuration = 0.3f;

        [Tooltip("Scale factor used to temporarily enlarge the movement text during an update.")]
        [SerializeField] private float movementScaleFactor = 1.2f;

        [Header("Scene Management")]
        [Tooltip("Name of the main menu scene to load.")]
        [SerializeField] private string mainMenuSceneName = "LevelSelection";

        #endregion

        #region Private Fields

        /// <summary>
        /// Holds the actual score value used for animation purposes.
        /// </summary>
        private float _actualScore;

        /// <summary>
        /// The original scale of the movement text.
        /// </summary>
        private float _currentMovementTextScale;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the UI by caching the initial movement text scale.
        /// </summary>
        private void Start()
        {
            if (movementText != null)
            {
                _currentMovementTextScale = movementText.transform.localScale.x;
            }
        }

        /// <summary>
        /// Subscribes to score and movement text change events.
        /// </summary>
        private void OnEnable()
        {
            EventBus<ChangeScoreTextEvent>.AddListener(ChangeScoreText);
            EventBus<ChangeMovementTextEvent>.AddListener(ChangeMovementText);
        }

        /// <summary>
        /// Unsubscribes from score and movement text change events.
        /// </summary>
        private void OnDisable()
        {
            EventBus<ChangeScoreTextEvent>.RemoveListener(ChangeScoreText);
            EventBus<ChangeMovementTextEvent>.RemoveListener(ChangeMovementText);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public void BackToMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Updates the score text based on the event data and animates the score change.
        /// If the score drops to zero or below, a win event is emitted.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Data containing the score change and whether this is the initial update.</param>
        private void ChangeScoreText(object sender, ChangeScoreTextEvent e)
        {
            // If this is the initial score setting, update immediately.
            if (e.IsInitial)
            {
                _actualScore = e.ScoreChange;
                scoreText.text = e.ScoreChange.ToString();
                return;
            }

            // Kill any running tween on the scoreText to avoid conflicting animations.
            DOTween.Kill(scoreText, true);

            // Decrease the actual score, ensuring it doesn't drop below zero.
            _actualScore = Mathf.Max(0, _actualScore - e.ScoreChange);

            // If the score reaches zero, emit a win event.
            if (_actualScore <= 0)
            {
                EventBus<FinishGameEvent>.Emit(this, new FinishGameEvent { IsWin = true });
            }

            // Animate the score text from its current value to the new actual score.
            DOTween.To(() => float.Parse(scoreText.text), x =>
                {
                    scoreText.text = Mathf.FloorToInt(x).ToString();
                },
                _actualScore, scoreAnimationDuration)
                .SetEase(Ease.OutQuad)
                .SetId(scoreText);
        }

        /// <summary>
        /// Updates the movement text with the new move count and plays a scale animation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Data containing the move count and whether this is the initial update.</param>
        private void ChangeMovementText(object sender, ChangeMovementTextEvent e)
        {
            movementText.text = "Moves: " + e.MovementCount;
            if (e.IsInitial) return;

            // Animate the movement text by scaling up and then returning to its original scale.
            movementText.transform
                .DOScale(_currentMovementTextScale * movementScaleFactor, movementAnimationDuration / 2)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => movementText.transform
                    .DOScale(_currentMovementTextScale, movementAnimationDuration / 2)
                    .SetEase(Ease.InQuad));
        }

        #endregion
    }
}
