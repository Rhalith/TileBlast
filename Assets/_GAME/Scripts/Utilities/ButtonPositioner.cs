using UnityEngine;

namespace Scripts.Utilities
{

    public class ButtonPositioner : MonoBehaviour
    {
        [SerializeField] private PositionType positionType;
        [SerializeField] [Range(0f, 0.1f)] private float paddingPercentage = 0.05f;

        private RectTransform _rectTransform;
        private Canvas _canvas;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_canvas == null || _rectTransform == null)
            {
                Debug.LogError("ButtonPositioner: Missing Canvas or RectTransform!");
                return;
            }

            ChangePosition();
        }

        private void ChangePosition()
        {
            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float adaptivePaddingX = screenWidth * paddingPercentage;
            float adaptivePaddingY = screenHeight * paddingPercentage;

            Vector2 screenPosition = Vector2.zero;

            switch (positionType)
            {
                case PositionType.TopLeft:
                    screenPosition = new Vector2(adaptivePaddingX, screenHeight - adaptivePaddingY);
                    break;

                case PositionType.TopRight:
                    screenPosition = new Vector2(screenWidth - adaptivePaddingX, screenHeight - adaptivePaddingY);
                    break;

                case PositionType.BottomLeft:
                    screenPosition = new Vector2(adaptivePaddingX, adaptivePaddingY);
                    break;

                case PositionType.BottomRight:
                    screenPosition = new Vector2(screenWidth - adaptivePaddingX, adaptivePaddingY);
                    break;

                default:
                    Debug.LogError("ButtonPositioner: Invalid Position Type");
                    return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(),
                screenPosition,
                uiCamera,
                out Vector2 localPoint
            );

            _rectTransform.localPosition = localPoint;
        }
    }

    public enum PositionType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

}