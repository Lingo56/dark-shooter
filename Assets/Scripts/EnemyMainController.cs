using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMainController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int health;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(int damage) 
    {
        health -= damage;

        if (health < 0)
        {
            Destroy(gameObject);
        }
    }

}
