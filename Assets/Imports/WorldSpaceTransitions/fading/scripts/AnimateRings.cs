using UnityEngine;

namespace Imports.WorldSpaceTransitions.fading.scripts
{
    [ExecuteInEditMode]
    public class AnimateRings : MonoBehaviour
    {
        public float ringsOffset;

        void Start()
        {
           
        }

        void OnEnable()
        {
            
        }

        // Update is called once per frame
        void OnValidate()
        {
            Debug.Log(ringsOffset.ToString());
            Shader.SetGlobalFloat("_ringOffset", ringsOffset);
        }

        void Update()
        {
            Debug.Log(ringsOffset.ToString());
            Shader.SetGlobalFloat("_ringOffset", ringsOffset);
        }
    }
}