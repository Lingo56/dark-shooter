using System.Collections;
using UnityEngine;

public class EnemyMainController : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private EnemyMainMovement enemyMovement;

    [SerializeField] private EnemyMainHitFlash flashEffect;
    [SerializeField] private Material enemyMaterial;

    [Header("Enemy Settings")]
    [SerializeField] private int maxHealth = 100;

    private int health;
    private int totalDamage;
    private bool initializedDeath = false;

    private Collider enemyCollider;
    private Renderer enemyRenderer;

    private void Start()
    {
        health = maxHealth;
        enemyCollider = GetComponent<Collider>();
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

            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            StartCoroutine(enemyMovement.EnableDeathMovement());
            initializedDeath = true;
            GameEvents.SpecificEnemyDeath(gameObject);

            Destroy(gameObject, 3.0f);
        }
        else if (IsAlive())
        {
            flashEffect.Flash(0.1f);
        }
    }

    private IEnumerator FadeOutAndDestroy(float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            if (enemyMaterial != null)
            {
                enemyMaterial.SetFloat("_Fade", t);
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}