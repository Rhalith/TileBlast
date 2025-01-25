namespace Scripts.Tiles
{

    using UnityEngine;

    [CreateAssetMenu(fileName = "TileData", menuName = "Game/Tile Data", order = 0)]
    public class TileData : ScriptableObject
    {
        public Sprite defaultIcon; // Default icon for this color
        public Sprite iconA;       // Icon for threshold A
        public Sprite iconB;       // Icon for threshold B
        public Sprite iconC;       // Icon for threshold C
    }
}