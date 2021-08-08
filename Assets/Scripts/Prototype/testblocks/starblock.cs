using System.Collections;
using UnityEngine;

/// <summary>
/// A bubbled star that can be hit for points. If it whiffs, you lose points
/// </summary>
public class starblock : MonoBehaviour
{
    protected bool _transitioning;
    protected bool _hittable;
    protected bool _wasHit;

    protected float _warpInTime;
    protected float _warpOutTime;

    public StarBlockHandler motherStar;

    /// <summary>
    /// For how many beats does a note hang before being activated?
    /// </summary>
    protected float _beatsBeforePlayableInSeconds;

    /// <summary>
    /// The period of time a starblock takes to crest from a 'poor' to a 'perfect'
    /// </summary>
    protected float _activeTime;


    public Transform coreStar;
    public Transform shellStar;
    public Transform collisionShell;

    public AudioClip EntranceSound;
    public AudioClip CollectedSound;
    public AudioClip VeryGoodCollectionSound;
    public AudioClip MissSound;

    public AudioSource MySoundSource;

    protected float coreScale;
    protected float shellScale;
    protected float collisionScale;

    /// <summary>
    /// Current accuracy, based on how close the time is to the bubble's apex.
    /// </summary>
    protected float pointAccuracy;

    /// <summary>
    /// Name of the starblock's handler. Is either 'Left' or 'Right' and is ascribed from the XML
    /// </summary>
    public string MyHandler;

    /// <summary>
    /// Whether this is a real star the player can interact with for points or a doppelganger
    /// </summary>
    public bool IsRealStar;

    /// <summary>
    /// Reference to a doppelganger, if the instance this script is on is a real star
    /// </summary>
    public GameObject Doppelganger;

    public TriggerContainer triggerContainer;

    protected enum hitState
    {
        Bad,
        Poor,
        Okay,
        Great,
        Perfect
    }

    public bool IsHittable
    {
        get
        {
            return _hittable;
        }
    }

    public Renderer MainStarMat;
    public Renderer OutlineStarMat;
    public Renderer BubbleMat;

    public void Initiate(StarBlockHandler motherOfStars, float warpInTime, float warpOutTime, float beatsBeforePlayableInSeconds, float activeTime, StarMaterialData materialData, string handlerName)
    {
        motherStar = motherOfStars;
        _warpInTime = warpInTime;
        _warpOutTime = warpOutTime;
        _beatsBeforePlayableInSeconds = beatsBeforePlayableInSeconds;
        _activeTime = activeTime;

        pointAccuracy = 0f;

        if (IsRealStar)
        {
            MySoundSource.clip = EntranceSound;
            MySoundSource.volume = .5f;
            MySoundSource.Play();
        }

        MainStarMat.material = new Material(IsRealStar?materialData.MainStarMat:materialData.MainStarMat);
        OutlineStarMat.material = new Material(IsRealStar ? materialData.OutlineStarMat:materialData.BubbleMat);
        BubbleMat.material = new Material(materialData.BubbleMat);

        MyHandler = handlerName;
        triggerContainer.Tags[0] = MyHandler;

        StartCoroutine(startWarpIn());
    }

    void Awake()
    {
        _hittable = false;
        _transitioning = true;

        collisionShell.gameObject.SetActive(false);

        coreScale = coreStar.localScale.x;
        shellScale = shellStar.localScale.x;
        collisionScale = collisionShell.localScale.x;

        coreStar.localScale = Vector3.zero;
        shellStar.localScale = Vector3.zero;
        collisionShell.localScale = Vector3.zero;
    }

