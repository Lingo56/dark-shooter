using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackController : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 2.0f; // Duration for fading in and out
    
    [Header("Idle Animation")]
    [SerializeField] private float idleDuration = 2.0f; // Duration to stay idle
    [SerializeField] private float idleAnimationSpeed = 1.0f; // Speed of the oscillation (if used)
    [SerializeField] private float idleOscillationAmplitude = 0.1f; // Amplitude of the oscillation

    private Material material;
    private float stateTime; // Tracks time within the current state
    private float edgeFadeValue; // Tracks the current value of _EdgeFadeDistance
    private float startValue; // Tracks the starting value for each transition

    private enum State { FadeIn, Idle, FadeOut }
    private State currentState = State.FadeIn;

    void Start()
    {
        // Get the material of the current object
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.sharedMaterial; // Use sharedMaterial to avoid material instantiation
        }
        else
        {
            Debug.LogError("No Renderer found on the current GameObject.");
        }

        edgeFadeValue = 0f;
        stateTime = 0f;
        currentState = State.FadeIn;
        startValue = edgeFadeValue;
    }

    void Update()
    {
        if (material == null) return;

        stateTime += Time.deltaTime;

        switch (currentState)
        {
            case State.FadeIn:
                HandleFadeIn();
                break;
            case State.Idle:
                HandleIdle();
                break;
            case State.FadeOut:
                HandleFadeOut();
                break;
        }

        material.SetFloat("_EdgeFadeDistance", edgeFadeValue);
    }
    
    void HandleFadeIn()
    {
        float t = stateTime / transitionDuration;
        edgeFadeValue = Mathf.Lerp(startValue, 1.0f, t);

        if (t >= 1.0f)
        {
            stateTime = 0f;
            startValue = edgeFadeValue; // Set start value for Idle
            currentState = State.Idle;
        }
    }

    void HandleIdle()
    {
        // Oscillate around the startValue from FadeIn
        float oscillation = Mathf.Sin(stateTime * idleAnimationSpeed) * idleOscillationAmplitude;
        edgeFadeValue = startValue + oscillation;

        if (stateTime >= idleDuration)
        {
            stateTime = 0f;
            startValue = edgeFadeValue; // Set start value for FadeOut
            currentState = State.FadeOut;
        }
    }

    void HandleFadeOut()
    {
        float t = stateTime / transitionDuration;
        edgeFadeValue = Mathf.Lerp(startValue, 0f, t);

        if (t >= 1.0f)
        {
            stateTime = 0f;
            startValue = edgeFadeValue; // Reset start value for next FadeIn
            currentState = State.FadeIn;
        }
    }
}
