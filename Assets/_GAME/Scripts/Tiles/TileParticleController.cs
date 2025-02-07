using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Scripts.Tiles
{
    /// <summary>
    /// Controls and animates particle effects for tiles.
    /// Spawns a number of particles with random offsets and curved paths,
    /// fades them out, and destroys them after the animation completes.
    /// </summary>
    public class TileParticleController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Particle Settings")]
        [Tooltip("Minimum number of particles to spawn.")]
        [SerializeField] private int minParticles = 6;

        [Tooltip("Maximum number of particles to spawn.")]
        [SerializeField] private int maxParticles = 8;

        [Tooltip("Duration for the particle movement animation.")]
        [SerializeField] private float moveDuration = 0.6f;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the particle controller and schedules its destruction.
        /// </summary>
        private void Start()
        {
            // Destroy this GameObject after the animation completes plus a small buffer.
            Destroy(gameObject, moveDuration + 0.3f);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays the particle effect based on the given tile data and tile renderer.
        /// Spawns multiple particles with random offsets, applies a curved animation,
        /// and fades them out before destroying.
        /// </summary>
        /// <param name="tileData">The tile data containing particle sprites.</param>
        /// <param name="tileRenderer">The SpriteRenderer of the tile (for position and sorting order).</param>
        public void PlayEffect(TileData tileData, SpriteRenderer tileRenderer)
        {
            // Determine the number of particles to spawn.
            int particleCount = Random.Range(minParticles, maxParticles + 1);

            for (int i = 0; i < particleCount; i++)
            {
                // Create a new GameObject for the particle.
                GameObject particle = new GameObject("TileParticle");
                SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();

                // Choose the appropriate particle sprite.
                particleRenderer.sprite = tileRenderer.sprite == tileData.iconC ? tileData.shinyParticle : tileData.particle;
                particleRenderer.sortingLayerName = "Particles";
                particleRenderer.sortingOrder = tileRenderer.sortingOrder + 10;

                // Set a random offset around the tile's position.
                Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
                particle.transform.position = tileRenderer.transform.position + randomOffset;

                // Apply a random rotation.
                float randomRotation = Random.Range(0f, 360f);
                particle.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

                // Set the particle's scale.
                particle.transform.localScale = Vector3.one * 0.15f;

                // Determine the direction of the curved path.
                bool moveLeft = i % 2 == 0;
                Vector3[] path = GetCurvedPath(particle.transform.position, moveLeft);

                // Animate the particle along the path and fade it out.
                AnimateAndDestroy(particle, particleRenderer, path);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generates a curved path for the particle's movement.
        /// </summary>
        /// <param name="startPos">The starting position of the particle.</param>
        /// <param name="moveLeft">True if the particle should move left, false for right.</param>
        /// <returns>An array of Vector3 positions representing the path.</returns>
        private Vector3[] GetCurvedPath(Vector3 startPos, bool moveLeft)
        {
            float horizontalDistance = Random.Range(1.5f, 2.5f);
            float peakHeight = Random.Range(0.1f, 0.2f);

            // Calculate the midpoint and end point of the path.
            Vector3 midPoint = startPos + new Vector3(moveLeft ? -horizontalDistance / 2 : horizontalDistance / 2, peakHeight, 0);
            Vector3 endPoint = startPos + new Vector3(moveLeft ? -horizontalDistance : horizontalDistance, -peakHeight * 30f, 0);

            return new[] { midPoint, endPoint };
        }

        /// <summary>
        /// Animates the particle along a curved path, fades it out, and destroys it after the animation.
        /// </summary>
        /// <param name="particle">The particle GameObject to animate.</param>
        /// <param name="particleRenderer">The SpriteRenderer component of the particle.</param>
        /// <param name="path">The animation path as an array of Vector3 positions.</param>
        private void AnimateAndDestroy(GameObject particle, SpriteRenderer particleRenderer, Vector3[] path)
        {
            // Animate the particle along a Catmull-Rom spline.
            particle.transform.DOPath(path, moveDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            // Fade out the particle and destroy it upon completion.
            particleRenderer.DOFade(0, moveDuration)
                .OnComplete(() => Destroy(particle));
        }

        #endregion
    }
}
