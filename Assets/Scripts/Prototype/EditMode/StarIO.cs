using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Handles XML Save and Load of star stream information for a song
/// </summary>
public class StarIO : MonoBehaviour
{
    /// <summary>
    /// Directory to place output data.
    /// </summary>
    protected string ResourceDirectory = "";

    /// <summary>
    /// File to place output data.
    /// </summary>
    private string SetsDataOutputFile;

    protected bool IsBusy;
    /// <summary>
    /// Is the XML handler currently engaged in a task? If false, it is open for a new task
    /// </summary>
    public bool IsProcessing
    {
        get
        {
            return IsBusy;
        }
    }

    private string getPath()
    {
        #if UNITY_EDITOR
            return Application.streamingAssetsPath;
        #elif UNITY_ANDROID
            return Application.persistentDataPath;
        #elif UNITY_STANDALONE_WIN
            return Application.streamingAssetsPath;
        #else  
            return "";
        #endif
    }

    void Awake()
    {
        ResourceDirectory = Path.Combine(getPath(), "StarData");
        SetsDataOutputFile = Path.Combine(ResourceDirectory, "BTSProto.xml");

        if (!Directory.Exists(ResourceDirectory))
        {
            Directory.CreateDirectory(ResourceDirectory);
        }
    }

    /// <summary>
    /// Stars for the left hand
    /// key: the number of smallestUnit the star belongs at
    /// value: the Vector3 local position
    /// </summary>
    public Dictionary<int, Vector3> LeftStars;
    /// <summary>
    /// Stars for the right hand
    /// key: the number of smallestUnit the star belongs at
    /// value: the Vector3 local position
    /// </summary>
    public Dictionary<int, Vector3> RightStars;

    /// <summary>
    /// A queue of NotePieces that hold all the stars to play
    /// </summary>
    public Queue<NotePiece> StarStream;

    /// <summary>
    /// The max value that was found in the file
    /// This value is the max 'smallestUnit' value that was recorded
    /// </summary>
    protected int maxTimeSample;
    public int MaxTimeSample
    {
        get
        {
            return maxTimeSample;
        }
        set
        {
            maxTimeSample = value;
        }
    }

    protected float savedPercent;
    /// <summary>
    /// The percentage written to the XML Stream so far
    /// </summary>
    public float SavePercentile
    {
        get
        {
            return savedPercent;
        }
    }

