using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    public Material radialMaterial; // The material with the radial shader
    public float timerDuration = 10f; // Duration of the timer in seconds

    private float elapsedTime = 0f;

    void Update()
    {
        if (elapsedTime < timerDuration)
        {
            elapsedTime += Time.deltaTime;
            float fillAmount = elapsedTime / timerDuration;
            radialMaterial.SetFloat("_Cutoff", 1 - fillAmount);
        }
        else
        {
            // Timer has finished
            // You can trigger any event you want here
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        radialMaterial.SetFloat("_Cutoff", 1);
    }
}
