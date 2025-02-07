using System.Collections.Generic;
using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages audio playback for background music and sound effects.
    /// This singleton handles sound events, audio source pooling, volume controls, and saving/loading audio settings.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio Clips")]
        [Tooltip("Background music clip played continuously.")]
        [SerializeField] private AudioClip backgroundMusic;

        [Tooltip("Sound clip for win events.")]
        [SerializeField] private AudioClip winSound;

        [Tooltip("Sound clip for lose events.")]
        [SerializeField] private AudioClip loseSound;

        [Tooltip("Sound clip for button click events.")]
        [SerializeField] private AudioClip clickSound;

        [Tooltip("Sound clip for multiple rapid click events.")]
        [SerializeField] private AudioClip multiClickSound;

        #endregion

        #region Private Fields

        /// <summary>
        /// Pool of audio sources used for playing sound effects.
        /// </summary>
        private List<AudioSource> _audioSources = new ();

        /// <summary>
        /// Audio source dedicated to background music.
        /// </summary>
        private AudioSource _musicSource;

        /// <summary>
        /// Current volume level for background music.
        /// </summary>
        private float _musicVolume = 1f;

        /// <summary>
        /// Current volume level for sound effects.
        /// </summary>
        private float _sfxVolume = 1f;

        /// <summary>
        /// Indicates whether the background music is muted.
        /// </summary>
        private bool _isMusicMuted;

        /// <summary>
        /// Indicates whether the sound effects are muted.
        /// </summary>
        private bool _isSFXMuted;

        #endregion

        #region Public Properties

        /// <summary>
        /// Singleton instance of the AudioManager.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        /// <summary>
        /// Gets the current music volume.
        /// </summary>
        public float MusicVolume => _musicVolume;

        /// <summary>
        /// Gets the current sound effects volume.
        /// </summary>
        public float SFXVolume => _sfxVolume;

        /// <summary>
        /// Gets a value indicating whether the background music is muted.
        /// </summary>
        public bool IsMusicMuted => _isMusicMuted;

        /// <summary>
        /// Gets a value indicating whether the sound effects are muted.
        /// </summary>
        public bool IsSfxMuted => _isSFXMuted;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Clear any previous settings to ensure a fresh start.
                ClearSoundSettings();

                // Initialize audio sources for music and SFX.
                InitializeAudioSources();

                // Load saved audio settings.
                LoadSoundSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // Subscribe to play sound events.
            EventBus<PlaySoundEvent>.AddListener(PlaySound);
        }

        private void OnDisable()
        {
            // Unsubscribe from play sound events.
            EventBus<PlaySoundEvent>.RemoveListener(PlaySound);
        }

        #endregion

        #region Audio Initialization and Playback

        /// <summary>
        /// Initializes the audio sources for background music and sound effects.
        /// </summary>
        private void InitializeAudioSources()
        {
            // Create and configure the background music source.
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.clip = backgroundMusic;
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            _musicSource.mute = _isMusicMuted;
            _musicSource.Play();

            // Create a pool of audio sources for simultaneous sound effects.
            for (int i = 0; i < 5; i++)
            {
                Debug.Log("isSFXMuted: " + _isSFXMuted);
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
                source.mute = _isSFXMuted;
                _audioSources.Add(source);
            }
        }

        /// <summary>
        /// Handles the play sound event by playing the appropriate clip.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="event">The PlaySoundEvent containing sound type information.</param>
        private void PlaySound(object sender, PlaySoundEvent @event)
        {
            if (_isSFXMuted) return;

            AudioClip clipToPlay = GetClip(@event.SoundType);
            if (clipToPlay != null)
            {
                PlayClip(clipToPlay);
            }
        }

        /// <summary>
        /// Retrieves the audio clip corresponding to the given sound type.
        /// </summary>
        /// <param name="soundType">The type of sound to retrieve.</param>
        /// <returns>The associated AudioClip, or null if not defined.</returns>
        private AudioClip GetClip(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Win => winSound,
                SoundType.Lose => loseSound,
                SoundType.Click => clickSound,
                SoundType.MultiClick => multiClickSound,
                _ => null,
            };
        }

        /// <summary>
        /// Plays the specified audio clip using an available audio source.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        private void PlayClip(AudioClip clip)
        {
            // Try to find an audio source that is not currently playing.
            AudioSource availableSource = _audioSources.Find(source => !source.isPlaying);
            if (availableSource == null)
            {
                // If none are available, create a new audio source and add it to the pool.
                availableSource = gameObject.AddComponent<AudioSource>();
                _audioSources.Add(availableSource);
            }

            availableSource.clip = clip;
            availableSource.volume = _isSFXMuted ? 0f : _sfxVolume;
            availableSource.Play();
        }

        #endregion

        #region Sound Control Methods

        /// <summary>
        /// Sets the background music volume and updates the audio source.
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0).</param>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp(volume, 0f, 1f);
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            SaveSoundSettings();
        }

        /// <summary>
        /// Sets the sound effects volume and updates all SFX audio sources.
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0).</param>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp(volume, 0f, 1f);
            foreach (var source in _audioSources)
            {
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
            }
            SaveSoundSettings();
        }

        /// <summary>
        /// Toggles the mute state of the background music.
        /// </summary>
        public void ToggleMusicMute()
        {
            _isMusicMuted = !_isMusicMuted;
            _musicSource.mute = _isMusicMuted;
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            SaveSoundSettings();
        }

        /// <summary>
        /// Toggles the mute state of the sound effects.
        /// </summary>
        public void ToggleSFXMute()
        {
            _isSFXMuted = !_isSFXMuted;
            foreach (var source in _audioSources)
            {
                source.mute = _isSFXMuted;
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
            }
            SaveSoundSettings();
        }

        #endregion

        #region Settings Persistence

        /// <summary>
        /// Saves the current sound settings to PlayerPrefs.
        /// </summary>
        private void SaveSoundSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.SetInt("MusicMuted", _isMusicMuted ? 1 : 0);
            PlayerPrefs.SetInt("SFXMuted", _isSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads saved sound settings from PlayerPrefs and applies them.
        /// </summary>
        private void LoadSoundSettings()
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            _isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            _isSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            _musicSource.mute = _isMusicMuted;
            foreach (var source in _audioSources)
            {
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
                source.mute = _isSFXMuted;
            }
        }

        /// <summary>
        /// Clears previously saved sound settings from PlayerPrefs.
        /// </summary>
        private void ClearSoundSettings()
        {
            PlayerPrefs.DeleteKey("MusicVolume");
            PlayerPrefs.DeleteKey("SFXVolume");
            PlayerPrefs.DeleteKey("MusicMuted");
            PlayerPrefs.DeleteKey("SFXMuted");
            PlayerPrefs.Save();
        }

        #endregion
    }

    /// <summary>
    /// Enum defining different types of sound effects.
    /// </summary>
    public enum SoundType
    {
        Win,
        Lose,
        Click,
        MultiClick
    }
}
