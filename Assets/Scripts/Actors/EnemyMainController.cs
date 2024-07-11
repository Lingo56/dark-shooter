using System.Collections;
using UnityEngine;

public class EnemyMainController : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private EnemyMainMovement enemyMovement;

    [SerializeField] private EnemyMainHitFlash flashEffect;

    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;

    private float health;
    private int totalDamage;
    private bool initializedDeath = false;

    private Collider enemyCollider;
    private Renderer enemyRenderer;
    private Material enemyMaterial;

    private void Start()
    {
        health = maxHealth;
        enemyCollider = GetComponent<Collider>();

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyMaterial = enemyRenderer.material;
        }
        else
        {
            Debug.LogError("Renderer component not found on enemy GameObject!");
        }
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
            flashEffect.Flash(enemyMovement.DeathPausePeriod);

            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            StartCoroutine(FadeOutAndDestroy(1f));
            StartCoroutine(enemyMovement.EnableDeathMovement());
            initializedDeath = true;
            GameEvents.SpecificEnemyDeath(gameObject);
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
            float t = 1 - ((Time.time - startTime) / duration);
            if (enemyMaterial != null)
            {
                enemyMaterial.SetFloat("_Fade", t);
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}