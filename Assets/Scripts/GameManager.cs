using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int hitCount;
    private int score;
    private Coroutine countdownCoroutine; // To store the reference to the running coroutine

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnEnemyHit += RegisterHit; // Subscribe to the event
            GameEvents.OnEnemyDeath += RegisterDeath;
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
            GameEvents.OnEnemyDeath -= RegisterDeath;
        }
    }

    public void RegisterHit()
    {
        hitCount++;
        UIEvents.HitCountChanged(hitCount); // Trigger the UI event

        // Stop the countdown coroutine if it's running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        // Start the countdown coroutine
        countdownCoroutine = StartCoroutine(CountdownHitCount());
    }

    public void RegisterDeath()
    {
        score++;
        UIEvents.ScoreChanged(score);
    }

    private IEnumerator CountdownHitCount()
    {
        yield return new WaitForSeconds(2f);

        float comboReductionTimer = 0.5f;

        while (hitCount > 0)
        {
            hitCount--;
            UIEvents.HitCountChanged(hitCount); // Update the UI
            yield return new WaitForSeconds(comboReductionTimer); // Adjust the countdown interval as needed
            comboReductionTimer /= 1.25f;
        }

        countdownCoroutine = null; // Clear the coroutine reference when done
    }

    public int GetHitCount()
    {
        return hitCount;
    }
}
