using UnityEngine;
using UnityEngine.Pool;

public class GameSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Reference to the enemy prefab
    [SerializeField] private float spawnInterval = 2f; // Time in seconds between each spawn

    private GameObject playerObject;
    private ObjectPool<GameObject> enemyPool; // Unity's built-in object pool

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player object not found! Make sure the player is tagged as 'Player'.");
            return;
        }

        // Initialize the object pool
        enemyPool = new ObjectPool<GameObject>(
            CreatePooledItem,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            false, // Collection checks
            10, // Default capacity
            20 // Maximum size
        );

        InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);

        GameEvents.OnSpecificEnemyDeath += HandleSpecificEnemyDeath;
    }

    private void OnDestroy()
    {
        GameEvents.OnSpecificEnemyDeath -= HandleSpecificEnemyDeath;
    }

    private GameObject CreatePooledItem()
    {
        return Instantiate(enemyPrefab);
    }

    private void OnTakeFromPool(GameObject enemy)
    {
        enemy.SetActive(true);
    }

    private void OnReturnedToPool(GameObject enemy)
    {
        enemy.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject enemy)
    {
        Destroy(enemy);
    }

    private void SpawnObject()
    {
        if (playerObject != null)
        {
            Vector3 directionToPlayer = playerObject.transform.position - transform.position;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToPlayer);

            GameObject enemy = enemyPool.Get(); // Get an enemy from the pool
            enemy.transform.position = transform.position;
            enemy.transform.rotation = spawnRotation;

            EnemyMainController enemyController = enemy.GetComponent<EnemyMainController>();
            if (enemyController != null)
            {
                enemyController.SetObjectPool(enemyPool); // Set the object pool reference if needed
            }
        }
        else
        {
            Debug.LogError("Player object is not assigned.");
        }
    }

    private void HandleSpecificEnemyDeath(GameObject enemy)
    {
        if (enemyPool != null)
        {
            enemyPool.Release(enemy); // Return the enemy to the pool
        }
    }
}