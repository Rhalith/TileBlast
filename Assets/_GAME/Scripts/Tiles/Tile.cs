using System;
using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Tiles
{
    public class Tile : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Vector2Int GridPosition { get; set; }
        private TileData tileData;
        private int thresholdA;
        private int thresholdB;
        private int thresholdC;

        private void OnMouseDown()
        {
            EventBus<TileClickedEvent>.Emit(this, new TileClickedEvent{ClickedTile = this});
        }

        public void Initialize(TileData data, int tA, int tB, int tC)
        {
            tileData = data;
            spriteRenderer.sprite = data.defaultIcon;
            thresholdA = tA;
            thresholdB = tB;
            thresholdC = tC;
        }

        public bool IsSameColor(Tile otherTile)
        {
            return otherTile != null && tileData == otherTile.tileData;
        }

        public void ClearTile()
        {
            Destroy(gameObject);
        }

        public void MoveTo(Vector2 targetPosition)
        {
            transform.position = targetPosition;
        }

        public void AssignIcon(int groupSize)
        {
            if (groupSize >= thresholdC)
            {
                spriteRenderer.sprite = tileData.iconC;
            }
            else if (groupSize >= thresholdB)
            {
                spriteRenderer.sprite = tileData.iconB;
            }
            else if (groupSize >= thresholdA)
            {
                spriteRenderer.sprite = tileData.iconA;
            }
            else
            {
                spriteRenderer.sprite = tileData.defaultIcon;
            }
        }
    }
}