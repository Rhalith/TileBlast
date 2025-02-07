using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Level
{
    /// <summary>
    /// Handles the level selection UI by enabling/disabling level buttons
    /// based on the player's progress and loading the selected level.
    /// </summary>
    public class LevelChanger : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Elements")]
        [Tooltip("List of buttons used for selecting levels.")]
        [SerializeField] private List<Button> levelButtons;

        [Header("Level Data")]
        [Tooltip("List containing the data for each level.")]
        [SerializeField] private List<LevelData> levelDataList;

        [Header("Scene Settings")]
        [Tooltip("Name of the scene to load when a level is selected.")]
        [SerializeField] private string gameSceneName = "GameScene";

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes level buttons based on the player's unlocked progress.
        /// </summary>
        private void Start()
        {
            // Ensure all required references are set
            if (levelButtons == null || levelDataList == null)
            {
                Debug.LogError("LevelChanger: One or more required references (levelButtons, levelDataList) are missing.", this);
                return;
            }

            // Retrieve the highest unlocked level from PlayerPrefs.
            // The stored value represents the index of the last unlocked level.
            int unlockedLevel = PlayerPrefs.GetInt("Level", 0);

            // Setup each button's interactability and click event based on unlock status.
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];

                // A level is unlocked if its index is less than or equal to the unlocked level index.
                bool isUnlocked = i <= unlockedLevel;
                button.interactable = isUnlocked;

                if (isUnlocked)
                {
                    // Clear any previous listeners to prevent multiple subscriptions.
                    button.onClick.RemoveAllListeners();

                    // Capture the current index for use in the lambda expression.
                    int levelIndex = i;
                    button.onClick.AddListener(() => LoadLevel(levelIndex));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the game scene and sets the selected level.
        /// </summary>
        /// <param name="levelIndex">Index of the level to load.</param>
        private void LoadLevel(int levelIndex)
        {
            // Verify that the level index is valid within the levelDataList.
            if (levelIndex >= levelDataList.Count)
            {
                Debug.LogError($"LevelChanger: Invalid level index ({levelIndex}) requested.");
                return;
            }

            // Save the selected level index so that it can be accessed in the game scene.
            PlayerPrefs.SetInt("SelectedLevel", levelIndex);

            // Load the designated game scene.
            SceneManager.LoadScene(gameSceneName);
        }

        #endregion
    }
}
