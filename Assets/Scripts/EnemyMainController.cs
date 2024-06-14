using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyMainController : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private EnemyMainMovement enemyMovement;
    [SerializeField] private EnemyMainHitFlash flashEffect;

    [Header("Enemy Settings")]
    [SerializeField] private int maxHealth = 100;
    private int health;
    private int totalDamage;
    private bool initializedDeath = false;

    void Start()
    {
        health = maxHealth;
    }

    public void ProcessHit(int damage, float bulletHitForce, RaycastHit hit)
    {
        totalDamage += damage;
        enemyMovement.HandleBulletImpact(hit.normal, bulletHitForce);

        if (IsAlive())
        {
            flashEffect.Flash(0.4f, 1);
        }
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    // TODO: Use Physics.IgnoreCollision to ignore colliding with the player once enemy dies
    // Could maybe be pointless depending on if the enemies just disappear a couple seconds after death
    public void HandleEnemyDamage() 
    {
        health -= totalDamage;
        totalDamage = 0;

        enemyMovement.ApplyAccumulatedForce();

        if (!IsAlive() && !initializedDeath) {
            enemyMovement.EnableDeathMovement();
            initializedDeath = true;
        }
    }
}
