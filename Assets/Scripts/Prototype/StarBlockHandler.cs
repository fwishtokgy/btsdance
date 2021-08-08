using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds a queue of all current stars
/// </summary>
public class StarBlockHandler : MonoBehaviour
{
    protected Queue<starblock> Stars;

    private void Awake()
    {
        ScoreHandler.OnDeath += KillAllStars;
    }
    private void OnDestroy()
    {
        ScoreHandler.OnDeath -= KillAllStars;
    }

    public void RegisterStar(starblock star)
    {
        //Stars.Enqueue(star);
    }

    public delegate void OnStarCollected();
    /// <summary>
    /// Called when a star has been collected
    /// </summary>
    public static event OnStarCollected OnCollected;
    public void ReportStarCollection(starblock star)
    {
        if (OnCollected != null)
        {
            OnCollected();
        }
    }

    public delegate void OnStarDeath();
    public static event OnStarDeath OnDeath;
    public void ReportStarDeath(starblock star)
    {
        if (OnDeath != null)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// Destroys all stars currently held as children to this object
    /// </summary>
    public void KillAllStars()
    {
        foreach (var star in transform.GetComponentsInChildren<starblock>())
        {
            star.EarlyKill();
        }
    }
}