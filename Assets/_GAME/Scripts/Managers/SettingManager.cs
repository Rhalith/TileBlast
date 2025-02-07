using UnityEngine;
using UnityEngine.UI;
using Scripts.Event.Events;
using Scripts.Event;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages the game's settings UI, including audio toggles and volume sliders,
    /// as well as toggling tile colliders.
    /// </summary>
    public class SettingManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio Settings")]
        [Tooltip("Toggle to enable or disable background music.")]
        [SerializeField] private Toggle musicToggle;

        [Tooltip("Toggle to enable or disable sound effects.")]
        [SerializeField] private Toggle sfxToggle;

        [Tooltip("Slider to adjust the background music volume.")]
        [SerializeField] private Slider musicSlider;

        [Tooltip("Slider to adjust the sound effects volume.")]
        [SerializeField] private Slider sfxSlider;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the settings UI by loading saved audio settings and setting up listeners.
        /// </summary>
        private void Start()
        {
            LoadSettings();
            SetupListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads the saved audio settings from the AudioManager.
        /// </summary>
        private void LoadSettings()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager instance not found. Settings will not be loaded.");
                return;
            }

            if (musicToggle != null)
            {
                musicToggle.isOn = AudioManager.Instance.IsMusicMuted;
            }

            if (sfxToggle != null)
            {
                sfxToggle.isOn = AudioManager.Instance.IsSfxMuted;
            }

            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.MusicVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.SFXVolume;
            }
        }

        /// <summary>
        /// Sets up UI event listeners for toggles and sliders.
        /// </summary>
        private void SetupListeners()
        {
            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(isOn => ToggleMusic(isOn));
            }

            if (sfxToggle != null)
            {
                sfxToggle.onValueChanged.AddListener(isOn => ToggleSFX(isOn));
            }

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(volume => SetMusicVolume(volume));
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(volume => SetSFXVolume(volume));
            }
        }

        #endregion

        #region Audio Control Methods

        /// <summary>
        /// Toggles the mute state of the background music.
        /// </summary>
        /// <param name="isOn">The new state of the music toggle (not used directly).</param>
        private void ToggleMusic(bool isOn)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMusicMute();
            }
        }

        /// <summary>
        /// Toggles the mute state of the sound effects.
        /// </summary>
        /// <param name="isOn">The new state of the SFX toggle (not used directly).</param>
        private void ToggleSFX(bool isOn)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleSFXMute();
            }
        }

        /// <summary>
        /// Sets the volume of the background music.
        /// </summary>
        /// <param name="volume">The new music volume level.</param>
        private void SetMusicVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(volume);
            }
        }

        /// <summary>
        /// Sets the volume of the sound effects.
        /// </summary>
        /// <param name="volume">The new sound effects volume level.</param>
        private void SetSFXVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }
        }

        #endregion

        #region Additional Settings

        /// <summary>
        /// Toggles the tile colliders in the game.
        /// </summary>
        /// <param name="isEnable">True to enable tile colliders; false to disable them.</param>
        public void ToggleTileCollider(bool isEnable)
        {
            EventBus<ToggleTileColliderEvent>.Emit(this, new ToggleTileColliderEvent { IsEnable = isEnable });
        }

        #endregion
    }
}
