using UnityEngine;
using UnityEngine.Serialization;

public class AOESpawner : MonoBehaviour
{
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private GameObject ground;
    [SerializeField] private float spawnInterval = 2f;  // Time in seconds between each spawn
    [SerializeField] private float heightOffset = 0.01f;  // Time in seconds between each spawn
    private GameObject playerObject;

    private void Start()
    {
        // Assign the class-level playerObject
        playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player object not found! Make sure the player is tagged as 'Player'.");
        }
        else
        {
            InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);
        }
    }

    private void SpawnObject()
    {
        if (playerObject != null)
        {
            // Calculate spawn position directly under the player
            Vector3 spawnPosition = new Vector3(
                playerObject.transform.position.x, 
                ground.transform.position.y + heightOffset,  // Assuming the spawner is at ground level
                playerObject.transform.position.z
            );
            
            // Ensure the spawned object is aligned with the ground plane
            Quaternion spawnRotation = Quaternion.Euler(0f, playerObject.transform.eulerAngles.y, 0f);

            Instantiate(attackEffect, spawnPosition, spawnRotation);
        }
        else
        {
            Debug.LogError("Player object is not assigned.");
        }
    }
}