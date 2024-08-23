using System.Collections;
using UnityEngine;

namespace Actors
{
    public class EnemyMainHitFlash : MonoBehaviour
    {
        private Renderer rend;
        private Material originalMaterial;
        [SerializeField] private Material flashMaterial; // Assign a white material in the inspector

        // Start is called before the first frame update
        private void Start()
        {
            rend = GetComponent<Renderer>();
            originalMaterial = rend.material;
        }

        public void Flash(float duration)
        {
            rend.material = flashMaterial;
            StartCoroutine(RevertMaterialAfterDelay(duration));
        }

        private IEnumerator RevertMaterialAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            rend.material = originalMaterial;
        }
    }
}