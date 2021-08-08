using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gives some extra control to the OVRScreenFade screenfader
/// </summary>
public class MySceneTransitioner: Singleton<MySceneTransitioner>
{
    protected MySceneTransitioner() { }
    public OVRScreenFade ScreenFader;
    protected bool originalFadeOnStartValue;

    /// <summary>
    /// Enables a gradual fade out to a new scene
    /// </summary>
    /// <param name="scenename">The name of the scene</param>
    public void ClassicFadeToScene(string scenename)
    {
        originalFadeOnStartValue = ScreenFader.fadeOnStart;
        ScreenFader.fadeOnStart = false;
        ScreenFader.SetFadeLevel(0f);
        ScreenFader.FadeOut();
        StartCoroutine(Fade(scenename));
    }
    protected IEnumerator Fade(string scenename)
    {
        yield return new WaitUntil(isFadeComplete);
        ScreenFader.fadeOnStart = originalFadeOnStartValue;
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }
    protected bool isFadeComplete()
    {
        return ScreenFader.currentAlpha == 1f;
    }
}
