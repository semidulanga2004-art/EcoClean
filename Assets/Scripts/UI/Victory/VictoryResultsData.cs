using System;

namespace EcoClean.Victory
{
    /// <summary>
    /// Plain data holder for the results screen.
    /// percentage is 0-100 (e.g. 90 means 90%).
    /// </summary>
    [Serializable]
    public struct VictoryResultsData
    {
        public int finalScore;
        public int cleanedCount;
        public int totalCount;
        public float percentage;

        public VictoryResultsData(int finalScore, int cleanedCount, int totalCount, float percentage)
        {
            this.finalScore = finalScore;
            this.cleanedCount = cleanedCount;
            this.totalCount = totalCount;
            this.percentage = percentage;
        }
    }

    /// <summary>
    /// Optional hand-off between scenes. The project lead calls Set(...) in the
    /// Gameplay scene, then loads the Victory scene. VictoryResultsDisplay reads
    /// the values automatically when the Victory scene starts.
    /// </summary>
    public static class VictoryResultsHandoff
    {
        public static bool HasPending { get; private set; }
        public static VictoryResultsData Pending { get; private set; }

        public static void Set(int finalScore, int cleanedCount, int totalCount, float percentage)
        {
            Pending = new VictoryResultsData(finalScore, cleanedCount, totalCount, percentage);
            HasPending = true;
        }

        public static void Clear() { HasPending = false; }
    }
}
