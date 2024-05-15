using UnityEngine;
using System.Collections;

/**
 *	Rapidly sets a light on/off.
 *	
 *	(c) 2015, Jean Moreno
**/

[RequireComponent(typeof(Light))]
public class WFX_LightFlicker : MonoBehaviour
{
    public float onTime = 0.05f;  // Time the light is on

    private float originalOnTime;
    private Coroutine flickerCoroutine;

    void OnEnable()
    {
        originalOnTime = onTime; // Store the original onTime value
        flickerCoroutine = StartCoroutine(FlickerAndDisable());
    }

    IEnumerator FlickerAndDisable()
    {
        GetComponent<Light>().enabled = true; // Start with the light on

        yield return new WaitForSeconds(onTime);

        // Disable the GameObject after onTime
        gameObject.SetActive(false);
    }

    // Public method to reset the timer
    public void ResetTimer()
    {
        onTime = originalOnTime; // Reset onTime to its original value
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(FlickerAndDisable());
        }
    }
}
