using UnityEngine;

public class GameSpawner : MonoBehaviour
{
    [SerializeField] private GameObject SpawnEnemy;
    [SerializeField] private float spawnInterval = 2f;  // Time in seconds between each spawn
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
            // Calculate rotation to look at the player
            Vector3 directionToPlayer = playerObject.transform.position - transform.position;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToPlayer);

            Instantiate(SpawnEnemy, transform.position, spawnRotation);
        }
        else
        {
            Debug.LogError("Player object is not assigned.");
        }
    }
}