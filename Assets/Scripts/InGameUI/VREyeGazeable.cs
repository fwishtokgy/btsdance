using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class VREyeGazeable : MonoBehaviour
{
    /// <summary>
    /// The UnityEvent that will be sent when this UI item is pressed down upon.
    /// </summary>
    [Serializable]
    public class GazeEnterEvent : UnityEvent { }
    /// <summary>
    /// An instance of the custom UnityEvent for handling initial touch downs.
    /// </summary>
    [FormerlySerializedAs("onGazeEnter")]
    [SerializeField]
    protected GazeEnterEvent onGazeEnter = new GazeEnterEvent();
    /// <summary>
    ///  Public accessor to the event that fires on an initial touch down.
    /// </summary>
    public virtual GazeEnterEvent OnGazeEnter
    {
        get
        {
            return onGazeEnter;
        }
        set
        {
            onGazeEnter = value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Gaze")
        {
            Debug.Log("Invoke Enter");
            onGazeEnter.Invoke();
        }
    }

    /// <summary>
    /// The UnityEvent that will be sent when this UI item is pressed down upon.
    /// </summary>
    [Serializable]
    public class GazeExitEvent : UnityEvent { }
    /// <summary>
    /// An instance of the custom UnityEvent for handling initial touch downs.
    /// </summary>
    [FormerlySerializedAs("onGazeExit")]
    [SerializeField]
    protected GazeExitEvent onGazeExit = new GazeExitEvent();
    /// <summary>
    ///  Public accessor to the event that fires on an initial touch down.
    /// </summary>
    public virtual GazeExitEvent OnGazeExit
    {
        get
        {
            return onGazeExit;
        }
        set
        {
            onGazeExit = value;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Gaze")
        {
            Debug.Log("InvokeExit");
            onGazeExit.Invoke();
        }
    }
}
