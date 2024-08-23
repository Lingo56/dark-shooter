using UnityEngine;
using UnityEngine.UI.Extensions;

// TODO: Fix marker sticking when firing first shot
namespace UI
{
    public class UIHitMarker : MonoBehaviour
    {
        [Header("Dependancies")]
        [SerializeField] private RectTransform rectTransform; // Reference to the RectTransform of the UI element

        [SerializeField] private RectTransform crosshairCircleRectTransform;
        [SerializeField] private RectTransform comboTrackerRectTransform;
        [SerializeField] private UICircle uiCircle; // Reference to the UICircle component

        [Header("Config")]
        [SerializeField] private float animationDuration = 0.15f; // Duration of the animation in seconds

        [SerializeField] private float fadeDelay = 0.2f; // Delay after animation before resetting to zero
        [SerializeField] private float fadeOutDuration = 0.5f; // Duration of the fade-out effect
        [SerializeField] private float markerDistance = 15f; // How far the hit marker goes
        [SerializeField] private float markerThickness = 2f; // How far the hit marker goes
        [SerializeField] private float markerOutlineThickness = 0.5f; // How far the hit marker goes

        private float timer = 0f; // Timer to track the animation progress
        private bool showMarker = false; // Flag to check if animation is running
        private bool fullBrightness = false; // Flag to check if animation has completed

        private Vector2 initialSize = Vector2.zero; // Initial size (usually starts from 0,0)
        private Vector2 targetSize = new(30f, 30f); // Target size to reach (30x30)

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
        }

        private void Update()
        {
            if (showMarker)
            {
                // Display unfaded hitmarker
                if (fullBrightness)
                {
                    timer += Time.deltaTime;

                    // Apply the new size to the RectTransform
                    rectTransform.sizeDelta = targetSize;

                    // Adjust the thickness of the UICircle
                    uiCircle.Thickness = markerThickness; // Example: set thickness to half the width
                    uiCircle.OutlineThickness = markerOutlineThickness; // Example: set thickness to half the width

                    if (timer >= fadeDelay)
                    {
                        // Start fading out
                        fullBrightness = false;
                        timer = 0f;
                    }
                }
                else // Fade the hitmarker
                {
                    timer += Time.deltaTime;

                    // Calculate lerp factor based on fade out progress
                    float fadeFactor = Mathf.Clamp01(timer / fadeOutDuration);

                    SetMarkerAlpha(Mathf.Lerp(1f, 0f, fadeFactor));

                    // Check if fade out is complete
                    if (fadeFactor >= 1f)
                    {
                        // Reset to initial state
                        rectTransform.sizeDelta = initialSize;
                        uiCircle.Thickness = 0;
                        uiCircle.OutlineThickness = 0;
                        showMarker = false;

                        timer = 0f;
                    }
                }
            }
        }

        private void EnableMarker()
        {
            initialSize = comboTrackerRectTransform.sizeDelta; // Update initial size
            targetSize = comboTrackerRectTransform.sizeDelta + new Vector2(markerDistance, markerDistance); // Update target size

            showMarker = true;
            fullBrightness = true;

            SetMarkerAlpha(1f);

            timer = 0f;
        }

        private void SetMarkerAlpha(float alpha)
        {
            Color color = uiCircle.color;
            color.a = alpha;
            uiCircle.color = color;
        }
    }
}