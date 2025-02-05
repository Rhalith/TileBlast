using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Level
{
    public class LevelChanger : MonoBehaviour
    {
        [SerializeField] private List<Button> levelButtons;      // List of level buttons
        [SerializeField] private List<LevelData> levelDataList;    // Corresponding LevelData assets for each level
        [SerializeField] private string gameSceneName = "GameScene"; // The scene where the game will load

        private void Start()
        {
            // Validate serialized field references.
            if (levelButtons == null || levelDataList == null)
            {
                Debug.LogError("LevelChanger: levelButtons and/or levelDataList reference is missing.", this);
                return;
            }

            // Get the unlocked level from PlayerPrefs; defaults to 1 if not set.
            int unlockedLevel = PlayerPrefs.GetInt("Level", 1);

            // Configure each level button.
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                bool isUnlocked = i < unlockedLevel;
                button.interactable = isUnlocked;

                if (isUnlocked)
                {
                    // Clear any pre-existing listeners to avoid duplicates.
                    button.onClick.RemoveAllListeners();

                    // Capture the current index for the closure.
                    int levelIndex = i;
                    button.onClick.AddListener(() => LoadLevel(levelIndex));
                }
            }
        }

        private void LoadLevel(int levelIndex)
        {
            // Guard clause to ensure the level index is valid.
            if (levelIndex >= levelDataList.Count)
            {
                Debug.LogError($"LevelChanger: Invalid level index ({levelIndex}) requested.");
                return;
            }

            PlayerPrefs.SetInt("SelectedLevel", levelIndex);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
