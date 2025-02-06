using System.Collections.Generic;
using System.Linq;
using Scripts.Level;
using UnityEngine;

namespace Scripts.Managers
{

    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private List<LevelData> levelDataList;
        
        private void Start()
        {

            int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 0);
            if (selectedLevel <= levelDataList.Count)
            {
                gridManager.InitializeGrid(levelDataList[selectedLevel]);
                Debug.Log($"Loaded Level {selectedLevel + 1}: {levelDataList[selectedLevel].Rows}x{levelDataList[selectedLevel].Columns} grid.");
            }
            else
            {
                Debug.LogError("Invalid level selection!");
            }
        }
    }

}