using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Helps control the stepping through of the audio
/// </summary>
public class AudioScrubber : MonoBehaviour
{
    [SerializeField]
    protected AudioSource audioSource;
    [SerializeField]
    protected AudioClip audioClip;
    [SerializeField]
    protected int BeatsPerMinute;

    /// <summary>
    /// The max duration in seconds we will play a sample
    /// </summary>
    [SerializeField]
    protected float MaxPreviewDuration;
    /// <summary>
    /// The min duration in seconds we will play a sample
    /// </summary>
    [SerializeField]
    protected float MinPreviewDuration;
    /// <summary>
    /// The current duration in seconds that we will play the current sample
    /// </summary>
    protected float currentPreviewDuration;

    protected float TimeSamplesPerBeat;

    /// <summary>
    /// The smallest unit in TimeSamples we can step through the piece
    /// </summary>
    protected int SmallestUnit;
    /// <summary>
    /// The total number of samples per SmallestUnit that the song is comprised of
    /// </summary>
    protected int MaxSamples;
    /// <summary>
    /// The current position in the song by number of SmallestUnit values
    /// </summary>
    protected int CurrentSamplePtr;
    /// <summary>
    /// The current number of TimeSamples the current step unit is comprised of
    /// </summary>
    protected int CurrentUnitInTimeSamples;
    /// <summary>
    /// The last TimeSample position that we are currently cleaning up
    /// </summary>
    protected int SampleBeingCleanedUp;

    /// <summary>
    /// The current number of samples per multiples of the SmallestUnit
    /// </summary>
    public int CurrentSampleIndex
    {
        get
        {
            return CurrentSamplePtr;
        }
    }

    private bool IsInitiated;

    public Text DebugTimeSampleText;

    /// <summary>
    /// How quickly we will lerp stars in and out
    /// </summary>
    public float StarGrowthSpeed;


    /// <summary>
    /// The root object to parent all star instances to
    /// </summary>
    public Transform StarsRoot;

    public StarWandEditor StarWandRight;//this is bad
    public StarWandEditor StarWandLeft;//this is bad
    public StarIO IOHandler;

    /// <summary>
    /// A number to convert TimeSamples to Seconds
    /// </summary>
    protected float TimeSampleToSecondsMultiplier;

    /// <summary>
    /// Reference to a reference video
    /// </summary>
    public VideoPlayer videoPlayer;

    /// <summary>
    /// Initiates the audio scrubber
    /// </summary>
    /// <param name="smallestBeat">The smallest beat we should be able to scrub by.</param>
    /// <param name="startingBeatUnit">The starting number of beats we should be able to scrub by</param>
    public void Initiate(float smallestBeat, float startingBeatUnit)
    {
        IsInitiated = true;

        TimeSamplesPerBeat = (60f / BeatsPerMinute) * audioClip.frequency;

        SmallestUnit = Mathf.RoundToInt(TimeSamplesPerBeat * smallestBeat);
        SetUnit(smallestBeat);
        
        SetUnit(startingBeatUnit);
        CurrentSamplePtr = 0;

        MaxSamples = (audioClip.samples - (audioClip.samples % SmallestUnit)) / SmallestUnit;

        videoPlayer.Prepare();
        videoPlayer.seekCompleted += LaunchSample;

        TimeSampleToSecondsMultiplier = (1 / TimeSamplesPerBeat) * (60f / BeatsPerMinute);

        DebugTimeSampleText.text = "TimeSample: " + CurrentSamplePtr;
    }

    private void OnDestroy()
    {
        videoPlayer.seekCompleted -= LaunchSample;
    }

    /// <summary>
    /// Returns if an empty spot is available.
    /// </summary>
    /// <param name="controller">The hand whose list we want to check</param>
    public bool HasEmptySpot(OVRInput.Controller controller)
    {
        switch (controller)
        {
            case OVRInput.Controller.LTouch:
                return !IOHandler.LeftStars.ContainsKey(CurrentSamplePtr);
            case OVRInput.Controller.RTouch:
                return !IOHandler.RightStars.ContainsKey(CurrentSamplePtr);
        }
        return false;
    }

    /// <summary>
    /// Load in stars from the XML
    /// </summary>
    /// <param name="xmlhandler"></param>
    public void LoadInStars(StarIO xmlhandler)
    {
        if (!xmlhandler.IsProcessing)
        {
            xmlhandler.LoadToDictionary();
        }
    }
    /// <summary>
    /// Save current star data to the XML
    /// </summary>
    /// <param name="xmlhandler"></param>
    public void SaveStars(StarIO xmlhandler)
    {
        if (!xmlhandler.IsProcessing)
        {
            xmlhandler.Save();
        }
    }

