using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the reading in of stars to the beat of the level music
/// </summary>
public class ProtoDanceJuggler : MonoBehaviour
{
    /// <summary>
    /// Prefab holding a starBlock and starShootIn script, used to instantiate bubbled stars
    /// </summary>
    public GameObject DanceBitPrefab;

    /// <summary>
    /// The parent of any stars we instantiate
    /// </summary>
    public StarBlockHandler StarRoot;

    /// <summary>
    /// The duration in seconds from a star's appearance to the beginning of the poppable bubble's appearance
    /// </summary>
    public float warpInTime;
    /// <summary>
    /// The duration in seconds from the end of a poppable bubble's closure and the star's disappearance
    /// </summary>
    public float warpOutTime;
    /// <summary>
    /// The duration in beats 
    /// </summary>
    public float numberOfPredecessingBeats;
    /// <summary>
    /// The duration in beats of a bubble's existence
    /// </summary>
    public float activeTime;

    /// <summary>
    /// Reference to the audiosource containing the level music
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// The Beats per Minute for the level music
    /// </summary>
    public int BeatsPerMinute;

    /// <summary>
    /// The base value of the level music's time signature
    /// </summary>
    public int TimeSignature;

    /// <summary>
    /// The number of timesamples per beat
    /// </summary>
    protected float samplesPerBeat;

    /// <summary>
    /// How long to pause initiation of the level
    /// </summary>
    protected float SongLoadPause;

    /// <summary>
    /// Whether the juggler is playing through the stars
    /// </summary>
    protected bool isRunning;

    /// <summary>
    /// The current time that has passed since the level music began
    /// </summary>
    protected double trueTime;

    /// <summary>
    /// The beats before a star spawns, translated into seconds
    /// </summary>
    protected float numberOfPredecessingBeatsInSeconds;

    /// <summary>
    /// The number of seconds to wait before starting to play the level music
    /// </summary>
    protected float predecessingTime;

    /// <summary>
    /// Reference to the scene transitioner
    /// </summary>
    public MySceneTransitioner sceneTransitioner;

    /// <summary>
    /// Reference to the IO Handler
    /// </summary>
    public StarIO IOHandler;

    /// <summary>
    /// Materials for stars for the left hand
    /// </summary>
    public StarMaterialData LeftStarMaterials;
    /// <summary>
    /// Materials for stars for the right hand
    /// </summary>
    public StarMaterialData RightStarMaterials;

    /// <summary>
    /// Reference to the score handler
    /// </summary>
    public ScoreHandler scoreHandler;

    /// <summary>
    /// Reference to the GazePoint, to be used for hypothetical shooting star spawning
    /// </summary>
    public Transform GazePoint;
    /// <summary>
    /// Reference to a parent for all star clone holders, for player reference
    /// </summary>
    public Transform MiniClonePlatform;


    void Start()
    {
        isRunning = false;

        SongLoadPause = 0f;
        samplesPerBeat = (BeatsPerMinute / 60f) * audioSource.clip.frequency;

        var secondsPerBeat = (BeatsPerMinute / 60f);
        numberOfPredecessingBeatsInSeconds = numberOfPredecessingBeats * secondsPerBeat;
        predecessingTime = warpInTime + numberOfPredecessingBeatsInSeconds;

        ScoreHandler.OnDeath += KillLevel;

        StartCoroutine(InitiateSongLoad());
    }
    private void OnDestroy()
    {
        ScoreHandler.OnDeath -= KillLevel;
    }

