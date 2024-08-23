using TMPro;
using UnityEngine;

namespace UI
{
    public class UIHUDTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        private void OnEnable()
        {
            GameEvents.OnTimerUpdate += UpdateTimer; // Subscribe to the event
        }

        private void OnDisable()
        {
            GameEvents.OnTimerUpdate -= UpdateTimer; // Subscribe to the event
        }

        public void UpdateTimer(float elapsedTime, float timerDuration)
        {
            timerText.text = elapsedTime.ToString();
        }
    }
}