using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Utilities
{
    /// <summary>
    /// Scales a UI Image on a Canvas to fit the screen based on the image's aspect ratio.
    /// If the current image size is smaller than the computed size, it scales up by a factor.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CanvasImageScaler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Image Settings")]
        [Tooltip("Target Image to be scaled. If not assigned, the Image component on this GameObject will be used.")]
        [SerializeField] private Image targetImage;

        [Tooltip("Optional multiplier applied if the current size is smaller than the computed size.")]
        [SerializeField] private float scaleMultiplier = 1.5f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Cached RectTransform of the target image.
        /// </summary>
        private RectTransform targetRect;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Ensures that the target image is assigned and caches the RectTransform.
        /// </summary>
        private void Awake()
        {
            // If no target image is assigned, attempt to get the Image component from this GameObject.
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            // If still null, log an error and disable the script.
            if (targetImage == null)
            {
                Debug.LogError("CanvasImageScaler: No target image assigned or found.", this);
                enabled = false;
                return;
            }

            // Cache the RectTransform for later use.
            targetRect = targetImage.rectTransform;
        }

        /// <summary>
        /// Validates the target sprite and adjusts the image size.
        /// </summary>
        private void Start()
        {
            // Warn if the target image does not have a sprite assigned.
            if (targetImage.sprite == null)
            {
                Debug.LogWarning("CanvasImageScaler: Target image sprite is not assigned.", this);
                return;
            }

            AdjustImageSize();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Adjusts the image size so that it fits the screen while preserving the sprite's aspect ratio.
        /// If the current size is less than the computed size, the image is scaled up.
        /// The image is then centered within its parent Canvas.
        /// </summary>
        private void AdjustImageSize()
        {
            // Get the current screen dimensions and compute the screen aspect ratio.
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float screenRatio = screenWidth / screenHeight;

            // Retrieve the sprite and its dimensions.
            Sprite sprite = targetImage.sprite;
            Vector2 spriteSize = sprite.bounds.size;
            float imageRatio = spriteSize.x / spriteSize.y;

            float newWidth, newHeight;

            // Determine the new dimensions based on comparing screen and image aspect ratios.
            if (screenRatio >= imageRatio)
            {
                newWidth = screenWidth;
                newHeight = screenWidth / imageRatio;
            }
            else
            {
                newWidth = screenHeight * imageRatio;
                newHeight = screenHeight;
            }
            
            // Optionally scale up the image if the current size is smaller than the new size.
            Vector2 previousSize = targetRect.sizeDelta;
            if (previousSize.x > newWidth)
            {
                newWidth *= scaleMultiplier;
                newHeight *= scaleMultiplier;
            }

            // Apply the computed dimensions to the RectTransform.
            targetRect.sizeDelta = new Vector2(newWidth, newHeight);
            
            // Center the image by setting anchors, pivot, and anchored position to the center.
            Vector2 center = new Vector2(0.5f, 0.5f);
            targetRect.anchorMin = center;
            targetRect.anchorMax = center;
            targetRect.pivot = center;
            targetRect.anchoredPosition = Vector2.zero;
        }

        #endregion
    }
}
