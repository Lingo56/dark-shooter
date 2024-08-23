using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Imports.WorldSpaceTransitions.common
{
    public class MyCanvasSetting : MonoBehaviour {

        // Use this for initialization
        void Start ()
        {
#if UNITY_ANDROID      
        mySetting();
#endif
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
#if UNITY_ANDROID      
        mySetting();
#endif
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void mySetting()
        {
            CanvasScaler[] canv = FindObjectsOfType(typeof(CanvasScaler)) as CanvasScaler[];
            foreach (CanvasScaler cns in canv) cns.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

    }
}
