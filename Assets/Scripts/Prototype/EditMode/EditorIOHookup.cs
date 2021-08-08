using System.Collections;
using UnityEngine;

/// <summary>
/// Hooks up the XML IO Handler with controls, so the player can initiate save or load or exit
/// </summary>
public class EditorIOHookup : MonoBehaviour
{
    public KeyCode DebugSaveKey;
    public OVRInput.Button SaveButton;

    public KeyCode DebugExitKey;
    public OVRInput.Button ExitButton;

    /// <summary>
    /// The controller that should hold these functions
    /// </summary>
    public OVRInput.Controller Controller;

    public StarIO IOHandler;

    public AudioScrubber Scrubber;

    public MySceneTransitioner sceneTransitioner;

    public TextMesh StatusText;

    // Start is called before the first frame update
    void Start()
    {
        Scrubber.LoadInStars(IOHandler);
        StartCoroutine(LoadAnyInitialStars());
    }
    IEnumerator LoadAnyInitialStars()
    {
        yield return new WaitUntil(isReadyForInstructions);
        Scrubber.RefreshStars();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(DebugSaveKey) || OVRInput.GetDown(SaveButton, Controller))
        {
            Scrubber.SaveStars(IOHandler);
            StartCoroutine(SetStatusToWait());
        }
        if (Input.GetKeyDown(DebugExitKey) || OVRInput.GetDown(ExitButton, Controller))
        {
            Scrubber.SaveStars(IOHandler);
            StartCoroutine(SetStatusToWait());
            StartCoroutine(ReturnToMenu());
        }
    }

    IEnumerator SetStatusToWait()
    {
        while (!isReadyForInstructions())
        {
            StatusText.text = string.Format("Saving... {0}%", Mathf.RoundToInt(IOHandler.SavePercentile * 100f));
            yield return new WaitForEndOfFrame();
        }
        for (int ticker = 3; ticker > 0; ticker += -1)
        {
            StatusText.text = string.Format("Completed Save. ({0})", ticker);
            yield return new WaitForSeconds(1);
        }
        StatusText.text = "";
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitUntil(isReadyForInstructions);
        yield return new WaitForSeconds(3);
        sceneTransitioner.ClassicFadeToScene("startscene");
        StatusText.text = string.Format("Closing Editor.");
    }

    protected bool isReadyForInstructions()
    {
        return IOHandler.IsProcessing == false;
    }
}
