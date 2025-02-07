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
    /// <summary>
    /// Manages the game grid, including tile creation, interaction, collapse, fill, and board shuffling.
    /// Responsible for initializing the grid based on level data, handling tile clicks, updating game states,
    /// and ensuring that the board does not become deadlocked.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Tile Settings")]
        [Tooltip("Prefab for the tile objects.")]
        [SerializeField] private GameObject tilePrefab;

        [Tooltip("List of tile data used for initializing tiles.")]
        [SerializeField] private List<TileData> tileDataList;

        [Tooltip("Duration for tile falling animations.")]
        [SerializeField] private float fallDuration = 0.5f;

        #endregion

        #region Private Fields

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

        #endregion

        #region Unity Methods

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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the game finish event. If the player wins, the current level is saved and the level selection scene is loaded after a delay.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The finish game event data.</param>
        private void OnGameFinished(object sender, FinishGameEvent e)
        {
            _isGameFinished = true;
            if (e.IsWin)
            {
                // Save the current level if the player wins.
                if (PlayerPrefs.GetInt("Level") < _levelIndex)
                {
                    PlayerPrefs.SetInt("Level", _levelIndex);
                }
                // Load the level selection scene after 3 seconds.
                Invoke(nameof(LoadLevelSelection), 3f);
            }
        }

        /// <summary>
        /// Loads the level selection scene.
        /// </summary>
        private void LoadLevelSelection()
        {
            SceneManager.LoadScene("LevelSelection");
        }

        #endregion

        #region Grid Initialization

        /// <summary>
        /// Initializes the grid using the specified level data.
        /// Sets up game parameters, updates UI elements, calculates tile size, and creates the grid.
        /// </summary>
        /// <param name="levelData">Data for the current level.</param>
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

            // Update UI elements through events.
            EventBus<ChangeMovementTextEvent>.Emit(this,
                new ChangeMovementTextEvent { MovementCount = _allowedMoves, IsInitial = true });
            EventBus<ChangeScoreTextEvent>.Emit(this,
                new ChangeScoreTextEvent { ScoreChange = _targetScore, IsInitial = true });

            // Limit the tile data list to the number of colors specified by the level.
            if (tileDataList.Count > levelData.NumColors)
            {
                tileDataList = tileDataList.GetRange(0, levelData.NumColors);
            }

            CalculateTileSize();
            CreateGrid();
        }

        /// <summary>
        /// Calculates the size of each tile based on the screen dimensions and grid configuration.
        /// </summary>
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

            // Apply a spacing factor to ensure some padding between tiles.
            float spacingFactor = 0.95f;
            float tileWidth = (cameraWidth / _columns) * spacingFactor;
            float tileHeight = (cameraHeight / _rows) * spacingFactor;
            float tileSize = Mathf.Min(tileWidth, tileHeight);
            _tileSize = new Vector2(tileSize, tileSize);
        }

        /// <summary>
        /// Creates the grid by instantiating tiles and assigning them to their correct positions.
        /// </summary>
        private void CreateGrid()
        {
            _grid = new Tile[_rows, _columns];

            float gridWidth = _tileSize.x * _columns;
            float gridHeight = _tileSize.y * _rows;

            _gridStartPosition = new Vector2(
                transform.position.x - gridWidth / 2 + _tileSize.x / 2,
                transform.position.y + gridHeight / 2 - _tileSize.y / 2
            );

            // Spawn each tile in the grid.
            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    SpawnTile(row, column);
                }
            }

            // Assign icons to tile groups.
            AssignGroupIcons();

            // Check for deadlock; if found, shuffle the board.
            if (IsDeadlocked())
            {
                ShuffleBoard();
            }
        }

        #endregion

        #region Tile Spawning

        /// <summary>
        /// Calculates and returns the world position for a tile given its grid coordinates.
        /// </summary>
        /// <param name="row">Row index of the tile.</param>
        /// <param name="column">Column index of the tile.</param>
        /// <returns>World position of the tile.</returns>
        private Vector2 GetWorldPosition(int row, int column)
        {
            float x = _gridStartPosition.x + column * _tileSize.x;
            float y = _gridStartPosition.y - row * _tileSize.y;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Instantiates and initializes a tile at the specified grid position.
        /// </summary>
        /// <param name="row">Row index for the tile.</param>
        /// <param name="column">Column index for the tile.</param>
        private void SpawnTile(int row, int column)
        {
            Vector2 tilePosition = GetWorldPosition(row, column);
            GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);

            // Adjust scale based on the prefab's sprite size.
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

            // Set the sprite's sorting order to ensure correct layering.
            SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _rows - row;
            }

            _grid[row, column] = tile;
        }

        #endregion

        #region Tile Interaction

        /// <summary>
        /// Handles tile click events. Processes groups of connected tiles, updates score and moves,
        /// collapses and fills the grid, and shuffles the board if necessary.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Tile clicked event data.</param>
        private void OnTileClicked(object sender, TileClickedEvent e)
        {
            if (_isGameFinished) return;
            if (_allowedMoves <= 0)
            {
                EventBus<FinishGameEvent>.Emit(this, new FinishGameEvent { IsWin = false });
                return;
            }

            List<Tile> group = GetConnectedTiles(e.ClickedTile);

            // Only clear groups of two or more connected tiles.
            if (group.Count >= 2)
            {
                e.ClickedTile.SpawnParticleEffect();
                EventBus<PlaySoundEvent>.Emit(this,
                    group.Count > 4
                        ? new PlaySoundEvent { SoundType = SoundType.MultiClick }
                        : new PlaySoundEvent { SoundType = SoundType.Click });

                // Clear all tiles in the connected group.
                foreach (Tile tile in group)
                {
                    _grid[tile.GridPosition.x, tile.GridPosition.y] = null;
                    tile.ClearTile();
                }

                // Update score and movement count.
                EventBus<ChangeScoreTextEvent>.Emit(this,
                    new ChangeScoreTextEvent { ScoreChange = group.Count * 100 });
                _allowedMoves--;
                EventBus<ChangeMovementTextEvent>.Emit(this,
                    new ChangeMovementTextEvent { MovementCount = _allowedMoves });

                // If no moves remain, end the game.
                if (_allowedMoves <= 0 && !_isGameFinished)
                {
                    EventBus<FinishGameEvent>.Emit(this, new FinishGameEvent { IsWin = false });
                    EventBus<PlaySoundEvent>.Emit(this, new PlaySoundEvent { SoundType = SoundType.Lose });
                    Invoke(nameof(LoadLevelSelection), 3f);
                }

                CollapseGrid();
                FillGrid();

                if (IsDeadlocked())
                {
                    ShuffleBoard();
                }
            }
        }

        #endregion

        #region Grid Manipulation

        /// <summary>
        /// Checks whether the grid is deadlocked (i.e., no available moves exist).
        /// </summary>
        /// <returns>True if deadlocked; otherwise, false.</returns>
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

        /// <summary>
        /// Shuffles the board by randomly reassigning tile positions.
        /// </summary>
        private void ShuffleBoard()
        {
            List<Tile> allTiles = new List<Tile>();

            // Collect all non-null tiles.
            foreach (var tile in _grid)
            {
                if (tile != null)
                {
                    allTiles.Add(tile);
                }
            }

            // Shuffle the list (assuming an extension method exists in Scripts.Utilities).
            allTiles.Shuffle();

            int index = 0;
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    _grid[row, col] = allTiles[index];
                    _grid[row, col].GridPosition = new Vector2Int(row, col);
                    _grid[row, col].name = $"{row} {col}";
                    _grid[row, col].GetComponent<SpriteRenderer>().sortingOrder = _rows - row;
                    _grid[row, col].transform.DOMove(GetWorldPosition(row, col), fallDuration);
                    index++;
                }
            }

            AssignGroupIcons();
        }

        /// <summary>
        /// Assigns group icons to each connected group of tiles.
        /// </summary>
        private void AssignGroupIcons()
        {
            HashSet<Tile> processedTiles = new HashSet<Tile>();

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    Tile currentTile = _grid[row, column];
                    if (currentTile == null || processedTiles.Contains(currentTile))
                        continue;

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

        /// <summary>
        /// Collapses the grid by moving tiles downward to fill empty spaces.
        /// </summary>
        private void CollapseGrid()
        {
            for (int column = 0; column < _columns; column++)
            {
                int emptyCount = 0;

                // Traverse from bottom to top in each column.
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

        /// <summary>
        /// Fills empty spaces in the grid by instantiating new tiles.
        /// </summary>
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

                        // Update group icons after filling a new tile.
                        AssignGroupIcons();
                    }
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Returns a list of all tiles connected to the start tile that share the same color.
        /// </summary>
        /// <param name="startTile">The tile from which to begin the search.</param>
        /// <returns>A list of connected tiles of the same color.</returns>
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

        /// <summary>
        /// Gets the adjacent neighboring tiles (up, down, left, right) for the given tile.
        /// </summary>
        /// <param name="tile">The reference tile.</param>
        /// <returns>An enumerable of neighboring tiles.</returns>
        private IEnumerable<Tile> GetNeighbors(Tile tile)
        {
            Vector2Int tilePos = tile.GridPosition;

            if (tilePos.x > 0) yield return _grid[tilePos.x - 1, tilePos.y];
            if (tilePos.x < _rows - 1) yield return _grid[tilePos.x + 1, tilePos.y];
            if (tilePos.y > 0) yield return _grid[tilePos.x, tilePos.y - 1];
            if (tilePos.y < _columns - 1) yield return _grid[tilePos.x, tilePos.y + 1];
        }

        #endregion
    }
}