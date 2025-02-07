using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages particle effects for the game.
    /// Listens for game finish events and, if the game is won, plays the appropriate particle effects.
    /// </summary>
    public class ParticleManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Particle Systems")]
        [Tooltip("Particle system for the left side effect.")]
        [SerializeField] private ParticleSystem leftParticle;

        [Tooltip("Particle system for the right side effect.")]
        [SerializeField] private ParticleSystem rightParticle;

        #endregion

        #region Private Fields

        /// <summary>
        /// Reference to the main orthographic camera for positioning particle systems.
        /// </summary>
        private Camera _orthoCamera;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            // Ensure all particle systems are stopped at the start.
            StopAllParticles();
        }

        private void Start()
        {
            // Cache the main camera reference.
            _orthoCamera = Camera.main;
            SetParticlePosition();
        }

        private void OnEnable()
        {
            // Subscribe to finish game events.
            EventBus<FinishGameEvent>.AddListener(PlayParticleEffect);
        }

        private void OnDisable()
        {
            // Unsubscribe from finish game events.
            EventBus<FinishGameEvent>.RemoveListener(PlayParticleEffect);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the finish game event. If the player wins, triggers win sounds and particle effects.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The finish game event data.</param>
        private void PlayParticleEffect(object sender, FinishGameEvent e)
        {
            if (e.IsWin)
            {
                // Emit a win sound event.
                EventBus<PlaySoundEvent>.Emit(this, new PlaySoundEvent { SoundType = SoundType.Win });
                // Play the win particle effects.
                PlayWinParticles();
            }
        }

        #endregion

        #region Particle Methods

        /// <summary>
        /// Stops both the left and right particle systems.
        /// </summary>
        private void StopAllParticles()
        {
            if (leftParticle != null)
            {
                leftParticle.Stop();
            }

            if (rightParticle != null)
            {
                rightParticle.Stop();
            }
        }

        /// <summary>
        /// Sets the positions of the particle systems to the left and right edges of the screen.
        /// </summary>
        private void SetParticlePosition()
        {
            if (_orthoCamera == null)
            {
                Debug.LogWarning("Main camera not found. Cannot position particles.");
                return;
            }

            // Convert viewport coordinates to world positions.
            Vector3 leftParticlePosition = _orthoCamera.ViewportToWorldPoint(new Vector3(0, 0, _orthoCamera.nearClipPlane));
            Vector3 rightParticlePosition = _orthoCamera.ViewportToWorldPoint(new Vector3(1, 0, _orthoCamera.nearClipPlane));

            if (leftParticle != null)
            {
                leftParticle.transform.position = leftParticlePosition;
            }

            if (rightParticle != null)
            {
                rightParticle.transform.position = rightParticlePosition;
            }
        }

        /// <summary>
        /// Plays the win particle effects.
        /// </summary>
        private void PlayWinParticles()
        {
            if (leftParticle != null)
            {
                leftParticle.Play();
            }

            if (rightParticle != null)
            {
                rightParticle.Play();
            }
        }

        #endregion
    }
}
