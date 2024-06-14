using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScoreCounter : MonoBehaviour
{
    public TextMeshProUGUI scoreCountText;

    private void OnEnable()
    {
        UIEvents.OnScoreChanged += UpdateScoreUI; // Subscribe to the event
    }

    private void OnDisable()
    {
        UIEvents.OnScoreChanged -= UpdateScoreUI; // Unsubscribe from the event
    }

    private void UpdateScoreUI(int newScore)
    {
        scoreCountText.text = "" + newScore; // Update the UI
    }
}
