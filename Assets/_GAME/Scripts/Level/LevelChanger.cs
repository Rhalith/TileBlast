using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Level
{
    public class LevelChanger : MonoBehaviour
    {
        [SerializeField] private List<Button> levelButtons; 
        [SerializeField] private List<LevelData> levelDataList;
        [SerializeField] private string gameSceneName = "GameScene";

        private void Start()
        {
            if (levelButtons == null || levelDataList == null)
            {
                Debug.LogError("LevelChanger: levelButtons and/or levelDataList reference is missing.", this);
                return;
            }
            
            int unlockedLevel = PlayerPrefs.GetInt("Level", 1);

            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                bool isUnlocked = i < unlockedLevel;
                button.interactable = isUnlocked;

                if (isUnlocked)
                {
                    button.onClick.RemoveAllListeners();

                    int levelIndex = i;
                    button.onClick.AddListener(() => LoadLevel(levelIndex));
                }
            }
        }

        private void LoadLevel(int levelIndex)
        {
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
