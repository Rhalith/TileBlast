using System.Collections.Generic;
using Scripts.Tiles;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int rows = 10;
        [SerializeField] private int columns = 12;
        [SerializeField] private GameObject tilePrefab; // Assign a tile prefab in Unity Inspector
        [SerializeField] private Sprite[] colorIcons; // Assign color sprites in Unity Inspector
        [SerializeField] private int thresholdA = 4;
        [SerializeField] private int thresholdB = 7;
        [SerializeField] private int thresholdC = 10;

        private Tile[,] grid;
        private Vector2 tileSize; // Size of the tile prefab
        private Vector2 gridStartPosition; // Top-left position of the grid

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

            // Get the size of the tile prefab, accounting for scaling
            SpriteRenderer renderer = tilePrefab.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("Tile Prefab does not have a SpriteRenderer component.");
                return;
            }

            tileSize = renderer.bounds.size; // Get the bounds size of the tile sprite
        }

        private void CreateGrid()
        {
            grid = new Tile[rows, columns];

            // Calculate the starting position to center the grid
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

                    // Rename the tile GameObject to match its grid position
                    tileObject.name = $"{row} {column}";

                    Tile tile = tileObject.GetComponent<Tile>();
                    tile.Initialize(Random.Range(0, colorIcons.Length), colorIcons);
                    tile.GridPosition = new Vector2Int(row, column); // Ensure the tile knows its grid position

                    // Adjust the sorting layer based on the row
                    SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = rows - row; // Higher rows get lower sorting orders
                    }

                    grid[row, column] = tile;
                }
            }
        }

        private Vector2 GetWorldPosition(int row, int column)
        {
            // Dynamically calculate spacing using tile bounds
            float x = gridStartPosition.x + column * tileSize.x * 2.25f;
            float y = gridStartPosition.y - row * tileSize.y * 2.25f;
            return new Vector2(x, y);
        }

        public void OnTileClicked(Tile clickedTile)
        {
            List<Tile> group = GetConnectedTiles(clickedTile);

            if (group.Count >= 2) // Minimum collapsible group size
            {
                foreach (Tile tile in group)
                {
                    tile.ClearTile();
                }

                CollapseGrid();
                FillGrid();
            }
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

            // Check all four directions
            if (tilePos.x > 0) yield return grid[tilePos.x - 1, tilePos.y]; // Up
            if (tilePos.x < rows - 1) yield return grid[tilePos.x + 1, tilePos.y]; // Down
            if (tilePos.y > 0) yield return grid[tilePos.x, tilePos.y - 1]; // Left
            if (tilePos.y < columns - 1) yield return grid[tilePos.x, tilePos.y + 1]; // Right
        }

        private void CollapseGrid()
        {
            for (int column = 0; column < columns; column++)
            {
                int emptyCount = 0;

                for (int row = rows - 1; row >= 0; row--)
                {
                    if (grid[row, column] == null)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        Tile tile = grid[row, column];
                        grid[row, column] = null;
                        grid[row + emptyCount, column] = tile;

                        tile.MoveTo(GetWorldPosition(row + emptyCount, column));
                    }
                }
            }
        }

        private void FillGrid()
        {
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (grid[row, column] == null)
                    {
                        Vector2 tilePosition = GetWorldPosition(row, column);
                        GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);

                        // Rename the new tile GameObject
                        tileObject.name = $"{row} {column}";

                        Tile tile = tileObject.GetComponent<Tile>();
                        tile.Initialize(Random.Range(0, colorIcons.Length), colorIcons);
                        tile.GridPosition = new Vector2Int(row, column);

                        // Adjust the sorting layer based on the row
                        SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.sortingOrder = rows - row; // Higher rows get lower sorting orders
                        }

                        grid[row, column] = tile;
                    }
                }
            }
        }
    }
}
