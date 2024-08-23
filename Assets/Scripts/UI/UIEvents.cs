using System;

namespace UI
{
    public static class UIEvents
    {
        public static event Action<int> OnHitCountChanged;
        public static event Action<int> OnScoreChanged;

        public static void HitCountChanged(int newHitCount)
        {
            OnHitCountChanged?.Invoke(newHitCount);
        }    
    
        public static void ScoreChanged(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
        }
    }
}
