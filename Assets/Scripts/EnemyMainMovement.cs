using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMainMovement : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player's transform
    [SerializeField] private float maxSpeed = 2f; // Maximum speed of the enemy
    [SerializeField] private float acceleration = 1f; // Acceleration rate towards the player
    [SerializeField] private float hitForce = 10f; // Force applied when hit by the gun
    [SerializeField] private float rotationSpeed = 2f; // Speed at which the enemy resets rotation towards the player
    [SerializeField] private float hitDamping = 0.95f; // Damping factor for reducing hit acceleration

    private Vector3 velocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 followVelocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 hitVelocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 hitAcceleration = Vector3.zero; // Accumulated acceleration from hits
    private Vector3 originalScale; // Store the original scale of the enemy

    [SerializeField] private float stutterDuration = 0.5f; // Duration of the stutter effect
    [SerializeField] private float stutterScaleFactor = 1.2f; // Scale factor for the stutter effect
    [SerializeField] private int stutterFrequency = 10; // Number of times the scale stutters

    private List<Vector3> hitNormals = new List<Vector3>(); // List to store hit normals

    public EnemyMainHitFlash flashEffect;
    private AudioSource hitAudio;

    void Start()
    {
        hitAudio = GetComponent<AudioSource>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        // Rotate the enemy to face the direction it's moving
        if (followVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(followVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        #region Follow Movement

        // Calculate direction to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Accelerate towards the player
        Vector3 accelerationVector = directionToPlayer * acceleration * Time.fixedDeltaTime;

        // Apply acceleration towards the player
        followVelocity += accelerationVector;

        // Clamp the velocity to the maximum speed
        followVelocity = Vector3.ClampMagnitude(followVelocity, maxSpeed);

        #endregion

        #region Hit Movement

        // Apply acceleration from hits
        hitVelocity += hitAcceleration;

        // Reduce hit acceleration over time
        hitVelocity *= hitDamping;
        hitAcceleration *= hitDamping;

        #endregion

        velocity = followVelocity + hitVelocity;

        // Move the enemy
        transform.position += velocity * Time.deltaTime;

        // Stop the hit acceleration completely if it is very small
        if (hitAcceleration.magnitude < 0.01f)
        {
            hitAcceleration = Vector3.zero;
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + velocity;
        Color lineColor = Color.red; // Choose a color for the line

        Debug.DrawLine(startPosition, endPosition, lineColor);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            // Reflect the velocity vector off the floor
            Vector3 normal = collision.contacts[0].normal;
            velocity = Vector3.Reflect(velocity, normal);
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
            Vector3 hitDirection = Vector3.zero;
            foreach (Vector3 normal in hitNormals)
            {
                hitDirection += normal;
            }
            hitDirection /= hitNormals.Count;

            // Apply the hit force as acceleration
            hitAcceleration = hitDirection * -hitForce;

            // Clear the list of hit normals
            hitNormals.Clear();
            followVelocity = Vector3.zero;

            // Start the stutter effect
            //StartCoroutine(StutterEffect());

            flashEffect.Flash(0.4f, 1);
            hitAudio.Play();
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
