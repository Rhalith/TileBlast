using System.Collections.Generic;
using Scripts.Level;
using UnityEngine;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages level loading by initializing the game grid using the corresponding level data.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Managers")]
        [Tooltip("Reference to the GridManager responsible for setting up and managing the game grid.")]
        [SerializeField] private GridManager gridManager;
        
        [Header("Level Data")]
        [Tooltip("List of LevelData ScriptableObjects representing each level's configuration.")]
        [SerializeField] private List<LevelData> levelDataList;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Called on startup. Retrieves the selected level from PlayerPrefs and initializes the grid accordingly.
        /// </summary>
        private void Start()
        {
            int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 0);

            // Validate that the selected level index is within the bounds of levelDataList.
            if (selectedLevel < levelDataList.Count)
            {
                gridManager.InitializeGrid(levelDataList[selectedLevel]);
                Debug.Log($"Loaded Level {selectedLevel + 1}: {levelDataList[selectedLevel].Rows}x{levelDataList[selectedLevel].Columns} grid.");
            }
            else
            {
                Debug.LogError("Invalid level selection!");
            }
        }

        #endregion
    }
}