    /// <summary>
    /// Registers a star to a hand's list
    /// </summary>
    /// <param name="controller">The controller hand the star belongs to</param>
    /// <param name="starposition">The local position of the star</param>
    public void RegisterStar(OVRInput.Controller controller, Vector3 starposition)
    {
        switch (controller)
        {
            case OVRInput.Controller.LTouch:
                IOHandler.LeftStars.Add(CurrentSamplePtr, starposition);
                break;
            case OVRInput.Controller.RTouch:
                IOHandler.RightStars.Add(CurrentSamplePtr, starposition);
                break;
        }
        if (IOHandler.MaxTimeSample < CurrentSamplePtr)
        {
            IOHandler.MaxTimeSample = CurrentSamplePtr;
        }
    }
    /// <summary>
    /// Updates the position of a hand's star
    /// </summary>
    /// <param name="controller">The controller hand the star belongs to</param>
    /// <param name="starposition">The new local position of the star</param>
    public void UpdateStar(OVRInput.Controller controller, Vector3 starposition)
    {
        switch (controller)
        {
            case OVRInput.Controller.LTouch:
                if (IOHandler.LeftStars.ContainsKey(CurrentSamplePtr))
                {
                    IOHandler.LeftStars[CurrentSamplePtr] = starposition;
                }
                break;
            case OVRInput.Controller.RTouch:
                if (IOHandler.RightStars.ContainsKey(CurrentSamplePtr))
                {
                    IOHandler.RightStars[CurrentSamplePtr] = starposition;
                }
                break;
        }
    }
    /// <summary>
    /// Deregisters a star under a hand's list for the current timesample
    /// </summary>
    /// <param name="controller">The hand controller the star belongs to</param>
    public void UnregisterStar(OVRInput.Controller controller)
    {
        switch (controller)
        {
            case OVRInput.Controller.LTouch:
                IOHandler.LeftStars.Remove(CurrentSamplePtr);
                break;
            case OVRInput.Controller.RTouch:
                IOHandler.RightStars.Remove(CurrentSamplePtr);
                break;
        }
    }

