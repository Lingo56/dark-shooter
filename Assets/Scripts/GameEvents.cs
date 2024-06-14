using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnEnemyHit;
    public static event Action OnEnemyDeath;

    public static void EnemyHitAlive()
    {
        OnEnemyHit?.Invoke();
    }
    public static void EnemyDeath()
    {
        OnEnemyDeath?.Invoke();
    }
}
