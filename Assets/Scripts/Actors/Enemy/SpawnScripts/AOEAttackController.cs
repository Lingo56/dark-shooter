using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackController : MonoBehaviour
{
    private Animator animator;
    private Material material;

    void Start()
    {
        animator = GetComponent<Animator>();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.sharedMaterial;
        }
        else
        {
            Debug.LogError("No Renderer found on the current GameObject.");
        }
    }

    void Update()
    {
        if (material == null) return;

        // Check the Animator's current state and synchronize with the script logic
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("AOEFadeIn"))
        {
            HandleFadeIn();
        }
        else if (stateInfo.IsName("AOEIdle"))
        {
            HandleIdle();
        }
        else if (stateInfo.IsName("AOEFadeOut"))
        {
            HandleFadeOut();
        } else if (stateInfo.IsName("AOEComplete"))
        {
            HandleComplete();
        }
    }

    void HandleFadeIn()
    {
        // Add any additional logic needed during the FadeIn state
    }

    void HandleIdle()
    {
        // Add any additional logic needed during the Idle state
    }

    void HandleFadeOut()
    {
        // Add any additional logic needed during the FadeOut state
    }
    
    void HandleComplete()
    {
        Destroy(gameObject);
    }
}
