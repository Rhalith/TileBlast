using UnityEngine;

namespace Scripts.Utilities
{
    /// <summary>
    /// Positions a UI button in a specified corner of the screen using adaptive padding.
    /// </summary>
    public class ButtonPositioner : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Position Settings")]
        [Tooltip("Select the corner of the screen where the button will be positioned.")]
        [SerializeField] private PositionType positionType;

        [Tooltip("Percentage of the screen used as padding. Acceptable values range from 0 to 0.1.")]
        [SerializeField, Range(0f, 0.1f)] private float paddingPercentage = 0.05f;

        #endregion

        #region Private Fields

        /// <summary>
        /// RectTransform component of the UI element.
        /// </summary>
        private RectTransform _rectTransform;

        /// <summary>
        /// The parent Canvas containing the UI element.
        /// </summary>
        private Canvas _canvas;

        #endregion

        #region Unity Methods

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_canvas == null || _rectTransform == null)
            {
                Debug.LogError("ButtonPositioner: Missing Canvas or RectTransform!");
                return;
            }

            PositionButton();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Positions the button based on the selected position type and adaptive padding.
        /// </summary>
        private void PositionButton()
        {
            // Determine the appropriate camera for UI rendering.
            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Calculate adaptive padding.
            float adaptivePaddingX = screenWidth * paddingPercentage;
            float adaptivePaddingY = screenHeight * paddingPercentage;

            Vector2 screenPosition = Vector2.zero;

            // Set the screen position based on the specified position type.
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

            // Convert the screen position to a local point in the Canvas.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.GetComponent<RectTransform>(),
                    screenPosition,
                    uiCamera,
                    out Vector2 localPoint))
            {
                _rectTransform.localPosition = localPoint;
            }
            else
            {
                Debug.LogError("ButtonPositioner: Failed to convert screen point to local point.");
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines the possible screen corners for positioning the UI button.
    /// </summary>
    public enum PositionType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
