using UI;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UIComboTrackerCrosshair : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private RectTransform rectTransform; // Reference to the RectTransform of the UI element

    [SerializeField] private RectTransform crosshairRectTransform; // Reference to the RectTransform of the UI element
    [SerializeField] private UICircle uiCircle; // Reference to the UICircle component

    [Header("Tracker Circle Config")]
    [SerializeField] private float trackerThickness; // Reference to the UICircle component

    [SerializeField] private float outlineThickness; // Reference to the UICircle component

    private void OnEnable()
    {
        UIEvents.OnHitCountChanged += UpdateTrackerSize; // Subscribe to the event
    }

    private void OnDisable()
    {
        UIEvents.OnHitCountChanged -= UpdateTrackerSize; // Unsubscribe from the event
    }

    private void Start()
    {
        // Initialize the RectTransform if not assigned in the Inspector
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Initialize the UICircle if not assigned in the Inspector
        if (uiCircle == null)
        {
            uiCircle = GetComponent<UICircle>();
        }
    }

    private void UpdateTrackerSize(int currentCombo)
    {
        if (currentCombo > 10)
        {
            rectTransform.sizeDelta = crosshairRectTransform.sizeDelta + new Vector2(currentCombo / 3, currentCombo / 3);
            uiCircle.Thickness = trackerThickness;
            uiCircle.OutlineThickness = outlineThickness;
        }
        else
        {
            rectTransform.sizeDelta = Vector2.zero; // Reset to zero after delay
            uiCircle.Thickness = 0; // Reset the thickness to 0
            uiCircle.OutlineThickness = 0;
        }
    }
}