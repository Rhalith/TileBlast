using UnityEngine;

namespace Scripts.Tiles
{
    /// <summary>
    /// Contains configuration data for a tile, including its icons and particle effects.
    /// </summary>
    [CreateAssetMenu(fileName = "TileData", menuName = "Game/Tile Data", order = 0)]
    public class TileData : ScriptableObject
    {
        #region Tile Icons

        [Header("Tile Icons")]
        [Tooltip("Default icon for this tile's color.")]
        public Sprite defaultIcon;

        [Tooltip("Icon for threshold A.")]
        public Sprite iconA;

        [Tooltip("Icon for threshold B.")]
        public Sprite iconB;

        [Tooltip("Icon for threshold C.")]
        public Sprite iconC;

        #endregion

        #region Particle Effects

        [Header("Particle Effects")]
        [Tooltip("Particle effect for this tile's color.")]
        public Sprite particle;

        [Tooltip("Shiny particle effect for this tile's color.")]
        public Sprite shinyParticle;

        #endregion
    }
}