using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Tiles
{
    /// <summary>
    /// Represents a single tile in the game grid.
    /// Handles initialization, user input, visual updates, and interaction with game events.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Tile Components")]
        [Tooltip("The BoxCollider2D component used for tile collision detection.")]
        [SerializeField] private BoxCollider2D boxCollider;

        [Tooltip("The SpriteRenderer component used to display the tile's icon.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Prefab for the particle effect to play when the tile is clicked.")]
        [SerializeField] private GameObject particleEffectPrefab;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the grid position of the tile.
        /// </summary>
        public Vector2Int GridPosition { get; set; }

        #endregion

        #region Private Fields

        private TileData _tileData;
        private int _thresholdA;
        private int _thresholdB;
        private int _thresholdC;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            EventBus<FinishGameEvent>.AddListener(OnFinishGame);
            EventBus<ToggleTileColliderEvent>.AddListener(OnToggleCollider);
        }

        private void OnDisable()
        {
            EventBus<FinishGameEvent>.RemoveListener(OnFinishGame);
            EventBus<ToggleTileColliderEvent>.RemoveListener(OnToggleCollider);
        }

        private void OnMouseDown()
        {
            // Emit the tile click event when this tile is clicked.
            EventBus<TileClickedEvent>.Emit(this, new TileClickedEvent { ClickedTile = this });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Disables the tile's collider when the game finishes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">FinishGameEvent data.</param>
        private void OnFinishGame(object sender, FinishGameEvent e)
        {
            ToggleCollider(false);
        }

        /// <summary>
        /// Toggles the tile collider based on the event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">ToggleTileColliderEvent data.</param>
        private void OnToggleCollider(object sender, ToggleTileColliderEvent e)
        {
            ToggleCollider(e.IsEnable);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Enables or disables the tile's collider.
        /// </summary>
        /// <param name="isEnable">True to enable the collider; false to disable it.</param>
        private void ToggleCollider(bool isEnable)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = isEnable;
            }
            else
            {
                Debug.LogWarning($"{name}: BoxCollider2D is not assigned.");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the tile with the specified data and threshold values.
        /// </summary>
        /// <param name="data">Tile data containing icon references.</param>
        /// <param name="tA">Threshold for icon A.</param>
        /// <param name="tB">Threshold for icon B.</param>
        /// <param name="tC">Threshold for icon C.</param>
        public void Initialize(TileData data, int tA, int tB, int tC)
        {
            _tileData = data;
            spriteRenderer.sprite = data.defaultIcon;
            _thresholdA = tA;
            _thresholdB = tB;
            _thresholdC = tC;
        }

        /// <summary>
        /// Checks if this tile has the same color as another tile.
        /// </summary>
        /// <param name="otherTile">The other tile to compare.</param>
        /// <returns>True if the tiles share the same tile data; otherwise, false.</returns>
        public bool IsSameColor(Tile otherTile)
        {
            return otherTile != null && _tileData == otherTile._tileData;
        }

        /// <summary>
        /// Clears the tile by destroying its game object.
        /// </summary>
        public void ClearTile()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Spawns and plays the particle effect associated with the tile.
        /// </summary>
        public void SpawnParticleEffect()
        {
            if (particleEffectPrefab != null)
            {
                GameObject particleInstance = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
                TileParticleController particleController = particleInstance.GetComponent<TileParticleController>();

                if (particleController != null)
                {
                    particleController.PlayEffect(_tileData, spriteRenderer);
                }
            }
        }

        /// <summary>
        /// Assigns an icon to the tile based on the size of the connected group.
        /// </summary>
        /// <param name="groupSize">The number of connected tiles in the group.</param>
        public void AssignIcon(int groupSize)
        {
            if (groupSize >= _thresholdC)
            {
                spriteRenderer.sprite = _tileData.iconC;
            }
            else if (groupSize >= _thresholdB)
            {
                spriteRenderer.sprite = _tileData.iconB;
            }
            else if (groupSize >= _thresholdA)
            {
                spriteRenderer.sprite = _tileData.iconA;
            }
            else
            {
                spriteRenderer.sprite = _tileData.defaultIcon;
            }
        }

        #endregion
    }
}
