using System.Collections.Generic;
using DG.Tweening;
using Scripts.Event;
using Scripts.Event.Events;
using Scripts.Level;
using Scripts.Tiles;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Scripts.Managers
{

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private List<TileData> tileDataList;
        [SerializeField] private float fallDuration = 0.5f;

        private int _levelIndex;
        private int _rows;
        private int _columns;
        private int _thresholdA;
        private int _thresholdB;
        private int _thresholdC;
        private int _allowedMoves;
        private float _targetScore;
        private Tile[,] _grid;
        private Vector2 _tileSize;
        private Vector2 _gridStartPosition;
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private bool _isGameFinished;


        private void OnEnable()
        {
            EventBus<FinishGameEvent>.AddListener(OnGameFinished);
            EventBus<TileClickedEvent>.AddListener(OnTileClicked);
        }

        private void OnDisable()
        {
            EventBus<FinishGameEvent>.RemoveListener(OnGameFinished);
            EventBus<TileClickedEvent>.RemoveListener(OnTileClicked);
        }

        private void OnGameFinished(object sender, FinishGameEvent @event)
        {
            _isGameFinished = true;
            if (@event.IsWin)
            {
                PlayerPrefs.SetInt("Level", _levelIndex);
                Invoke(nameof(LoadLevelSelection), 3f);
            }
        }

        private void LoadLevelSelection()
        {
            SceneManager.LoadScene("LevelSelection");
        }

        public void InitializeGrid(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("GridManager: No Level Data provided!");
                return;
            }

            _isGameFinished = false;
            _levelIndex = levelData.LevelNumber;
            _rows = levelData.Rows;
            _columns = levelData.Columns;
            _thresholdA = levelData.GroupThresholdA;
            _thresholdB = levelData.GroupThresholdB;
            _thresholdC = levelData.GroupThresholdC;
            _allowedMoves = levelData.AllowedMoves;
            _targetScore = levelData.TargetScore;

            EventBus<ChangeMovementTextEvent>.Emit(this,
                new ChangeMovementTextEvent { MovementCount = _allowedMoves, IsInitial = true });

            EventBus<ChangeScoreTextEvent>.Emit(this,
                new ChangeScoreTextEvent { ScoreChange = _targetScore, IsInitial = true });

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

            float screenRatio = (float)Screen.width / Screen.height;
            float cameraHeight = Camera.main.orthographicSize * 2;
            float cameraWidth = cameraHeight * screenRatio;

            float spacingFactor = 0.95f;

            float tileWidth = (cameraWidth / _columns) * spacingFactor;
            float tileHeight = (cameraHeight / _rows) * spacingFactor;

            float tileSize = Mathf.Min(tileWidth, tileHeight);
            _tileSize = new Vector2(tileSize, tileSize);
        }


        private void CreateGrid()
        {
            _grid = new Tile[_rows, _columns];

            float gridWidth = _tileSize.x * _columns;
            float gridHeight = _tileSize.y * _rows;

            _gridStartPosition = new Vector2(
                transform.position.x - gridWidth / 2 + _tileSize.x / 2,
                transform.position.y + gridHeight / 2 - _tileSize.y / 2
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

            float prefabSizeX = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
            float prefabSizeY = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;

            float scaleFactor = 1.15f;

            tileObject.transform.localScale = new Vector3(
                (_tileSize.x / prefabSizeX) * scaleFactor,
                (_tileSize.y / prefabSizeY) * scaleFactor,
                1f
            );

            tileObject.name = $"{row} {column}";

            Tile tile = tileObject.GetComponent<Tile>();
            int colorIndex = Random.Range(0, tileDataList.Count);
            tile.Initialize(tileDataList[colorIndex], _thresholdA, _thresholdB, _thresholdC);
            tile.GridPosition = new Vector2Int(row, column);

            SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _rows - row;
            }

            _grid[row, column] = tile;
        }


        private void OnTileClicked(object sender, TileClickedEvent @event)
        {
            if (_isGameFinished) return;
            if (_allowedMoves <= 0)
            {
                EventBus<FinishGameEvent>.Emit(this, new FinishGameEvent { IsWin = false });
                return;
            }

            List<Tile> group = GetConnectedTiles(@event.ClickedTile);

            if (group.Count >= 2)
            {
                @event.ClickedTile.SpawnParticleEffect();
                EventBus<PlaySoundEvent>.Emit(this,
                    group.Count > 4
                        ? new PlaySoundEvent { SoundType = SoundType.MultiClick }
                        : new PlaySoundEvent { SoundType = SoundType.Click });
                foreach (Tile tile in group)
                {
                    _grid[tile.GridPosition.x, tile.GridPosition.y] = null;
                    tile.ClearTile();
                }

                EventBus<ChangeScoreTextEvent>.Emit(this,
                    new ChangeScoreTextEvent { ScoreChange = group.Count * 100 });

                _allowedMoves--;
                EventBus<ChangeMovementTextEvent>.Emit(this,
                    new ChangeMovementTextEvent { MovementCount = _allowedMoves });
                if (_allowedMoves <= 0 && !_isGameFinished)
                {
                    EventBus<FinishGameEvent>.Emit(this, new FinishGameEvent { IsWin = false });
                }

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
                        tile.GetComponent<SpriteRenderer>().sortingOrder = _rows - tile.GridPosition.x;
                        tile.name = $"{tile.GridPosition.x} {tile.GridPosition.y}";
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

                        float prefabSizeX = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
                        float prefabSizeY = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;
                        tileObject.transform.localScale = new Vector3(
                            (_tileSize.x / prefabSizeX) * 1.15f,
                            (_tileSize.y / prefabSizeY) * 1.15f,
                            1f
                        );

                        _grid[row, column] = tile;
                        tile.transform.DOMove(GetWorldPosition(row, column), fallDuration);
                        tile.GetComponent<SpriteRenderer>().sortingOrder = _rows - tile.GridPosition.x;
                        tile.name = $"{tile.GridPosition.x} {tile.GridPosition.y}";
                        SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sortingOrder = _rows - row;
                        }

                        AssignGroupIcons();
                    }
                }
            }
        }
    }

}