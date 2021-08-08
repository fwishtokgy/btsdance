using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pulls the level music to a crawl and then proceeds to kill the current level
/// </summary>
public class LevelKiller : MonoBehaviour
{
    public AudioSource audioSource;
    /// <summary>
    /// How long it takes in seconds to kill the audio
    /// </summary>
    public float KillTime;

    /// <summary>
    /// Reference to audio that will give a quip after the audio dies
    /// </summary>
    public AudioSource announcerSource;

    public MySceneTransitioner sceneTransitioner;

    protected float startingPitch;
    // Start is called before the first frame update
    void Start()
    {
        ScoreHandler.OnDeath += InitiateDeath;

        startingPitch = audioSource.pitch;
    }
    private void OnDestroy()
    {
        ScoreHandler.OnDeath -= InitiateDeath;
    }

    /// <summary>
    /// Start pulling the audio back, and then kill the level
    /// </summary>
    public void InitiateDeath()
    {
        StartCoroutine(KillMusic());
    }

    IEnumerator KillMusic()
    {
        var timer = 0f;
        while (timer < KillTime)
        {
            audioSource.pitch = Mathf.Lerp(startingPitch, 0f, timer / KillTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        audioSource.Stop();
        yield return new WaitForEndOfFrame();

        announcerSource.Play();

        yield return new WaitForSeconds(.5f);

        sceneTransitioner.ClassicFadeToScene("startscene");
    }
}
