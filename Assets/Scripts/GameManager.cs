using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float timerDuration = 10f; // In seconds
    [SerializeField] private GamePauseManager pauseManager;

    private int hitCount;
    private int score;
    private Coroutine countdownCoroutine; // To store the reference to the running coroutine
    private float elapsedTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnEnemyHit += RegisterEnemyHit; // Subscribe to the event
            GameEvents.OnEnemyDeath += RegisterEnemyDeath;
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
            GameEvents.OnEnemyHit -= RegisterEnemyHit; // Unsubscribe from the event
            GameEvents.OnEnemyDeath -= RegisterEnemyDeath;
        }
    }

    private void Update()
    {
        RunGameTimer();

        // Example to trigger pause (e.g., pressing the 'P' key)
        if (Input.GetKeyDown(KeyCode.P))
        {
            pauseManager.TogglePause(score);
        }
    }

    public void RegisterEnemyHit()
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

    public void RegisterEnemyDeath()
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

    private void RunGameTimer()
    {
        if (elapsedTime < timerDuration)
        {
            elapsedTime += Time.deltaTime;
            GameEvents.TimerUpdate(elapsedTime, timerDuration);
        }
        else
        {
            // Timer has finished
            // You can trigger any event you want here
        }
    }
}