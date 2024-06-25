using UnityEngine;

public class UIDeathMarker : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform; // Reference to the RectTransform of the UI element

    private Vector2 initialSize = Vector2.zero; // Initial size (usually starts from 0,0)
    private Vector2 targetSize = new Vector2(30f, 30f); // Target size to reach (30x30)

    [SerializeField] private float animationDuration = 0.15f; // Duration of the animation in seconds
    [SerializeField] private float postAnimationDelay = 0.2f; // Delay after animation before resetting to zero
    private float timer = 0f; // Timer to track the animation progress
    private bool isAnimating = false; // Flag to check if animation is running
    private bool hasCompletedAnimation = false; // Flag to check if animation has completed

    private void OnEnable()
    {
        GameEvents.OnEnemyDeath += EnableMarker; // Subscribe to the event
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDeath -= EnableMarker; // Unsubscribe from the event
    }

    private void Start()
    {
        // Initialize the RectTransform if not assigned in the Inspector
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        initialSize = rectTransform.sizeDelta; // Store initial size
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
                    rectTransform.sizeDelta = Vector2.zero; // Reset to zero after delay
                    isAnimating = false;
                    hasCompletedAnimation = false;
                    timer = 0f;
                }
            }
        }
    }

    private void EnableMarker()
    {
        isAnimating = true;
        timer = 0f;
        hasCompletedAnimation = false;
    }
}