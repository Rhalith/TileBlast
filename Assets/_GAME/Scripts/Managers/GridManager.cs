using System.Collections.Generic;
using DG.Tweening;
using Scripts.Event;
using Scripts.Event.Events;
using Scripts.Level;
using Scripts.Tiles;
using Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Managers
{

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private List<TileData> tileDataList;
        [SerializeField] private float fallDuration = 0.5f;

        private int _rows;
        private int _columns;
        private int _thresholdA;
        private int _thresholdB;
        private int _thresholdC;
        private int _allowedMoves;
        private Tile[,] _grid;
        private Vector2 _tileSize;
        private Vector2 _gridStartPosition;

        private void OnEnable()
        {
            EventBus<TileClickedEvent>.AddListener(OnTileClicked);
        }

        private void OnDisable()
        {
            EventBus<TileClickedEvent>.RemoveListener(OnTileClicked);
        }

        public void InitializeGrid(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("GridManager: No Level Data provided!");
                return;
            }

            _rows = levelData.Rows;
            _columns = levelData.Columns;
            _thresholdA = levelData.GroupThresholdA;
            _thresholdB = levelData.GroupThresholdB;
            _thresholdC = levelData.GroupThresholdC;
            _allowedMoves = levelData.AllowedMoves; // Assign allowed moves

            // Ensure TileData count matches numColors
            if (tileDataList.Count > levelData.NumColors)
            {
                tileDataList = tileDataList.GetRange(0, levelData.NumColors);
            }

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

            SpriteRenderer spriteRenderer = tilePrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("Tile Prefab does not have a SpriteRenderer component.");
                return;
            }

            _tileSize = spriteRenderer.bounds.size;
        }

        private void CreateGrid()
        {
            _grid = new Tile[_rows, _columns];

            _gridStartPosition = new Vector2(
                transform.position.x - (_columns * _tileSize.x) / 2f + _tileSize.x / 2f,
                transform.position.y + (_rows * _tileSize.y) / 2f - _tileSize.y / 2f
            );

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    SpawnTile(row, column);
                }
            }

            AssignGroupIcons();

            if (IsDeadlocked())
            {
                ShuffleBoard();
            }
        }

        private Vector2 GetWorldPosition(int row, int column)
        {
            float x = _gridStartPosition.x + column * _tileSize.x;
            float y = _gridStartPosition.y - row * _tileSize.y;
            return new Vector2(x, y);
        }

        private void SpawnTile(int row, int column)
        {
            Vector2 tilePosition = GetWorldPosition(row, column);
            GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);

            tileObject.name = $"{row} {column}";

            Tile tile = tileObject.GetComponent<Tile>();
            int colorIndex = Random.Range(0, tileDataList.Count);
            tile.Initialize(tileDataList[colorIndex], _thresholdA, _thresholdB, _thresholdC);
            tile.GridPosition = new Vector2Int(row, column);

            _grid[row, column] = tile;
        }

        private void OnTileClicked(object sender, TileClickedEvent @event)
        {
            if (_allowedMoves <= 0)
            {
                Debug.Log("No more moves left!");
                return;
            }

            List<Tile> group = GetConnectedTiles(@event.ClickedTile);

            if (group.Count >= 2)
            {
                foreach (Tile tile in group)
                {
                    _grid[tile.GridPosition.x, tile.GridPosition.y] = null;
                    tile.ClearTile();
                }

                _allowedMoves--; // Decrease move count on valid tile match
                Debug.Log($"Moves Left: {_allowedMoves}");

                CollapseGrid();
                FillGrid();

                if (IsDeadlocked())
                {
                    ShuffleBoard();
                }
            }
        }

        private bool IsDeadlocked()
        {
            foreach (var tile in _grid)
            {
                if (tile != null && GetConnectedTiles(tile).Count >= 2)
                {
                    return false;
                }
            }

            return true;
        }

        private void ShuffleBoard()
        {
            List<Tile> allTiles = new List<Tile>();

            foreach (var tile in _grid)
            {
                if (tile != null)
                {
                    allTiles.Add(tile);
                }
            }

            allTiles.Shuffle();

            int index = 0;
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    _grid[row, col] = allTiles[index];
                    _grid[row, col].GridPosition = new Vector2Int(row, col);
                    _grid[row, col].transform.DOMove(GetWorldPosition(row, col), fallDuration);
                    index++;
                }
            }

            AssignGroupIcons();
        }

        private void AssignGroupIcons()
        {
            HashSet<Tile> processedTiles = new HashSet<Tile>();

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    Tile currentTile = _grid[row, column];
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

            if (tilePos.x > 0) yield return _grid[tilePos.x - 1, tilePos.y];
            if (tilePos.x < _rows - 1) yield return _grid[tilePos.x + 1, tilePos.y];
            if (tilePos.y > 0) yield return _grid[tilePos.x, tilePos.y - 1];
            if (tilePos.y < _columns - 1) yield return _grid[tilePos.x, tilePos.y + 1];
        }

        private void CollapseGrid()
        {
            for (int column = 0; column < _columns; column++)
            {
                int emptyCount = 0;

                for (int row = _rows - 1; row >= 0; row--)
                {
                    if (_grid[row, column] == null)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        Tile tile = _grid[row, column];
                        _grid[row, column] = null;
                        _grid[row + emptyCount, column] = tile;

                        tile.GridPosition = new Vector2Int(row + emptyCount, column);
                        tile.transform.DOMove(GetWorldPosition(row + emptyCount, column), fallDuration);
                    }
                }
            }
        }

        private void FillGrid()
        {
            for (int column = 0; column < _columns; column++)
            {
                for (int row = 0; row < _rows; row++)
                {
                    if (_grid[row, column] == null)
                    {
                        Vector2 spawnPosition = GetWorldPosition(-1, column);
                        GameObject tileObject = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);

                        Tile tile = tileObject.GetComponent<Tile>();
                        int colorIndex = Random.Range(0, tileDataList.Count);
                        tile.Initialize(tileDataList[colorIndex], _thresholdA, _thresholdB, _thresholdC);
                        tile.GridPosition = new Vector2Int(row, column);

                        _grid[row, column] = tile;
                        tile.transform.DOMove(GetWorldPosition(row, column), fallDuration);
                    }
                }
            }

            AssignGroupIcons();
        }
    }

}