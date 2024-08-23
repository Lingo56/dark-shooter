﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Imports.WorldSpaceTransitions.common
{
    public class SceneSwitch : MonoBehaviour
    {
        private static SceneSwitch switchInstance;
        public Dropdown sceneDropdown;

        public void SwitchScene(int val)
        {
            if (val == SceneManager.GetActiveScene().buildIndex) return; //toggle buttons change twice
            SceneManager.LoadSceneAsync(val);
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (switchInstance == null)
            {
                switchInstance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetKey("escape"))
            {
                //Debug.Log("escape");
                Application.Quit();
            }
        }
    }
}