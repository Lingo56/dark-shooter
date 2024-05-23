using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEnemyMovement : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float hoverSpeed = 2f; // Speed at which the enemy hovers towards the player
    public float backwardForce = 100f; // Force applied when hit by the gun
    public float hitDistance = 1f; // Distance to move when hit
    public float rotationSpeed = 2f; // Speed at which the enemy resets rotation towards the player

    private bool isHit = false; // Flag to indicate if the enemy is hit by the gun
    private Vector3 direction;
    private Vector3 targetPosition;
    private Vector3 originalScale; // Store the original scale of the enemy

    public float stutterDuration = 0.5f; // Duration of the stutter effect
    public float stutterScaleFactor = 1.2f; // Scale factor for the stutter effect
    public int stutterFrequency = 10; // Number of times the scale stutters

    private List<Vector3> hitNormals = new List<Vector3>(); // List to store hit normals

    void Start()
    {
        originalScale = transform.localScale; // Initialize the original scale
    }

    void Update()
    {
        if (player != null)
        {
            if (!isHit)
            {
                // Calculate the direction from the enemy to the player
                direction = (player.position - transform.position).normalized;

                // Calculate the target position
                targetPosition = player.position - direction;

                // Move the enemy towards the target position based on hover speed
                transform.position += direction * hoverSpeed * Time.deltaTime;

                // Smoothly rotate the enemy to face the player
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // Lerp the enemy towards the target position based on shot force
                transform.position = Vector3.Lerp(transform.position, targetPosition, backwardForce * Time.deltaTime);
            }

            // Calculate the distance between current and target position
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance <= 0.5f && isHit == true)
                isHit = false;

            // Clamp the distance to prevent overshooting
            if (Vector3.Distance(transform.position, targetPosition) > distance)
            {
                transform.position = targetPosition;
            }
        }
    }

    public void ApplyHitNormal(Vector3 hitNormal)
    {
        // Add hit normal to the list
        hitNormals.Add(hitNormal);
    }

    public void ApplyAccumulatedForce()
    {
        if (hitNormals.Count > 0)
        {
            // Calculate the average direction from all hit normals
            direction = Vector3.zero;
            foreach (Vector3 normal in hitNormals)
            {
                direction += normal;
            }
            direction /= hitNormals.Count;

            // Calculate the target position
            targetPosition = transform.position - direction * hitDistance;

            isHit = true;

            // Clear the list of hit normals
            hitNormals.Clear();

            // Start the stutter effect
            StartCoroutine(StutterEffect());
        }
    }

    private IEnumerator StutterEffect()
    {
        Vector3 targetScaleUp = originalScale * stutterScaleFactor;
        Vector3 targetScaleDown = originalScale;
        float halfDuration = stutterDuration / (2 * stutterFrequency);

        for (int i = 0; i < stutterFrequency; i++)
        {
            // Lerp scale up
            float elapsedTime = 0f;
            while (elapsedTime < halfDuration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScaleUp, elapsedTime / halfDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScaleUp;

            // Lerp scale down
            elapsedTime = 0f;
            while (elapsedTime < halfDuration)
            {
                transform.localScale = Vector3.Lerp(targetScaleUp, targetScaleDown, elapsedTime / halfDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScaleDown;
        }

        // Ensure the scale is reset to original after the stutter effect
        transform.localScale = originalScale;
    }
}
