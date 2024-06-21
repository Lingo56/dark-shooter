using UnityEngine;

public class UIHUDTimer : MonoBehaviour
{
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
    }
}