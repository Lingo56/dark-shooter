using TMPro;
using UnityEngine;

public class UIComboCounter : MonoBehaviour
{
    public TextMeshProUGUI comboCountText;

    private void OnEnable()
    {
        UIEvents.OnHitCountChanged += UpdateHitCountUI; // Subscribe to the event
    }

    private void OnDisable()
    {
        UIEvents.OnHitCountChanged -= UpdateHitCountUI; // Unsubscribe from the event
    }

    private void UpdateHitCountUI(int newHitCount)
    {
        comboCountText.text = "" + newHitCount; // Update the UI
    }
}