    /// <summary>
    /// Save the current stars to the XML file
    /// </summary>
    public void Save()
    {
        IsBusy = true;

        savedPercent = 0;

        StartCoroutine(SaveAsync());
    }
    IEnumerator SaveAsync()
    {
        var newSongFile = new StarSongXML { };

        var newInfo = new InfoXML { };

        var newSequence = new SongXML { TimeSamples = new List<TimeSampleXML>() };

        var step = 0;
        
        while (step <= MaxTimeSample)
        {
            if (LeftStars.ContainsKey(step) || RightStars.ContainsKey(step))
            {
                var newStep = new TimeSampleXML { Value = step, Stars = new List<StarInfoXML>() };
                if (LeftStars.ContainsKey(step))
                {
                    var newStar = new StarInfoXML {HandOwner = "Left", X = LeftStars[step].x, Y = LeftStars[step].y , Z = LeftStars[step].z };
                    newStep.Stars.Add(newStar);
                }
                if (RightStars.ContainsKey(step))
                {
                    var newStar = new StarInfoXML { HandOwner = "Right", X = RightStars[step].x, Y = RightStars[step].y, Z = RightStars[step].z };
                    newStep.Stars.Add(newStar);
                }
                newSequence.TimeSamples.Add(newStep);
            }

            step++;
            savedPercent = 1f * step / MaxTimeSample;
            yield return new WaitForEndOfFrame();
        }
        newSongFile.info = newInfo;
        newSongFile.song = newSequence;

        newSongFile.Save(SetsDataOutputFile);
        savedPercent = 1f;
        yield return new WaitForSeconds(1f);
        IsBusy = false;
    }
    /// <summary>
    /// Load the stars from the XML into Dictionaries
    /// </summary>
    public void LoadToDictionary()
    {
        LeftStars = new Dictionary<int, Vector3>();
        RightStars = new Dictionary<int, Vector3>();

        if (File.Exists(SetsDataOutputFile))
        {
            IsBusy = true;
            var savedStars = StarSongXML.Load(SetsDataOutputFile);
            StartCoroutine(LoadAsyncToDictionary(savedStars));
        }
    }
    IEnumerator LoadAsyncToDictionary(StarSongXML sets)
    {
        for (int timesampleIndex = 0; timesampleIndex < sets.song.TimeSamples.Count; timesampleIndex++)
        {
            var stars = sets.song.TimeSamples[timesampleIndex];
            foreach (var star in stars.Stars)
            {
                switch (star.HandOwner)
                {
                    case "Left":
                        LeftStars.Add(stars.Value, new Vector3(star.X, star.Y, star.Z));
                        break;
                    case "Right":
                        RightStars.Add(stars.Value, new Vector3(star.X, star.Y, star.Z));
                        break;
                    default:
                        break;
                }
                if (stars.Value > MaxTimeSample)
                {
                    maxTimeSample = stars.Value;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        IsBusy = false;
    }

    /// <summary>
    /// Load stars from the XML into a queue
    /// </summary>
    /// <param name="BPM">The Beats Per Minute of the song</param>
    /// <param name="clipFrequency">The clip frequency</param>
    /// <param name="timeSignature">The suffix of the time signature</param>
    /// <param name="preloadTime">The preload time of a star in seconds</param>
    public void LoadToList(int BPM, float clipFrequency, int timeSignature, float preloadTime)
    {
        StarStream = new Queue<NotePiece>();

        if (File.Exists(SetsDataOutputFile))
        {
            IsBusy = true;
            var savedStars = StarSongXML.Load(SetsDataOutputFile);
            StartCoroutine(LoadAsyncToList(savedStars, BPM, clipFrequency, timeSignature, preloadTime));
        }
    }
    protected IEnumerator LoadAsyncToList(StarSongXML sets, int BPM, float clipFrequency, int timeSignature, float preloadTime)
    {
        var TimeSamplesPerBeat = (60f / BPM) * clipFrequency;
        // arbitrarily using .25f multiplier... this should be saved by the XML later
        var smallestBeat = .25f;
        var SmallestUnit = Mathf.RoundToInt(TimeSamplesPerBeat * smallestBeat);
        var TimeSampleToSecondsMultiplier = (1 / TimeSamplesPerBeat) * (60f / BPM);

        var BeatsPerTimeSamples = SmallestUnit * TimeSampleToSecondsMultiplier;
        for (int timesampleIndex = 0; timesampleIndex < sets.song.TimeSamples.Count; timesampleIndex++)
        {
            var stars = sets.song.TimeSamples[timesampleIndex];
            foreach (var star in stars.Stars)
            {
                var newNotePiece = new NotePiece(stars.Value * BeatsPerTimeSamples, star.X, star.Y, star.Z, star.HandOwner == "Left");
                newNotePiece.setLoadTimeStamp(BPM, timeSignature, preloadTime);
                StarStream.Enqueue(newNotePiece);
            }
            yield return new WaitForEndOfFrame();
        }
        IsBusy = false;
    }


    [XmlRoot("StarSongXML")]
    public class StarSongXML
    {
        [XmlElement(ElementName = "Info", Type = typeof(InfoXML))]
        public InfoXML info;
        [XmlElement(ElementName = "Song", Type = typeof(SongXML))]
        public SongXML song;

        public TextAsset dataFile;

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(StarIO.StarSongXML));
            var encoding = Encoding.GetEncoding("UTF-8");

            using (var stream = new StreamWriter(path, false, encoding))
            {
                stream.BaseStream.SetLength(0);
                serializer.Serialize(stream, this);
                stream.Dispose();
                stream.Close();
            }
        }
        public static StarSongXML Load(string path)
        {
            var serializer = new XmlSerializer(typeof(StarSongXML));
            var encoding = Encoding.GetEncoding("UTF-8");
            using (var stream = new StreamReader(path, encoding))
            {
                return serializer.Deserialize(stream.BaseStream) as StarSongXML;
            }
        }
    }
    public class InfoXML
    {
    }
    public class SongXML
    {
        [XmlElement(ElementName = "TimeSample", Type = typeof(TimeSampleXML))]
        public List<TimeSampleXML> TimeSamples;

    }
    public class TimeSampleXML
    {
        [XmlElement(ElementName = "Value", Type = typeof(int))]
        public int Value;

        [XmlElement(ElementName = "StarInfo", Type = typeof(StarInfoXML))]
        public List<StarInfoXML> Stars;
    }
    public class StarInfoXML
    {
        [XmlAttribute("HandOwner")]
        public string HandOwner;

        [XmlElement(ElementName = "X", Type = typeof(float))]
        public float X;
        [XmlElement(ElementName = "Y", Type = typeof(float))]
        public float Y;
        [XmlElement(ElementName = "Z", Type = typeof(float))]
        public float Z;
    }
}
