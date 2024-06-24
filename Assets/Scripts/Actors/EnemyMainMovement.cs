using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Fix enemies not kicking back enough when dead
public class EnemyMainMovement : MonoBehaviour
{
    [Header("Dependancies")]
    private Transform player; // Reference to the player's transform

    [Header("Enemy Movement Settings")]
    [SerializeField] private float maxSpeed = 2f; // Maximum speed of the enemy

    [SerializeField] private float acceleration = 1f; // Acceleration rate towards the player
    [SerializeField] private float hitDeaccelerationFactor = 1f; // Accumulated acceleration from hits
    [SerializeField] private float deathForceMultiplier = 1f; // Multiplier for how much faster the enemy should kick back when they die
    [SerializeField] private float deathLaunchPeriod = 1f; // How long to wait before enemy launches from death
    [SerializeField] private float rotationSpeed = 2f; // Speed at which the enemy resets rotation towards the player

    private Vector3 velocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 followVelocity = Vector3.zero; // Current velocity of the enemy
    private Vector3 hitVelocity = Vector3.zero; // Current velocity of the enemy

    private List<Vector3> hitNormals = new(); // List to store hit normals

    private AudioSource hitAudio;
    private Rigidbody rb;
    private float hitForce; // Force applied when hit by the gun
    private bool alive = true;
    private bool isWaiting = false;

    private void Start()
    {
        // Find the GameObject tagged as "Player" and get its Transform component
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found! Make sure the player is tagged as 'Player'.");
        }

        hitAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
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
        ShowVelocityDebugLine();

        if (alive && !isWaiting)
        {
            #region Hit Movement

            if (hitVelocity.magnitude > 0.01f)
            {
                hitVelocity *= hitDeaccelerationFactor;
            }
            else
            {
                hitVelocity = Vector3.zero;
            }

            #endregion Hit Movement

            #region Follow Movement

            // Calculate direction to the player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            // Accelerate towards the player
            Vector3 accelerationVector = acceleration * Time.fixedDeltaTime * directionToPlayer;

            // Apply acceleration towards the player
            followVelocity += accelerationVector;

            // Clamp the velocity to the maximum speed
            followVelocity = Vector3.ClampMagnitude(followVelocity, maxSpeed);

            #endregion Follow Movement

            velocity = followVelocity + hitVelocity;

            rb.velocity = velocity;
        }
        else if (!alive && !isWaiting)
        {
            ApplyDeathHit();
        }
    }

    private void ShowVelocityDebugLine()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + velocity;
        Color lineColor = Color.red; // Choose a color for the line

        Debug.DrawLine(startPosition, endPosition, lineColor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            // Reflect the velocity vector off the floor
            Vector3 normal = collision.contacts[0].normal;
            velocity = Vector3.Reflect(velocity, normal);
        }
    }

    public void HandleBulletImpact(Vector3 hitNormal, float bulletHitForce)
    {
        // Add hit normal to the list
        hitNormals.Add(hitNormal);
        hitForce += bulletHitForce;
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
            hitVelocity = hitDirection * -hitForce;

            // Clear the list of hit normals
            hitNormals.Clear();
            hitForce = 0;
            followVelocity = Vector3.zero;

            hitAudio.Play();
        }
    }

    public IEnumerator EnableDeathMovement()
    {
        alive = false;
        isWaiting = true;

        // Stop the follow acceleration/velocity
        followVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(deathLaunchPeriod);
        isWaiting = false;

        // Enable gravity on the Rigidbody
        rb.useGravity = true;

        Debug.Log(hitVelocity);
        ApplyDeathHit();
    }

    private void ApplyDeathHit()
    {
        // Apply the hit velocity as a force to the Rigidbody
        rb.AddForce(hitVelocity * deathForceMultiplier, ForceMode.Impulse);
        hitVelocity = Vector3.zero;
    }
}