    /// <summary>
    /// Start displaying the star
    /// </summary>
    IEnumerator startWarpIn()
    {
        var newScale = new Vector3(shellScale, shellScale, shellScale);

        StartCoroutine(oldFashionedScaleLerp(_warpInTime, shellStar.transform, Vector3.zero, newScale));

        yield return new WaitForSeconds(_warpInTime);

        _transitioning = false;

        StartCoroutine(hold());
    }
    /// <summary>
    /// Hold the star to give player time to find it
    /// </summary>
    IEnumerator hold()
    {
        var timeToHittable = (_beatsBeforePlayableInSeconds) - _activeTime;
        yield return new WaitForSeconds(timeToHittable);
        if (IsRealStar)
        {
            collisionShell.gameObject.SetActive(true);
        }
        StartCoroutine(activeState());
        if (IsRealStar)
        {
            _hittable = true;
        }
    }
    /// <summary>
    /// Lerp in a poppable bubble, then lerp it out
    /// </summary>
    IEnumerator activeState()
    {
        var timeToMiss = _activeTime * 2f;
        var timer = 0f;

        var coreStarScale = new Vector3(coreScale, coreScale, coreScale);
        StartCoroutine(oldFashionedScaleLerp(.25f, coreStar.transform, Vector3.zero, coreStarScale));

        pointAccuracy = collisionShell.localScale.x;
        var collisionScaleValue = 0f;
        var initiatedScaleBack = false;
        while (timer < timeToMiss)
        {
            pointAccuracy = Mathf.Sin(Mathf.PI * (timer / timeToMiss));
            collisionScaleValue = pointAccuracy * collisionScale;
            collisionShell.localScale = new Vector3(collisionScaleValue, collisionScaleValue, collisionScaleValue);
            if (!initiatedScaleBack && timer/timeToMiss > .5f && pointAccuracy < .25f)
            {
                initiatedScaleBack = true;
                StartCoroutine(oldFashionedScaleLerp(.25f, coreStar.transform, coreStarScale, Vector3.zero));
            }
            timer = timer + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        collisionShell.gameObject.SetActive(false);

        _hittable = false;
        yield return new WaitForSeconds(.05f);
        StartCoroutine(startWarpOut());
    }
    /// <summary>
    /// Start lerping the star away
    /// </summary>
    IEnumerator startWarpOut()
    {
        var timer = 0f;
        var oldScale = this.transform.localScale;

        if (IsRealStar)
        {
            MySoundSource.clip = MissSound;
            MySoundSource.volume = .25f;
            MySoundSource.Play();
            if (Doppelganger != null)
            {
                GameObject.Destroy(Doppelganger.gameObject);
            }
        }

        _transitioning = true;
        while (timer < _warpOutTime)
        {
            this.transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, (timer/_warpOutTime));
            timer = timer + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        BadKill();
    }
    /// <summary>
    /// Start closing out the bubble and star if it was hit by a player
    /// </summary>
    IEnumerator collectOut()
    {
        var timer = 0f;
        var oldScale = this.transform.localScale;

        var clip = CollectedSound;
        if (pointAccuracy > .85f)
        {
            clip = VeryGoodCollectionSound;
        }
        MySoundSource.clip = clip;
        MySoundSource.volume = 1f;
        MySoundSource.Play();

        if (IsRealStar && Doppelganger != null)
        {
            GameObject.Destroy(Doppelganger.gameObject);
        }

        var collectTime = .25f;

        var soundTimeRemaining = clip.length - collectTime;

        _transitioning = true;
        while (timer < collectTime)
        {
            this.transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, (timer / collectTime));
            timer = timer + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        this.transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(soundTimeRemaining);
        GoodKill();
    }
    /// <summary>
    /// helper function to scale some subject over a given duration
    /// </summary>
    IEnumerator oldFashionedScaleLerp(float duration, Transform subject, Vector3 oldScale, Vector3 newScale, float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        var timer = 0f;
        while (timer < duration)
        {
            subject.localScale = Vector3.Lerp(oldScale, newScale, (timer / duration));
            timer = timer + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        subject.localScale = newScale;
    }
    /// <summary>
    /// Kill that is called if the player hit the star
    /// </summary>
    protected void GoodKill()
    {
        if (IsRealStar)
        {
            motherStar.ReportStarCollection(this);
        }
        GameObject.Destroy(this.gameObject);
    }
    /// <summary>
    /// Kill that is called if the player never hit the star
    /// </summary>
    protected void BadKill()
    {
        if (IsRealStar)
        {
            motherStar.ReportStarDeath(this);
        }
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Manual kill called if something happens, like a level end
    /// </summary>
    public void EarlyKill()
    {
        StopAllCoroutines();
        StartCoroutine(startWarpOut());
    }
    /// <summary>
    /// Call to indicate a hit to the star
    /// </summary>
    public void CollisionCall()
    {
        if (!_hittable || _transitioning)
        {
            return;
        }
        StopAllCoroutines();
        _hittable = false;
        StartCoroutine(collectOut());
    }
}
