using System.Collections.Generic;
using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip multiClickSound;

        private List<AudioSource> _audioSources = new ();
        private AudioSource _musicSource;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMusicMuted;
        private bool _isSFXMuted;

        public static AudioManager Instance { get; private set; }

        public float MusicVolume => _musicVolume;

        public float SFXVolume => _sfxVolume;
        public bool IsMusicMuted => _isMusicMuted;
        public bool IsSfxMuted => _isSFXMuted;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ClearSoundSettings();
                InitializeAudioSources();
                LoadSoundSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventBus<PlaySoundEvent>.AddListener(PlaySound);
        }

        private void OnDisable()
        {
            EventBus<PlaySoundEvent>.RemoveListener(PlaySound);
        }

        private void InitializeAudioSources()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.clip = backgroundMusic;
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            _musicSource.mute = _isMusicMuted;
            _musicSource.Play();

            for (int i = 0; i < 5; i++) // Pooling 5 audio sources for simultaneous sounds
            {
                Debug.Log("isSFXMuted: " + _isSFXMuted);
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
                source.mute = _isSFXMuted;
                _audioSources.Add(source);
            }
        }

        private void PlaySound(object sender, PlaySoundEvent @event)
        {
            if (_isSFXMuted) return;

            AudioClip clipToPlay = GetClip(@event.SoundType);
            if (clipToPlay != null)
            {
                PlayClip(clipToPlay);
            }
        }

        private AudioClip GetClip(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Win => winSound,
                SoundType.Lose => loseSound,
                SoundType.Click => clickSound,
                SoundType.MultiClick => multiClickSound,
                _ => null
            };
        }

        private void PlayClip(AudioClip clip)
        {
            AudioSource availableSource = _audioSources.Find(source => !source.isPlaying);
            if (availableSource == null)
            {
                availableSource = gameObject.AddComponent<AudioSource>();
                _audioSources.Add(availableSource);
            }

            availableSource.clip = clip;
            availableSource.volume = _isSFXMuted ? 0f : _sfxVolume;
            availableSource.Play();
        }

        // --- SOUND CONTROL METHODS ---
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp(volume, 0f, 1f);
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            SaveSoundSettings();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp(volume, 0f, 1f);
            foreach (var source in _audioSources)
            {
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
            }
            SaveSoundSettings();
        }

        public void ToggleMusicMute()
        {
            Debug.Log("Toggling music mute");
            _isMusicMuted = !_isMusicMuted;
            _musicSource.mute = _isMusicMuted;
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            SaveSoundSettings();
        }

        public void ToggleSFXMute()
        {
            Debug.Log("Toggling SFX mute");
            _isSFXMuted = !_isSFXMuted;
            foreach (var source in _audioSources)
            {
                source.mute = _isSFXMuted;
                source.volume = _isSFXMuted ? 0f : _sfxVolume;
            }
            SaveSoundSettings();
        }

        private void SaveSoundSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.SetInt("MusicMuted", _isMusicMuted ? 1 : 0);
            PlayerPrefs.SetInt("SFXMuted", _isSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

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
        
        private void ClearSoundSettings()
        {
            PlayerPrefs.DeleteKey("MusicVolume");
            PlayerPrefs.DeleteKey("SFXVolume");
            PlayerPrefs.DeleteKey("MusicMuted");
            PlayerPrefs.DeleteKey("SFXMuted");
            PlayerPrefs.Save();
        }
    }

    public enum SoundType
    {
        Win,
        Lose,
        Click,
        MultiClick
    }
}