    /// <summary>
    /// Initiates the level based on the stock default wait time
    /// </summary>
    protected IEnumerator InitiateSongLoad()
    {
        XMLLoad();
        var timePassed = Time.time;
        yield return new WaitUntil(isReadyForPlay);
        timePassed = Time.time - timePassed;
        if (timePassed < SongLoadPause)
        {
            yield return new WaitForSeconds(SongLoadPause - timePassed);
        }
        audioSource.Play();
        trueTime = 0;
        isRunning = true;

        scoreHandler.SetMaximumInputData(IOHandler.StarStream.Count);
    }
    /// <summary>
    /// Returns whether the star data has all completed loading for the level
    /// </summary>
    /// <returns></returns>
    protected bool isReadyForPlay()
    {
        return IOHandler.IsProcessing == false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            RunThroughTable();
            if (HasSongEnded() || !audioSource.isPlaying)
            {
                isRunning = false;
                CloseLevel();
            }
        }
    }
    /// <summary>
    /// Takes the current playtime and spawns the necessary stars
    /// </summary>
    protected void RunThroughTable()
    {
        trueTime = trueTime + Time.deltaTime;
        while (IOHandler.StarStream.Count > 0 && IOHandler.StarStream.Peek().LoadTimeStamp <= trueTime)
        {
            GenerateAStar(IOHandler.StarStream.Dequeue());
        }
    }
    /// <summary>
    /// Returns whether the song has ended yet or not
    /// </summary>
    protected bool HasSongEnded()
    {
        return StarRoot.transform.childCount == 0 && IOHandler.StarStream.Count == 0 && trueTime >= (audioSource.clip.length);
    }

    /// <summary>
    /// Kills the current level by setting its running status to false
    /// </summary>
    protected void KillLevel()
    {
        isRunning = false;
    }

    /// <summary>
    /// Closes the level, killing off any remaining stars and initiating the score display
    /// </summary>
    protected void CloseLevel()
    {
        StarRoot.KillAllStars();
        scoreHandler.FinalizeRank();
    }

    /// <summary>
    /// Generates the necessary visuals for a star, based on the given data
    /// </summary>
    /// <param name="starData"></param>
    protected void GenerateAStar(NotePiece starData)
    {
        var realone = GenerateToParent(starData, StarRoot.transform);

        if (MiniClonePlatform != null)
        {
            var clone = GenerateToParent(starData, MiniClonePlatform, false);
            realone.Doppelganger = clone.gameObject;
        }
    }
    /// <summary>
    /// Generates a star and assigns it to a parent holder
    /// </summary>
    /// <param name="starData">Data with information on the star's location and hand-handler</param>
    /// <param name="rootParent">The parent to parent the star to</param>
    /// <param name="isReal">Whether this is a true star or a reference star</param>
    /// <returns></returns>
    protected starblock GenerateToParent(NotePiece starData, Transform rootParent, bool isReal = true)
    {
        var newStarObject = GameObject.Instantiate(DanceBitPrefab, rootParent);
        var spawnPosition = starData.Position;
        //if (!isReal)
        //{
        //    spawnPosition = new Vector3(-spawnPosition.x, spawnPosition.y, -spawnPosition.z);
        //}
        newStarObject.transform.localPosition = spawnPosition;
        //newStarObject.transform.position = GazePoint.position;
        var starScript = newStarObject.GetComponent<starblock>();
        newStarObject.GetComponent<StarShootIn>().SetFinalPosition(starData.Position);
        var materialData = (starData.IsLeft ? LeftStarMaterials : RightStarMaterials);
        var handlerName = (starData.IsLeft ? "Left" : "Right");
        starScript.IsRealStar = isReal;
        starScript.Initiate(StarRoot, warpInTime, warpOutTime, numberOfPredecessingBeatsInSeconds, activeTime, materialData, handlerName);
        return starScript;
    }

    /// <summary>
    /// Initiates load of stars from an XML Document
    /// </summary>
    protected void XMLLoad()
    {
        IOHandler.LoadToList(BeatsPerMinute, audioSource.clip.frequency, TimeSignature, predecessingTime);
    }
}

[System.Serializable]
public class StarMaterialData
{
    [SerializeField]
    public Material MainStarMat;

    [SerializeField]
    public Material OutlineStarMat;

    [SerializeField]
    public Material BubbleMat;
}


/// <summary>
/// Carries information on a spawn object at a particular point in some song
/// </summary>
public class NotePiece
{
    /// <summary>
    /// Whether the hand handler for this object is the left or right hand
    /// </summary>
    public bool IsLeft;
    

    protected float beatID;
    /// <summary>
    /// The number of beats into the song that this note piece belongs
    /// </summary>
    public float BeatID
    {
        get
        {
            return beatID;
        }
    }

    /// <summary>
    /// When this object's peak point is set, in seconds
    /// </summary>
    protected float timeStamp;


    protected float loadTimeStamp;
    /// <summary>
    /// When this object needs to spawn to give it time to animate/load into its spot
    /// </summary>
    public float LoadTimeStamp
    {
        get
        {
            return loadTimeStamp;
        }
    }
    /// <summary>
    /// Provides information so the correct time to spawn the note and give it time to animate/load in can be calculated
    /// </summary>
    /// <param name="BPM">Beats per Minute of the level music</param>
    /// <param name="timeSignature">The suffix of the level music's time signature</param>
    /// <param name="preloadTime">How many seconds the object needs to animate/load in</param>
    public void setLoadTimeStamp(int BPM, int timeSignature, float preloadTime)
    {
        timeStamp = (timeSignature * beatID * 60) / (BPM);
        loadTimeStamp = timeStamp - preloadTime;
    }

    protected Vector3 position;
    /// <summary>
    /// The local position of the object
    /// </summary>
    public Vector3 Position
    {
        get
        {
            return position;
        }
    }
    /// <summary>
    /// Holds data on a spawn that can be hit by the player by a controller
    /// </summary>
    /// <param name="beatid">The number of beats into the song that the spawn will spawn</param>
    /// <param name="x">The local x position</param>
    /// <param name="y">The local y position</param>
    /// <param name="z">The local z position</param>
    /// <param name="isLeft">Whether the hand that can hit it is the left hand. If false, it is the right hand</param>
    public NotePiece(float beatid, float x, float y, float z, bool isLeft)
    {
        beatID = beatid;
        position = new Vector3(x, y, z);
        IsLeft = isLeft;
    }
}