using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

//Expand upon this class later. It is very helpful
public class TriggerContainer : MonoBehaviour
{
    public string[] Names;
    public string[] Tags;

    protected bool IsValidTriggerer(Collider other)
    {
        if (Names.Length > 0)
        {
            foreach (var name in Names)
            {
                if (other.gameObject.name == name)
                {
                    return true;
                }
            }
        }
        if (Tags.Length > 0)
        {
            foreach (var tag in Tags)
            {
                if (other.tag == tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// The UnityEvent that will be sent when this UI item is pressed down upon.
    /// </summary>
    [Serializable]
    public class TriggerEnterEvent : UnityEvent { }
    /// <summary>
    /// An instance of the custom UnityEvent for handling initial touch downs.
    /// </summary>
    [FormerlySerializedAs("onTriggerEnter")]
    [SerializeField]
    protected TriggerEnterEvent onTriggerEnter = new TriggerEnterEvent();
    /// <summary>
    ///  Public accessor to the event that fires on an initial touch down.
    /// </summary>
    public virtual TriggerEnterEvent OnTriggerEnterEvent
    {
        get
        {
            return onTriggerEnter;
        }
        set
        {
            onTriggerEnter = value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTriggerer(other))
        {
            onTriggerEnter.Invoke();
        }
    }

    /// <summary>
    /// The UnityEvent that will be sent when this UI item is pressed down upon.
    /// </summary>
    [Serializable]
    public class TriggerExitEvent : UnityEvent { }
    /// <summary>
    /// An instance of the custom UnityEvent for handling initial touch downs.
    /// </summary>
    [FormerlySerializedAs("onTriggerExit")]
    [SerializeField]
    protected TriggerExitEvent onTriggerExit = new TriggerExitEvent();
    /// <summary>
    ///  Public accessor to the event that fires on an initial touch down.
    /// </summary>
    public virtual TriggerExitEvent OnTriggerExitEvent
    {
        get
        {
            return onTriggerExit;
        }
        set
        {
            onTriggerExit = value;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTriggerer(other))
        {
            onTriggerExit.Invoke();
        }
    }

}
