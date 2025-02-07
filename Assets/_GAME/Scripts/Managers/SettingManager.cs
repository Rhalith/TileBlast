using UnityEngine;
using UnityEngine.UI;
using Scripts.Event.Events;
using Scripts.Event;

namespace Scripts.Managers
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private void Start()
        {
            LoadSettings();

            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(delegate { ToggleMusic(musicToggle.isOn); });
            }

            if (sfxToggle != null)
            {
                sfxToggle.onValueChanged.AddListener(delegate { ToggleSFX(sfxToggle.isOn); });
            }

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicSlider.value); });
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(sfxSlider.value); });
            }
        }

        private void LoadSettings()
        {
            if (AudioManager.Instance != null)
            {
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
        }

        private void ToggleMusic(bool isOn)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMusicMute();
            }
        }

        private void ToggleSFX(bool isOn)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleSFXMute();
            }
        }

        private void SetMusicVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(volume);
            }
        }

        private void SetSFXVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }
        }
        
        public void ToggleTileCollider(bool isEnable)
        {
            EventBus<ToggleTileColliderEvent>.Emit(this, new ToggleTileColliderEvent { IsEnable = isEnable });
        }
    }
}
