using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds behavior for a star wand that allows the user to shift between modes
/// </summary>
public class StarWandEditor : MonoBehaviour
{
    /// <summary>
    /// The prefab we want to spawn and manipulate
    /// </summary>
    public GameObject StarPrefab;

    /// <summary>
    /// The point from which stars will spawn, in relation to the input object like a hand or wand
    /// </summary>
    public Transform SpawnPoint;

    /// <summary>
    /// The reference point of the spawned stars; their parent
    /// </summary>
    public Transform StarsRoot;

    /// <summary>
    /// All possible wandmodes
    /// </summary>
    public List<WandMode> WandModes;

    /// <summary>
    /// The controller this wand behavior is attached to
    /// </summary>
    public OVRInput.Controller Controller;

    /// <summary>
    /// Index of the current wand mode
    /// </summary>
    protected int currentWandMode;

    /// <summary>
    /// The index of the wand mode we open up the level editor with
    /// </summary>
    public int StartingWandModeIndex;

    /// <summary>
    /// Original scale of a non-current UI element
    /// </summary>
    protected float originalScale;
    /// <summary>
    /// Scale of the current UI element we are focusing on
    /// </summary>
    protected float largerScale;

    /// <summary>
    /// The root that holds all the UI overlay elements for the buttons
    /// </summary>
    public GameObject WandControllerLabelRoot;

    /// <summary>
    /// Holds a star if it is overlapping with this wand
    /// </summary>
    protected StarBitEditable CurrentTarget;

    public AudioScrubber Scrubber;

    /// <summary>
    /// Is the controller currently engaged with a star or is it available?
    /// </summary>
    public bool IsCurrentlyEngaged
    {
        get
        {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller);
        }
    }

    /// <summary>
    /// The current star the starwand is focusing on. Could be null
    /// </summary>
    public StarBitEditable CurrentFocus
    {
        get
        {
            return CurrentTarget;
        }
    }

    /// <summary>
    /// The current edit mode the starwand is engaged in
    /// </summary>
    public StarEdit.Mode CurrentMode
    {
        get
        {
            return WandModes[currentWandMode].Mode;
        }
    }

    /// <summary>
    /// Sets the target to a particular star
    /// </summary>
    /// <param name="starbit"></param>
    public void SetTarget(StarBitEditable starbit)
    {
        CurrentTarget = starbit;
    }
    /// <summary>
    /// Releases any star the starwand might have been focusing on
    /// </summary>
    /// <param name="starbit"></param>
    public void ReleaseTarget(StarBitEditable starbit)
    {
        if (CurrentTarget == starbit)
        {
            CurrentTarget = null;
        }    
    }

    // Start is called before the first frame update
    void Start()
    {
        currentWandMode = StartingWandModeIndex;

        originalScale = WandModes[currentWandMode].IndicatorUI.localScale.x;
        largerScale = originalScale * 1.5f;

        SetNewStarMode(WandModes[currentWandMode]);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsCurrentlyEngaged)
        {
            if (!WandControllerLabelRoot.activeSelf && OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, Controller))
            {
                WandControllerLabelRoot.SetActive(true);
            }
            else if (WandControllerLabelRoot.activeSelf && !OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, Controller))
            {
                WandControllerLabelRoot.SetActive(false);
            }

            if (WandControllerLabelRoot.activeSelf)
            {
                for (var wandIndex = 0; wandIndex < WandModes.Count; wandIndex++)
                {
                    if (wandIndex != currentWandMode)
                    {
                        if (OVRInput.Get(WandModes[wandIndex].ThumbstickInput, Controller))
                        {
                            CloseOldWandMode(WandModes[currentWandMode]);
                            SetNewStarMode(WandModes[wandIndex]);

                            currentWandMode = wandIndex;
                            wandIndex = WandModes.Count;
                        }
                    }
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.Alpha1) || CurrentTarget == null && currentWandMode == 1 && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, Controller))
        {
            AddNewStar();
        }
        else if (CurrentTarget != null)
        {
            
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, Controller))
            {
                Debug.Log("GET DOWN");
                CurrentTarget.ExecuteTrigger();
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, Controller))
            {
                Debug.Log("Get Up");
                CurrentTarget.ReleaseTrigger();
            }
        }

        if (IsCurrentlyEngaged && WandControllerLabelRoot.activeSelf)
        {
            WandControllerLabelRoot.SetActive(false);
        }
    }
    /// <summary>
    /// Adds a new star and then registers it to the music scrubber in the current timesample
    /// </summary>
    protected void AddNewStar()
    {
        // First, check if a star already exists on this timesample
        if (Scrubber.HasEmptySpot(Controller))
        {
            // Create a star
            var newStar = GameObject.Instantiate(StarPrefab);
            newStar.transform.position = SpawnPoint.position;
            newStar.transform.parent = StarsRoot;
            newStar.GetComponentInChildren<StarBitEditable>().Initiate(Scrubber.CurrentSampleIndex, this);

            // Register Star with Scrubber
            Scrubber.RegisterStar(Controller, newStar.transform.localPosition);
        }
    }
    /// <summary>
    /// Tells the music scrubber that we have moved a star in the current timesample
    /// </summary>
    /// <param name="star"></param>
    public void AlertToMove(StarBitEditable star)
    {
        Scrubber.UpdateStar(Controller, star.transform.parent.transform.localPosition);
    }
    /// <summary>
    /// Tells the music scrubber that we have deleted a star in the current timesample
    /// </summary>
    /// <param name="star"></param>
    public void AlertToDeath(StarBitEditable star)
    {
        Scrubber.UnregisterStar(Controller);
    }
    /// <summary>
    /// Cleanly closes out the UI of the last wand mode
    /// </summary>
    /// <param name="oldMode">The mode we want to clean up and hide</param>
    protected void CloseOldWandMode(WandMode oldMode)
    {
        oldMode.IndicatorUI.localScale = new Vector3(originalScale, originalScale, originalScale);
        oldMode.ModePiece.SetActive(false);
    }
    /// <summary>
    /// Sets the UI of a new wand mode to true
    /// </summary>
    /// <param name="newMode">The mode whose UI we want to show</param>
    protected void SetNewStarMode(WandMode newMode)
    {
        newMode.IndicatorUI.localScale = new Vector3(largerScale, largerScale, largerScale);
        newMode.ModePiece.SetActive(true);
    }

    [System.Serializable]
    public class WandMode
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public StarEdit.Mode Mode;

        [SerializeField]
        public Transform IndicatorUI;

        [SerializeField]
        public GameObject ModePiece;

        [SerializeField]
        public OVRInput.Button ThumbstickInput;
    }
}

public class StarEdit
{
    public enum Mode
    {
        NONE,
        ADD,
        MOVE,
        DELETE
    }
}