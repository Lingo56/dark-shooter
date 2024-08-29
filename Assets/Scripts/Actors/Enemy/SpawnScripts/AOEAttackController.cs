using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackController : MonoBehaviour
{
    public float minDistance = 0.67f; // Minimum value for _EdgeFadeDistance
    public float maxDistance = 1.0f; // Maximum value for _EdgeFadeDistance
    public float speed = 1.0f; // Speed of the oscillation

    private Material material;
    private float time;

    void Start()
    {
        // Get the material of the current object
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        else
        {
            Debug.LogError("No Renderer found on the current GameObject.");
        }
    }

    void Update()
    {
        if (material != null)
        {
            // Increment time
            time += Time.deltaTime * speed;

            // Calculate the new value using a sinusoidal function
            float newValue = Mathf.Lerp(minDistance, maxDistance, (Mathf.Sin(time) + 1.0f) / 2.0f);

            // Apply the new value to the _EdgeFadeDistance property of the material
            material.SetFloat("_EdgeFadeDistance", newValue);
        }
    }
}
