using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Managers
{

    public class ParticleManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem leftParticle;
        [SerializeField] private ParticleSystem rightParticle;

        private Camera _orthoCamera;

        private void Awake()
        {
            StopAllParticles();
        }

        private void Start()
        {
            _orthoCamera = Camera.main;
            SetParticlePosition();
        }

        private void OnEnable()
        {
            EventBus<FinishGameEvent>.AddListener(PlayParticleEffect);
        }
        
        private void OnDisable()
        {
            EventBus<FinishGameEvent>.RemoveListener(PlayParticleEffect);
        }

        private void PlayParticleEffect(object sender, FinishGameEvent @event)
        {
            if (@event.IsWin)
            {
                PlayWinParticles();
            }
        }

        private void StopAllParticles()
        {
            leftParticle.Stop();
            rightParticle.Stop();
        }

        private void SetParticlePosition()
        {
            Vector3 leftParticlePosition =
                _orthoCamera.ViewportToWorldPoint(new Vector3(0, 0, _orthoCamera.nearClipPlane));
            Vector3 rightParticlePosition =
                _orthoCamera.ViewportToWorldPoint(new Vector3(1, 0, _orthoCamera.nearClipPlane));

            leftParticle.transform.position = leftParticlePosition;
            rightParticle.transform.position = rightParticlePosition;
        }

        private void PlayWinParticles()
        {
            leftParticle.Play();
            rightParticle.Play();
        }
    }

}