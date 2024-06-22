using TMPro;
using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;  // Assign the UI Canvas with black screen and text here
    [SerializeField] private TextMeshProUGUI scoreText;  // Assign the UI Canvas with black screen and text here

    private bool isPaused = false;

    // Toggle function to pause/resume the game
    public void TogglePause(int score)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;  // Pause the game
        pauseScreen.SetActive(true);  // Show the pause screen
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;  // Resume the game
        pauseScreen.SetActive(false);  // Hide the pause screen
        isPaused = false;
    }

    public void EndGame(int score)
    {
        Time.timeScale = 0;  // Pause the game
        pauseScreen.SetActive(true);  // Show the pause screen
        scoreText.text = "Score: " + score.ToString();
        isPaused = true;
    }
}