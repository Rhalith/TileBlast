using UnityEngine;

namespace Scripts.Level
{
    /// <summary>
    /// Contains configuration data for a level.
    /// This data is used to define level-specific parameters such as target score, allowed moves,
    /// grid dimensions, and group thresholds.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        #region Level Configuration

        [Header("Level Configuration")]
        [Tooltip("Unique level number for identification purposes.")]
        [SerializeField] private int levelNumber;

        [Tooltip("Target score required to complete the level.")]
        [SerializeField] private float targetScore;

        [Tooltip("Number of moves allowed in the level.")]
        [SerializeField] private int allowedMoves;

        #endregion

        #region Grid Settings

        [Header("Grid Settings")]
        [Tooltip("Number of rows in the level grid.")]
        [SerializeField] private int rows;

        [Tooltip("Number of columns in the level grid.")]
        [SerializeField] private int columns;

        [Tooltip("Number of distinct colors used in the level.")]
        [SerializeField] private int numColors;

        #endregion

        #region Group Thresholds

        [Header("Group Thresholds")]
        [Tooltip("Threshold value for the first icon/group (A).")]
        [SerializeField] private int groupThresholdA;

        [Tooltip("Threshold value for the second icon/group (B).")]
        [SerializeField] private int groupThresholdB;

        [Tooltip("Threshold value for the third icon/group (C).")]
        [SerializeField] private int groupThresholdC;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the unique level number.
        /// </summary>
        public int LevelNumber => levelNumber;

        /// <summary>
        /// Gets the target score for the level.
        /// </summary>
        public float TargetScore => targetScore;

        /// <summary>
        /// Gets the number of moves allowed.
        /// </summary>
        public int AllowedMoves => allowedMoves;

        /// <summary>
        /// Gets the number of rows in the level grid.
        /// </summary>
        public int Rows => rows;

        /// <summary>
        /// Gets the number of columns in the level grid.
        /// </summary>
        public int Columns => columns;

        /// <summary>
        /// Gets the number of distinct colors used.
        /// </summary>
        public int NumColors => numColors;

        /// <summary>
        /// Gets the threshold value for the first icon/group (A).
        /// </summary>
        public int GroupThresholdA => groupThresholdA;

        /// <summary>
        /// Gets the threshold value for the second icon/group (B).
        /// </summary>
        public int GroupThresholdB => groupThresholdB;

        /// <summary>
        /// Gets the threshold value for the third icon/group (C).
        /// </summary>
        public int GroupThresholdC => groupThresholdC;

        #endregion
    }
}
