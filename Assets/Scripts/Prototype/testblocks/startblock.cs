using System.Collections;
using UnityEngine;

/// <summary>
/// Connects a collider to a scene that should load when the collider is hit.
/// </summary>
public class startblock : MonoBehaviour
{
    /// <summary>
    /// Name of the scene that should load
    /// </summary>
    public string NameOfTriggeredScene;

    /// <summary>
    /// Background music that should be faded out before closing level
    /// </summary>
    public AudioSource AtmosphericMusic;

    /// <summary>
    /// Rotater of the block
    /// </summary>
    public SimpleRotate DiscRotater;

    /// <summary>
    /// has the block already been triggered, or is it available for triggering?
    /// </summary>
    protected bool isTriggered;

    private void Awake()
    {
        isTriggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.tag == "Player" || other.tag == "Left" || other.tag == "Right"))
        {
            MySceneTransitioner.Instance.ClassicFadeToScene(NameOfTriggeredScene);
            StartCoroutine(StartFadeout());
            isTriggered = true;
        }

    }

    protected IEnumerator StartFadeout()
    {
        var timer = 0f;
        var startVolume = AtmosphericMusic.volume;
        var startRotate = DiscRotater.speed;
        var endRotate = new Vector3(0, startRotate.y * 2.5f, 0);
        var factor = 0f;
        while (timer < .5f)
        {
            timer = timer + Time.deltaTime;
            factor = timer / .5f;
            AtmosphericMusic.volume = Mathf.Lerp(startVolume, 0f, factor);
            DiscRotater.speed = Vector3.Lerp(startRotate, endRotate, factor);
            yield return new WaitForEndOfFrame();
        }
        AtmosphericMusic.Stop();
    }
}
