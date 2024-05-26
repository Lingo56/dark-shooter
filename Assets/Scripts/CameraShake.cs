using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Camera fpsCam;

    void Start()
    {
        fpsCam = GetComponent<Camera>();
    }

    public IEnumerator Shake(float amount, float duration)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * amount;
            float y = Random.Range(-1f, 1f) * amount;

            // Apply the shake offset to the current camera rotation
            fpsCam.transform.localRotation *= Quaternion.Euler(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is restored
        fpsCam.transform.localRotation = Quaternion.identity;
    }

    /*
    // TODO: Fix this conflicting with mouse look
    private IEnumerator ShakeCamera()
    {
        Quaternion originalCamRotation = fpsCam.transform.localRotation;
        Quaternion targetRotation = originalCamRotation * Quaternion.Euler(-shakeAmount, 0, 0);

        float elapsed = 0.0f;

        // Shake the camera
        while (elapsed < shakeRiseDuration)
        {
            float t = elapsed / shakeRiseDuration; // Calculate interpolation parameter
            fpsCam.transform.localRotation = Quaternion.Lerp(originalCamRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is the target rotation
        fpsCam.transform.localRotation = targetRotation;

        // Reset the elapsed time and rotation
        elapsed = 0.0f;
        Quaternion newCameraRotation = fpsCam.transform.localRotation;

        // Smoothly interpolate back to original rotation
        while (elapsed < shakeFallDuration)
        {
            float t = elapsed / shakeFallDuration; // Calculate interpolation parameter
            fpsCam.transform.localRotation = Quaternion.Lerp(newCameraRotation, originalCamRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exactly the original one
        fpsCam.transform.localRotation = originalCamRotation;
    }
    */
}
