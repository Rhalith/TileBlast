using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Level
{
    public class LevelChanger : MonoBehaviour
    {
        [SerializeField] private List<Button> levelButtons; // List of level buttons
        [SerializeField] private List<LevelData> levelDataList; // Corresponding LevelData assets for each level
        [SerializeField] private string gameSceneName = "GameScene"; // The scene where the game will load

        private void Start()
        {
            int unlockedLevel = PlayerPrefs.GetInt("Level", 1); // Default to 1 if no value is set

            for (int i = 0; i < levelButtons.Count; i++)
            {
                if (i < unlockedLevel)
                {
                    levelButtons[i].interactable = true;
                    int levelIndex = i;
                    levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
                }
                else
                {
                    levelButtons[i].interactable = false;
                }
            }
        }

        private void LoadLevel(int levelIndex)
        {
            if (levelIndex < levelDataList.Count)
            {
                PlayerPrefs.SetInt("SelectedLevel", levelIndex);
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError("Invalid level index!");
            }
        }
    }
}