    /// <summary>
    /// Purges out any stars that are not part of this current TimeSample, and then adds in any stars that are missing for the current TimeSample
    /// </summary>
    public void RefreshStars()
    {
        StartCoroutine(CleanRemoval());
    }
    protected IEnumerator CleanRemoval()
    {
        var IndexToClear = SampleBeingCleanedUp;
        var BodiesLeft = 100;
        while (BodiesLeft > 0)
        {
            var timeScale = Time.deltaTime * -StarGrowthSpeed;
            BodiesLeft = 0;
            foreach (var starScript in StarsRoot.GetComponentsInChildren<StarBitEditable>())
            {
                if (starScript.TimeSampleIndex == IndexToClear)
                {
                    BodiesLeft++;
                    starScript.transform.parent.transform.localScale += new Vector3(timeScale, timeScale, timeScale);
                    if (starScript.transform.parent.transform.localScale.x <= 0f)
                    {
                        GameObject.Destroy(starScript.transform.parent.gameObject);
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(CleanAdd());
    }
    protected IEnumerator CleanAdd()
    {
        var IndexToAdd = CurrentSamplePtr;
        var AddStars = IOHandler.LeftStars.ContainsKey(CurrentSamplePtr) || IOHandler.RightStars.ContainsKey(CurrentSamplePtr);

        if (AddStars)
        {
            if (IOHandler.LeftStars.ContainsKey(CurrentSamplePtr))
            {
                AddAnExistingStar(IOHandler.LeftStars[CurrentSamplePtr], StarWandLeft);
            }
            if (IOHandler.RightStars.ContainsKey(CurrentSamplePtr))
            {
                AddAnExistingStar(IOHandler.RightStars[CurrentSamplePtr], StarWandRight);
            }
            var currentScale = 0f;
            while (currentScale < 1f)
            {
                foreach (var starScript in StarsRoot.GetComponentsInChildren<StarBitEditable>())
                {
                    if (starScript.TimeSampleIndex == IndexToAdd)
                    {
                        starScript.transform.parent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                    }
                }
                currentScale = currentScale + (Time.deltaTime * StarGrowthSpeed);
                yield return new WaitForEndOfFrame();
            }
            foreach (var starScript in StarsRoot.GetComponentsInChildren<StarBitEditable>())
            {
                if (starScript.TimeSampleIndex == IndexToAdd)
                {
                    starScript.transform.parent.transform.localScale = Vector3.one;
                }
            }
        }
    }
    /// <summary>
    /// Internal helper class to instantiate a visual star, used by RefreshStars
    /// </summary>
    /// <param name="spawnPosition">The local spawn position</param>
    /// <param name="StarWand">The starwand of the hand controller this star belongs to</param>
    /// <returns>An editor-only script representing the star's data</returns>
    protected StarBitEditable AddAnExistingStar(Vector3 spawnPosition, StarWandEditor StarWand)
    {
        var newStar = GameObject.Instantiate(StarWand.StarPrefab);
        newStar.transform.parent = StarsRoot;
        newStar.transform.localPosition = spawnPosition;
        var datascript = newStar.GetComponentInChildren<StarBitEditable>();
        datascript.Initiate(CurrentSamplePtr, StarWand);
        newStar.transform.localScale = Vector3.zero;
        return datascript;
    }
    /// <summary>
    /// Sets the current unit by which the scrubber will step through the music
    /// </summary>
    /// <param name="UnitInBeats">The unit in beats that the scrubber should scrub by</param>
    public void SetUnit(float UnitInBeats)
    {
        if (!IsInitiated) return;

        var rawUnit = TimeSamplesPerBeat * UnitInBeats;
        CurrentUnitInTimeSamples = Mathf.FloorToInt(rawUnit / SmallestUnit) * SmallestUnit;
        
        currentPreviewDuration = (60f / BeatsPerMinute) * UnitInBeats;
        if (currentPreviewDuration < MinPreviewDuration)
        {
            currentPreviewDuration = MinPreviewDuration;
        }
        else if (currentPreviewDuration > MaxPreviewDuration)
        {
            currentPreviewDuration = MaxPreviewDuration;
        }
    }

    /// <summary>
    /// Steps forward by the current unit and plays a sample and displays stars
    /// </summary>
    public void StepForwardAndPlay()
    {
        if (!IsInitiated) return;

        var oldSamplePtr = CurrentSamplePtr;

        var leftover = (CurrentSamplePtr * SmallestUnit) % CurrentUnitInTimeSamples;
        if (leftover != 0)
        {
            CurrentSamplePtr = ((CurrentSamplePtr * SmallestUnit) + leftover) / SmallestUnit;
        }
        else
        {
            CurrentSamplePtr = ((CurrentSamplePtr * SmallestUnit) + CurrentUnitInTimeSamples) / SmallestUnit;
            if (CurrentSamplePtr > MaxSamples)
            {
                CurrentSamplePtr = MaxSamples;
            }
        }
        PlayBit();

        if (oldSamplePtr != CurrentSamplePtr)
        {
            SampleBeingCleanedUp = oldSamplePtr;
            RefreshStars();
        }
    }
    /// <summary>
    /// Steps backward by the current unit and plays a sample and displays stars
    /// </summary>
    public void StepBackwardAndPlay()
    {
        if (!IsInitiated) return;

        var oldSamplePtr = CurrentSamplePtr;

        var leftover = (CurrentSamplePtr * SmallestUnit) % CurrentUnitInTimeSamples;
        if (leftover != 0)
        {
            CurrentSamplePtr = ((CurrentSamplePtr * SmallestUnit) - leftover) / SmallestUnit;
        }
        else
        {
            CurrentSamplePtr = ((CurrentSamplePtr * SmallestUnit) - CurrentUnitInTimeSamples) / SmallestUnit;
            if (CurrentSamplePtr < 0)
            {
                CurrentSamplePtr = 0;
            }
        }
        PlayBit();

        if (oldSamplePtr != CurrentSamplePtr)
        {
            SampleBeingCleanedUp = oldSamplePtr;
            RefreshStars();
        }
    }
    /// <summary>
    /// Plays a sample and displays stars at the current point in the music
    /// </summary>
    public void PlayCurrentPoint()
    {
        if (!IsInitiated) return;
        PlayBit();
    }

    /// <summary>
    /// Works out the current place in the music and video, and then plays it
    /// </summary>
    protected void PlayBit()
    {
        StopCoroutine(PlaySample());
        DebugTimeSampleText.text = "TimeSample: " + CurrentSamplePtr;

        audioSource.timeSamples = CurrentSamplePtr * SmallestUnit;
        if (audioSource.timeSamples > (audioClip.samples - SmallestUnit))
        {
            audioSource.timeSamples = audioClip.samples - SmallestUnit;
        }

        videoPlayer.time = TimeSampleToSecondsMultiplier * audioSource.timeSamples;
    }
    IEnumerator PlaySample()
    {
        videoPlayer.Play();
        audioSource.Play();

        yield return new WaitForSeconds(currentPreviewDuration);
        audioSource.Stop();
        videoPlayer.Pause();
    }
    /// <summary>
    /// Initiates the playing of the video and audio when both are ready and set
    /// </summary>
    /// <param name="source"></param>
    protected void LaunchSample(VideoPlayer source)
    {
        StartCoroutine(PlaySample());
    }

    void Awake()
    {
        IsInitiated = false;
    }
}
