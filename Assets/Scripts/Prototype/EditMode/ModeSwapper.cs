using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helps swap between different input configurations for a controller
/// </summary>
public class ModeSwapper : MonoBehaviour
{
    protected int currentModeIndex;

    /// <summary>
    /// All different controller input configurations possible
    /// </summary>
    public List<ControllerModeSet> ControllerModes;

    /// <summary>
    /// The button that will allow user to cycle through the configurations
    /// </summary>
    public OVRInput.Button TriggerButton;

    /// <summary>
    /// The controller to target
    /// </summary>
    public OVRInput.Controller Controller;

    private void Awake()
    {
        currentModeIndex = 0;

        for (var modeIndex = 0; modeIndex < ControllerModes.Count; modeIndex++)
        {
            SetMode(modeIndex, false);
        }

        SetMode(currentModeIndex, true);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) || OVRInput.GetDown(TriggerButton, Controller))
        {
            SetMode(currentModeIndex, false);

            currentModeIndex = (currentModeIndex + 1) % ControllerModes.Count;

            SetMode(currentModeIndex, true);
        }
    }

    protected void SetMode(int index, bool value)
    {
        ControllerModes[index].MainScript.enabled = value;
        ControllerModes[index].RootUIObject.SetActive(value);
    }

    [System.Serializable]
    public class ControllerModeSet
    {
        /// <summary>
        /// The main behavior script that holds behavior for button input
        /// </summary>
        [SerializeField]
        public MonoBehaviour MainScript;

        /// <summary>
        /// The root gameobject that holds all of the UI and scripts
        /// </summary>
        [SerializeField]
        public GameObject RootUIObject;
    }
}
