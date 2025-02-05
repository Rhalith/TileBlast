using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Utilities
{

    public class CanvasImageScaler : MonoBehaviour
    {
        [SerializeField] private Image targetImage;

        private void Start()
        {
            FitImageToScreen();
        }

        private void FitImageToScreen()
        {
            if (targetImage == null) return;

            RectTransform imageRectTransform = targetImage.rectTransform;
            float screenRatio = (float)Screen.width / Screen.height;
            float imageRatio = targetImage.sprite.bounds.size.x / targetImage.sprite.bounds.size.y;

            if (screenRatio > imageRatio)
            {
                imageRectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / imageRatio);
            }
            else
            {
                imageRectTransform.sizeDelta = new Vector2(Screen.height * imageRatio, Screen.height);
            }
        }
    }

}