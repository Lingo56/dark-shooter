using TMPro;
using UnityEngine;

// TODO: Maybe the cleanest way to do this would be to instantiate or setup an object pool of death markers
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