using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnEnemyHit;

    public static event Action OnEnemyDeath;

    public static event Action<GameObject> OnSpecificEnemyDeath;

    public static event Action<float, float> OnTimerUpdate;

    public static void EnemyHitAlive()
    {
        OnEnemyHit?.Invoke();
    }

    public static void EnemyDeath()
    {
        OnEnemyDeath?.Invoke();
    }

    public static void SpecificEnemyDeath(GameObject gameObject)
    {
        OnEnemyDeath?.Invoke();
        OnSpecificEnemyDeath?.Invoke(gameObject);
    }

    public static void TimerUpdate(float elapsedTime, float timerDuration)
    {
        OnTimerUpdate?.Invoke(elapsedTime, timerDuration);
    }
}