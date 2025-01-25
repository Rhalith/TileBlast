using UnityEngine;

namespace Scripts.Tiles
{
    public class Tile : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Vector2Int GridPosition { get; set; }
        private int colorID;

        public void Initialize(int color, Sprite[] icons)
        {
            colorID = color;
            spriteRenderer.sprite = icons[color];
        }

        public bool IsSameColor(Tile otherTile)
        {
            return otherTile != null && colorID == otherTile.colorID;
        }

        public void ClearTile()
        {
            Destroy(gameObject);
        }

        public void MoveTo(Vector2 targetPosition)
        {
            transform.position = targetPosition;
        }
    }

}