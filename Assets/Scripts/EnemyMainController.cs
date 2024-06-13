using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyMainController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int health;

    [SerializeField] private EnemyMainMovement enemyMovement;
    [SerializeField] private EnemyMainHitFlash flashEffect;

    void Start()
    {
        health = maxHealth;
    }

    // TODO: StopFollowingAndEnableGravity is run before ApplyAccumulatedForce is
    // Can run ApplyAccumulatedForce in StopFollowingAndEnableGravity
    // But then it wouldn't count the bullets that hit the enemy after it died
    public void ApplyDamage(int damage, RaycastHit hit)
    {
        health -= damage;
        enemyMovement.ApplyHitNormal(hit.normal);

        if (isAlive())
        {
            flashEffect.Flash(0.4f, 1);
        }
        else
        {
            Debug.Log("dead");
            enemyMovement.StopFollowingAndEnableGravity();
        }
    }

    public bool isAlive()
    {
        return health > 0;
    }
}
