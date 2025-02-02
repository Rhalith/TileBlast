using UnityEngine;

namespace Scripts.Level
{

    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private int rows; // Grid rows (M)
        [SerializeField] private int columns; // Grid columns (N)
        [SerializeField] private int numColors; // Number of colors (K)
        [SerializeField] private int groupThresholdA; // Threshold for first icon (A)
        [SerializeField] private int groupThresholdB; // Threshold for second icon (B)
        [SerializeField] private int groupThresholdC; // Threshold for third icon (C)

        public int Rows => rows;
        public int Columns => columns;
        public int NumColors => numColors;
        public int GroupThresholdA => groupThresholdA;
        public int GroupThresholdB => groupThresholdB;
        public int GroupThresholdC => groupThresholdC;
    }

}