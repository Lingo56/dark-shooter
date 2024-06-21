using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMainHitFlash : MonoBehaviour
{
    private Renderer rend;
    private Material originalMaterial;
    [SerializeField] private Material flashMaterial; // Assign a white material in the inspector

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
    }

    public void Flash(float duration, int flashes)
    {
        StartCoroutine(FlashCoroutine(duration, flashes));
    }

    private IEnumerator FlashCoroutine(float duration, int flashes)
    {
        for (int i = 0; i < flashes; i++)
        {
            rend.material = flashMaterial;
            yield return new WaitForSeconds(duration / (flashes * 2));
            rend.material = originalMaterial;
            yield return new WaitForSeconds(duration / (flashes * 2));
        }
    }
}
