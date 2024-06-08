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

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(int damage, RaycastHit hit) 
    {
        health -= damage;

        if (health < 0)
        {
            enemyMovement.StopFollowingAndEnableGravity();
        }
        else {
            flashEffect.Flash(0.4f, 1);
            enemyMovement.ApplyHitNormal(hit.normal);
        }
    }

}
