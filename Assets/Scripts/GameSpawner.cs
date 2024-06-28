using UnityEngine;

public class GameSpawner : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;  // Reference to the Object Pool
    [SerializeField] private float spawnInterval = 2f;  // Time in seconds between each spawn
    private GameObject playerObject;

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player object not found! Make sure the player is tagged as 'Player'.");
        }
        else
        {
            InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);
        }

        GameEvents.OnSpecificEnemyDeath += HandleSpecificEnemyDeath;
    }

    private void OnDestroy()
    {
        GameEvents.OnSpecificEnemyDeath -= HandleSpecificEnemyDeath;
    }

    private void SpawnObject()
    {
        if (playerObject != null)
        {
            Vector3 directionToPlayer = playerObject.transform.position - transform.position;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToPlayer);

            GameObject enemy = objectPool.GetObject();
            enemy.transform.position = transform.position;
            enemy.transform.rotation = spawnRotation;

            EnemyMainController enemyController = enemy.GetComponent<EnemyMainController>();
            if (enemyController != null)
            {
                enemyController.SetObjectPool(objectPool);  // Set the Object Pool reference
            }
        }
        else
        {
            Debug.LogError("Player object is not assigned.");
        }
    }

    private void HandleSpecificEnemyDeath(GameObject enemy)
    {
        if (objectPool != null)
        {
            objectPool.ReturnObject(enemy);
        }
    }
}