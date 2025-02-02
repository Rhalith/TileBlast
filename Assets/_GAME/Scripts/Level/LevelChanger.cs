using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Level
{
    public class LevelChanger : MonoBehaviour
    {
        [SerializeField] private int startLevel;
        [SerializeField] private List<Button> levelButtons;
        private void Start()
        {
            PlayerPrefs.SetInt("Level", startLevel);
            
            for (int i = 0; i < levelButtons.Count; i++)
            {
                if (i <= PlayerPrefs.GetInt("Level")-1)
                {
                    levelButtons[i].interactable = true;
                }
                else
                {
                    levelButtons[i].interactable = false;
                }
            }
        }

        public void ChangeLevelData(int i)
        {
            PlayerPrefs.SetInt("Level", i);
        }
    }

}