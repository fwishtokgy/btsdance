using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the update and display of score information to the player, and the score's transformation into a rating at the end
/// </summary>
public class FancyScoreHandler : MonoBehaviour
{
    public Transform ScoreHolder;
    public Transform RatingHolder;

    public Vector3 FinalDisplayScorePosition;
    public Vector3 StartingScorePosition;

    public TextMesh RankText;

    public TextMesh ScoreText;
    public TextMesh ComboText;

    protected Vector3 finalRatingPosition;
    protected float finalRatingScale;

    public Collider Collider;

    public MySceneTransitioner sceneTransitioner;

    public SmoothFollow smoothFollow;
    public ConstantCameraLookAt CameraFollower;
    public float finalDistanceFromPlayer;
    public Transform finalFocus;

    public float finalHeight;
    public AudioSource audioSource;

    public TextMesh accuracyholder;

    // Start is called before the first frame update
    void Start()
    {
        finalRatingPosition = RatingHolder.localPosition;
        finalRatingScale = RatingHolder.localScale.x;

        RatingHolder.localPosition = new Vector3(0, -.5f, 0);
        RatingHolder.localScale = Vector3.zero;

        ScoreHolder.localPosition = StartingScorePosition;
    }

    public void InitiateAward(string Rank)
    {
        RankText.text = Rank;
        smoothFollow.enabled = false;
        CameraFollower.enabled = false;
        StartCoroutine(ShowAward());
    }
    protected IEnumerator ShowAward()
    {
        var time = 0f;
        var finalScaleVector = new Vector3(finalRatingScale, finalRatingScale, finalRatingScale);
        var oldPosition = RatingHolder.localPosition;
        var originalComboTextScale = ComboText.transform.localScale;
        var oldRootPosition = this.transform.localPosition;
        var planeangle = new Vector2(this.transform.position.x - smoothFollow.Target.position.x, this.transform.position.z - smoothFollow.Target.position.z).normalized;
        var finalRootPosition = new Vector3(planeangle.x * finalDistanceFromPlayer, finalHeight, planeangle.y * finalDistanceFromPlayer);
        var oldCamPosition = Camera.main.transform.position;
        var finalCamPosition = new Vector3(0, finalHeight, 0);
        finalFocus.position = Camera.main.transform.position;

        var oldScoreScale = ScoreHolder.transform.localScale;
        while (time < .25f)
        {
            var progress = time / .25f;
            this.transform.localPosition = Vector3.Lerp(oldRootPosition, finalRootPosition, progress);
            finalFocus.position = Vector3.Lerp(oldCamPosition, finalCamPosition, progress);
            this.transform.LookAt(finalFocus);
            ScoreHolder.transform.localScale = Vector3.Lerp(oldScoreScale, Vector3.zero, progress);
            time = time + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        this.transform.LookAt(finalCamPosition);
        RatingHolder.gameObject.SetActive(true);
        ComboText.gameObject.SetActive(false);
        audioSource.Play();
        time = 0f;
        accuracyholder.transform.localScale = Vector3.zero;

        while (time < .5f)
        {
            var progress = time / .5f;
            RatingHolder.localScale = Vector3.Lerp(Vector3.zero, finalScaleVector, progress);
            RatingHolder.localPosition = Vector3.Lerp(oldPosition, finalRatingPosition, progress);
            ScoreHolder.localPosition = Vector3.Lerp(StartingScorePosition, FinalDisplayScorePosition, progress);
            ComboText.transform.localScale = Vector3.Lerp(originalComboTextScale, Vector3.zero, progress);
            accuracyholder.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            time = time + Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
