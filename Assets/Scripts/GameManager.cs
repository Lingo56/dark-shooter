using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int hitCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnEnemyHit += RegisterHit; // Subscribe to the event
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            GameEvents.OnEnemyHit -= RegisterHit; // Unsubscribe from the event
        }
    }

    public void RegisterHit()
    {
        hitCount++;
        UIEvents.HitCountChanged(hitCount); // Trigger the UI event
    }

    public int GetHitCount()
    {
        return hitCount;
    }
}
