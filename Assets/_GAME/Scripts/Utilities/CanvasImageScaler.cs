using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Utilities
{
    [RequireComponent(typeof(Image))]
    public class CanvasImageScaler : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        private RectTransform targetRect;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage == null)
            {
                Debug.LogError("No target image assigned or found.", this);
                enabled = false;
                return;
            }
            
            targetRect = targetImage.rectTransform;
        }

        private void Start()
        {
            if (targetImage.sprite == null)
            {
                Debug.LogWarning("Target image sprite is not assigned.", this);
                return;
            }
            
            AdjustImageSize();
        }

        private void AdjustImageSize()
        {
            // Get screen dimensions and ratios
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float screenRatio = screenWidth / screenHeight;

            // Get sprite size and ratio
            Sprite sprite = targetImage.sprite;
            Vector2 spriteSize = sprite.bounds.size;
            float imageRatio = spriteSize.x / spriteSize.y;

            // Compute new size based on screen and image ratios
            float newWidth, newHeight;
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
            
            // Compare with previous size; if the new size is smaller, scale it up by 1.5.
            Vector2 previousSize = targetRect.sizeDelta;
            if (previousSize.x > newWidth)
            {
                newWidth *= 1.5f;
                newHeight *= 1.5f;
            }

            targetRect.sizeDelta = new Vector2(newWidth, newHeight);
                
            Vector2 center = new Vector2(0.5f, 0.5f);
            targetRect.anchorMin = center;
            targetRect.anchorMax = center;
            targetRect.pivot = center;
            targetRect.anchoredPosition = Vector2.zero;
        }
    }
}
