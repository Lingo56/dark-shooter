using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainEnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player's transform
    [SerializeField] private float maxSpeed = 2f; // Maximum speed of the enemy
    [SerializeField] private float acceleration = 1f; // Acceleration rate
    [SerializeField] private float deceleration = 2f; // Deceleration rate
    [SerializeField] private float hitForce = 10f; // Force applied when hit by the gun
    [SerializeField] private float rotationSpeed = 2f; // Speed at which the enemy resets rotation towards the player

    private bool isHit = false; // Flag to indicate if the enemy is hit by the gun
    private Vector3 velocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 direction;
    private Vector3 targetPosition;
    private Vector3 originalScale; // Store the original scale of the enemy

    [SerializeField] private float stutterDuration = 0.5f; // Duration of the stutter effect
    [SerializeField] private float stutterScaleFactor = 1.2f; // Scale factor for the stutter effect
    [SerializeField] private int stutterFrequency = 10; // Number of times the scale stutters

    private List<Vector3> hitNormals = new List<Vector3>(); // List to store hit normals

    void Start()
    {

    }

    void Update()
    {
        // Calculate direction to the player
        Vector3 direction = (player.position - transform.position).normalized;

        // Accelerate towards the player
        velocity += direction * acceleration * Time.deltaTime;

        // Clamp the velocity to the maximum speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // Move the enemy
        transform.position += velocity * Time.deltaTime;

        // Rotate the enemy to face the direction it's moving
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("colliding with something");

        if (collision.gameObject.CompareTag("Floor"))
        {
            Debug.Log("colliding with floor");
        }
    }
}
