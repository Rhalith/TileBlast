using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Scripts.Tiles
{   
    public class TileParticleController : MonoBehaviour
    {
        [SerializeField] private int minParticles = 6;
        [SerializeField] private int maxParticles = 8;
        [SerializeField] private float moveDuration = 0.6f;

        private void Start()
        {
            Destroy(gameObject, moveDuration + 0.3f);
        }

        public void PlayEffect(TileData tileData, SpriteRenderer tileRenderer)
        {
            int particleCount = Random.Range(minParticles, maxParticles + 1);

            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = new GameObject("TileParticle");
                SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();

                particleRenderer.sprite = tileRenderer.sprite == tileData.iconC ? tileData.shinyParticle : tileData.particle;
                particleRenderer.sortingLayerName = "Particles";
                particleRenderer.sortingOrder = tileRenderer.sortingOrder + 10;

                Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
                particle.transform.position = tileRenderer.transform.position + randomOffset;

                float randomRotation = Random.Range(0f, 360f);
                particle.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

                particle.transform.localScale = Vector3.one * 0.15f;

                bool moveLeft = i % 2 == 0;
                Vector3[] path = GetCurvedPath(particle.transform.position, moveLeft);

                AnimateAndDestroy(particle, particleRenderer, path);
            }
        }

        private Vector3[] GetCurvedPath(Vector3 startPos, bool moveLeft)
        {
            float horizontalDistance = Random.Range(1.5f, 2.5f);
            float peakHeight = Random.Range(0.1f, 0.2f);

            Vector3 midPoint = startPos + new Vector3(moveLeft ? -horizontalDistance / 2 : horizontalDistance / 2, peakHeight, 0);
            Vector3 endPoint = startPos + new Vector3(moveLeft ? -horizontalDistance : horizontalDistance, -peakHeight * 30f, 0);

            return new[] { midPoint, endPoint };
        }

        private void AnimateAndDestroy(GameObject particle, SpriteRenderer particleRenderer, Vector3[] path)
        {
            particle.transform.DOPath(path, moveDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            particleRenderer.DOFade(0, moveDuration)
                .OnComplete(() => Destroy(particle));
        }
    }
}
