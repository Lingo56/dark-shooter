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

    // TODO: Fix bullet velocity not being sent to enemy when they die
    // EnableDeathMovement is run before ApplyAccumulatedForce is
    // Can run ApplyAccumulatedForce in EnableDeathMovement
    // But then it wouldn't count the bullets that hit the enemy after it died
    public void TrackHitDamage(int damage, RaycastHit hit)
    {
        totalDamage += damage;
        enemyMovement.ApplyHitNormal(hit.normal);

        if (isAlive())
        {
            flashEffect.Flash(0.4f, 1);
        }
    }

    public bool isAlive()
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

        if (!isAlive() && !initializedDeath) {
            enemyMovement.EnableDeathMovement();
            initializedDeath = true;
        }
    }
}
