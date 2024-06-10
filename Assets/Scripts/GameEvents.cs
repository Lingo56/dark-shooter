using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnEnemyHit;

    public static void EnemyHit()
    {
        OnEnemyHit?.Invoke();
    }
}
