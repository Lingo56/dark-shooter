using UnityEngine;

public class UITimer : MonoBehaviour
{
    [SerializeField] private Material radialMaterial; // The material with the radial shader

    private void OnEnable()
    {
        GameEvents.OnTimerUpdate += UpdateTimer; // Subscribe to the event
    }

    private void OnDisable()
    {
        GameEvents.OnTimerUpdate -= UpdateTimer; // Subscribe to the event
        ResetTimer();
    }

    public void UpdateTimer(float elapsedTime, float timerDuration)
    {
        float fillAmount = elapsedTime / timerDuration;
        radialMaterial.SetFloat("_Cutoff", 1 - fillAmount);
    }

    public void ResetTimer()
    {
        radialMaterial.SetFloat("_Cutoff", 1);
    }
}