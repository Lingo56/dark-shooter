using UnityEngine;
using UnityEngine.Pool;

public class EnemyMainController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EnemyMainMovement enemyMovement;

    [SerializeField] private EnemyMainHitFlash flashEffect;

    [Header("Enemy Settings")]
    [SerializeField] private int maxHealth = 100;

    private ObjectPool<GameObject> objectPool;  // Reference to the Object Pool
    private int health;
    private int totalDamage;
    private bool initializedDeath = false;

    private void Start()
    {
        health = maxHealth;
    }

    public void SetObjectPool(ObjectPool<GameObject> pool)
    {
        objectPool = pool;
    }

    public void ProcessHit(int damage, float bulletHitForce, RaycastHit hit)
    {
        totalDamage += damage;
        enemyMovement.HandleBulletImpact(hit.normal, bulletHitForce);

        if (IsAlive())
        {
            GameEvents.EnemyHitAlive();
        }
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public void HandleEnemyDamage()
    {
        health -= totalDamage;
        totalDamage = 0;

        enemyMovement.ApplyAccumulatedForce();

        if (!IsAlive() && !initializedDeath)
        {
            flashEffect.Flash(enemyMovement.DeathLaunchPeriod);
            StartCoroutine(enemyMovement.EnableDeathMovement());
            initializedDeath = true;

            // Return the enemy object to the pool on death
            ReturnToPool();
        }
        else if (IsAlive())
        {
            flashEffect.Flash(0.1f);
        }
    }

    private void ReturnToPool()
    {
        if (objectPool != null)
        {
            objectPool.Release(gameObject);
        }
        else
        {
            Debug.LogWarning("Object pool reference is null. Unable to return to pool.");
            Destroy(gameObject); // Fallback to destroy if pool reference is not set
        }
    }
}