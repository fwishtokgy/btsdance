using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Connects a controller to the music level scrubber, allowing for user input
/// </summary>
public class TimeWand : MonoBehaviour
{
    /// <summary>
    /// The root of the UI for the controller overlay
    /// </summary>
    public GameObject WandControllerLabelRoot;

    protected LinkedListNode<TimeUnitInterval> CurrentUnit;
    /// <summary>
    /// The index of the time unit we want to start with
    /// </summary>
    [SerializeField]
    protected int StartingIntervalIndex;

    /// <summary>
    /// List of configurations containing different time units we can scrub with
    /// </summary>
    [SerializeField]
    private TimeUnitInterval[] EditorSetUnitIntervals;
    protected LinkedList<TimeUnitInterval> unitIntervals;


    /// <summary>
    /// The current time unit we are using should be reflect here
    /// </summary>
    [SerializeField]
    protected DoubleStackLabel CurrentIntervalLabel;
    /// <summary>
    /// A time unit for a previous entry
    /// </summary>
    [SerializeField]
    protected DoubleStackLabel PreviousIntervalLabel;
    /// <summary>
    /// A time unit for the next possible entry
    /// </summary>
    [SerializeField]
    protected DoubleStackLabel NextIntervalLabel;


    [SerializeField]
    protected AudioScrubber MyAudioScrubber;

    // Start is called before the first frame update
    void Start()
    {
        unitIntervals = new LinkedList<TimeUnitInterval>(EditorSetUnitIntervals);
        CurrentUnit = unitIntervals.Find(EditorSetUnitIntervals[StartingIntervalIndex]);
        SetNewInterval(CurrentUnit);
        MyAudioScrubber.Initiate(EditorSetUnitIntervals[0].Beats, CurrentUnit.Value.Beats);
    }

    protected void DecrementTimeUnit()
    {
        if (CurrentUnit.Previous != null)
        {
            SetNewInterval(CurrentUnit.Previous);
            CurrentUnit = CurrentUnit.Previous;
            MyAudioScrubber.SetUnit(CurrentUnit.Value.Beats);
        }
    }
    protected void IncrementTimeUnit()
    {
        if (CurrentUnit.Next != null)
        {
            SetNewInterval(CurrentUnit.Next);
            CurrentUnit = CurrentUnit.Next;
            MyAudioScrubber.SetUnit(CurrentUnit.Value.Beats);
        }
    }
    protected void SetNewInterval(LinkedListNode<TimeUnitInterval> NewNode)
    {
        CurrentIntervalLabel.SetText(NewNode.Value.UnitValueToShow, NewNode.Value.UnitLabel);

        var previous = NewNode.Previous;
        if (previous == null)
        {
            if (PreviousIntervalLabel.gameObject.activeSelf)
            {
                PreviousIntervalLabel.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!PreviousIntervalLabel.gameObject.activeSelf)
            {
                PreviousIntervalLabel.gameObject.SetActive(true);
            }
            PreviousIntervalLabel.SetText(previous.Value.UnitValueToShow, previous.Value.UnitLabel);
        }

        var next = NewNode.Next;
        if (next == null)
        {
            if (NextIntervalLabel.gameObject.activeSelf)
            {
                NextIntervalLabel.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!NextIntervalLabel.gameObject.activeSelf)
            {
                NextIntervalLabel.gameObject.SetActive(true);
            }
            NextIntervalLabel.SetText(next.Value.UnitValueToShow, next.Value.UnitLabel);
        }
    }

    protected void ScrubBack()
    {
        MyAudioScrubber.StepBackwardAndPlay();
    }
    protected void ScrubForward()
    {
        MyAudioScrubber.StepForwardAndPlay();
    }
    protected void PlayCurrent()
    {
        MyAudioScrubber.PlayCurrentPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (!WandControllerLabelRoot.activeSelf && OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            WandControllerLabelRoot.SetActive(true);
        }
        else if (WandControllerLabelRoot.activeSelf && !OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            WandControllerLabelRoot.SetActive(false);
        }

        // INTERVAL SET
        if (Input.GetKeyDown(KeyCode.Keypad8) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))
        {
            IncrementTimeUnit();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))
        {
            DecrementTimeUnit();
        }
        // TIME SCRUB
        else if (Input.GetKeyDown(KeyCode.Keypad6) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))
        {
            ScrubForward();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))
        {
            ScrubBack();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            PlayCurrent();
        }
    }


    [System.Serializable]
    public class TimeUnitInterval
    {
        [SerializeField]
        public string Name;

        /// <summary>
        /// The time unit in beats
        /// </summary>
        [SerializeField]
        public float Beats;

        /// <summary>
        /// A write-friendly display of the value of this timeunit by UnitLabel
        /// </summary>
        [SerializeField]
        public string UnitValueToShow;

        /// <summary>
        /// The name of the unit we should show. Example: Measure or Beat?
        /// </summary>
        [SerializeField]
        public string UnitLabel;
    }
}
