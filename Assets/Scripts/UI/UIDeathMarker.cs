using UnityEngine;
using UnityEngine.UI.Extensions;

// TODO: Remake this so that it updates on hit
// Also make it so that the circle expands to the current size of the combo meter
public class UIDeathMarker : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private RectTransform rectTransform; // Reference to the RectTransform of the UI element

    [SerializeField] private RectTransform crosshairCircleRectTransform;
    [SerializeField] private RectTransform comboTrackerRectTransform;
    [SerializeField] private UICircle uiCircle; // Reference to the UICircle component

    [Header("Config")]
    [SerializeField] private float animationDuration = 0.15f; // Duration of the animation in seconds

    [SerializeField] private float postAnimationDelay = 0.2f; // Delay after animation before resetting to zero
    [SerializeField] private float fadeOutDuration = 0.5f; // Duration of the fade-out effect
    [SerializeField] private float markerDistance = 15f; // How far the hit marker goes

    private float timer = 0f; // Timer to track the animation progress
    private bool isAnimating = false; // Flag to check if animation is running
    private bool hasCompletedAnimation = false; // Flag to check if animation has completed
    private bool isFadingOut = false; // Flag to check if fading out

    private Vector2 initialSize = Vector2.zero; // Initial size (usually starts from 0,0)
    private Vector2 targetSize = new Vector2(30f, 30f); // Target size to reach (30x30)

    private void OnEnable()
    {
        GameEvents.OnEnemyHit += EnableMarker; // Subscribe to the event
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyHit -= EnableMarker; // Unsubscribe from the event
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

        initialSize = crosshairCircleRectTransform.sizeDelta; // Store initial size
    }

    private void Update()
    {
        if (isAnimating)
        {
            if (!hasCompletedAnimation)
            {
                timer += Time.deltaTime;

                // Calculate lerp factor based on animation progress
                float lerpFactor = timer / animationDuration;

                // Interpolate between initialSize and targetSize
                Vector2 newSize = Vector2.Lerp(initialSize, targetSize, lerpFactor);

                // Apply the new size to the RectTransform
                rectTransform.sizeDelta = newSize;

                // Adjust the thickness of the UICircle
                uiCircle.Thickness = 4; // Example: set thickness to half the width
                uiCircle.OutlineThickness = 1; // Example: set thickness to half the width

                // Check if animation is complete
                if (lerpFactor >= 1f)
                {
                    hasCompletedAnimation = true;
                    timer = 0f;
                }
            }

            // After completing animation, wait for postAnimationDelay before resetting
            if (hasCompletedAnimation)
            {
                timer += Time.deltaTime;

                if (timer >= postAnimationDelay)
                {
                    // Start fading out
                    isFadingOut = true;
                    isAnimating = false;
                    timer = 0f;
                }
            }

            // Handle fading out
            if (isFadingOut)
            {
                timer += Time.deltaTime;

                // Calculate lerp factor based on fade out progress
                float fadeFactor = Mathf.Clamp01(timer / fadeOutDuration);

                // Get current color and set the alpha value based on fadeFactor
                Color color = uiCircle.color;
                color.a = Mathf.Lerp(1f, 0f, fadeFactor);
                uiCircle.color = color;

                // Check if fade out is complete
                if (fadeFactor >= 1f)
                {
                    // Reset to initial state
                    rectTransform.sizeDelta = initialSize;
                    uiCircle.Thickness = 0;
                    uiCircle.OutlineThickness = 0;
                    isFadingOut = false;
                    timer = 0f;
                }
            }
        }
    }

    private void EnableMarker()
    {
        initialSize = comboTrackerRectTransform.sizeDelta; // Update initial size
        targetSize = comboTrackerRectTransform.sizeDelta + new Vector2(markerDistance, markerDistance); // Update target size
        isAnimating = true;
        timer = 0f;
        hasCompletedAnimation = false;
    }
}