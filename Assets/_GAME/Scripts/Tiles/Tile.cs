using System;
using Scripts.Event;
using Scripts.Event.Events;
using UnityEngine;

namespace Scripts.Tiles
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject particleEffectPrefab; // Prefab that contains TileParticleController
        
        public Vector2Int GridPosition { get; set; }
        private TileData _tileData;
        private int _thresholdA;
        private int _thresholdB;
        private int _thresholdC;

        private void OnEnable()
        {
            EventBus<FinishGameEvent>.AddListener(ChangeTileStatus);
        }
        
        private void OnDisable()
        {
            EventBus<FinishGameEvent>.RemoveListener(ChangeTileStatus);
        }

        private void ChangeTileStatus(object sender, FinishGameEvent @event)
        {
            boxCollider.enabled = false;
        }

        private void OnMouseDown()
        {
            EventBus<TileClickedEvent>.Emit(this, new TileClickedEvent { ClickedTile = this });
        }

        public void Initialize(TileData data, int tA, int tB, int tC)
        {
            _tileData = data;
            spriteRenderer.sprite = data.defaultIcon;
            _thresholdA = tA;
            _thresholdB = tB;
            _thresholdC = tC;
        }

        public bool IsSameColor(Tile otherTile)
        {
            return otherTile != null && _tileData == otherTile._tileData;
        }

        public void ClearTile()
        {
            Destroy(gameObject);
        }

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
    }
}
