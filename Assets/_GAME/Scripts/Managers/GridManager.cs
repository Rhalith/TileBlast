using System;
using System.Collections.Generic;
using Scripts.Event;
using Scripts.Event.Events;
using Scripts.Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int rows = 10;
        [SerializeField] private int columns = 12;
        [SerializeField] private GameObject tilePrefab;

        [SerializeField] private List<TileData> tileDataList; // List of TileData assets, one for each color
        [SerializeField] private int thresholdA = 4;
        [SerializeField] private int thresholdB = 7;
        [SerializeField] private int thresholdC = 10;

        private Tile[,] grid;
        private Vector2 tileSize;
        private Vector2 gridStartPosition;

        private void OnEnable()
        {
            EventBus<TileClickedEvent>.AddListener(OnTileClicked);
        }
        
        private void OnDisable()
        {
            EventBus<TileClickedEvent>.RemoveListener(OnTileClicked);
        }

        private void OnTileClicked(object sender, TileClickedEvent @event)
        {
            List<Tile> group = GetConnectedTiles(@event.ClickedTile);

            if (group.Count >= 2)
            {
                foreach (Tile tile in group)
                {
                    tile.ClearTile();
                }
            }
        }

        private void Start()
        {
            CalculateTileSize();
            CreateGrid();
        }

        private void CalculateTileSize()
        {
            if (tilePrefab == null)
            {
                Debug.LogError("Tile Prefab is not assigned.");
                return;
            }

            SpriteRenderer renderer = tilePrefab.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("Tile Prefab does not have a SpriteRenderer component.");
                return;
            }

            tileSize = renderer.bounds.size;
        }

        private void CreateGrid()
        {
            grid = new Tile[rows, columns];

            gridStartPosition = new Vector2(
                transform.position.x - (columns * tileSize.x) / 2f + tileSize.x / 2f,
                transform.position.y + (rows * tileSize.y) / 2f - tileSize.y / 2f
            );

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector2 tilePosition = GetWorldPosition(row, column);
                    GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);

                    tileObject.name = $"{row} {column}";

                    Tile tile = tileObject.GetComponent<Tile>();
                    int colorIndex = Random.Range(0, tileDataList.Count);
                    tile.Initialize(tileDataList[colorIndex], thresholdA, thresholdB, thresholdC);
                    tile.GridPosition = new Vector2Int(row, column);

                    SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = rows - row;
                    }

                    grid[row, column] = tile;
                }
            }

            // Assign group-based icons after creating the grid
            AssignGroupIcons();
        }

        private Vector2 GetWorldPosition(int row, int column)
        {
            float x = gridStartPosition.x + column * tileSize.x;
            float y = gridStartPosition.y - row * tileSize.y;
            return new Vector2(x, y);
        }

        private List<Tile> GetConnectedTiles(Tile startTile)
        {
            List<Tile> connectedTiles = new List<Tile>();
            Queue<Tile> tilesToCheck = new Queue<Tile>();
            tilesToCheck.Enqueue(startTile);

            while (tilesToCheck.Count > 0)
            {
                Tile currentTile = tilesToCheck.Dequeue();

                if (currentTile == null || connectedTiles.Contains(currentTile) || !currentTile.IsSameColor(startTile))
                    continue;

                connectedTiles.Add(currentTile);

                foreach (Tile neighbor in GetNeighbors(currentTile))
                {
                    tilesToCheck.Enqueue(neighbor);
                }
            }

            return connectedTiles;
        }

        private IEnumerable<Tile> GetNeighbors(Tile tile)
        {
            Vector2Int tilePos = tile.GridPosition;

            if (tilePos.x > 0) yield return grid[tilePos.x - 1, tilePos.y];
            if (tilePos.x < rows - 1) yield return grid[tilePos.x + 1, tilePos.y];
            if (tilePos.y > 0) yield return grid[tilePos.x, tilePos.y - 1];
            if (tilePos.y < columns - 1) yield return grid[tilePos.x, tilePos.y + 1];
        }

        private void AssignGroupIcons()
        {
            HashSet<Tile> processedTiles = new HashSet<Tile>();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Tile currentTile = grid[row, column];
                    if (currentTile == null || processedTiles.Contains(currentTile)) continue;

                    List<Tile> group = GetConnectedTiles(currentTile);
                    int groupSize = group.Count;

                    foreach (Tile tile in group)
                    {
                        tile.AssignIcon(groupSize);
                        processedTiles.Add(tile);
                    }
                }
            }
        }
    }
}
