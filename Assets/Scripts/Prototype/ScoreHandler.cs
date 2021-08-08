using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the score numbers
/// </summary>
public class ScoreHandler : MonoBehaviour
{
    protected int Score;
    public int StartingScore;

    /// <summary>
    /// Amount to subtract from score. This should be a positive number
    /// </summary>
    public int ArbitraryPenalty;

    /// <summary>
    /// Amount to add to score. This should be a positive number.
    /// </summary>
    public int ArbitraryAward;

    protected int Streak;

    /// <summary>
    /// The graphic display controller
    /// </summary>
    public FancyScoreHandler scoreHandler;

    public delegate void OnScoreMet();
    public static event OnScoreMet OnDeath;

    protected int MaxPossibleScore;

    public void SetMaximumInputData(int numberOfBlocks)
    {
        MaxPossibleScore = (numberOfBlocks * ArbitraryAward) + StartingScore;
    }

    protected bool isCounting;
    protected void ReportDeath()
    {
        if (OnDeath != null)
        {
            OnDeath();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StarBlockHandler.OnCollected += AwardToScore;
        StarBlockHandler.OnDeath += PenalizeScore;

        Score = StartingScore;

        UpdateScore();
        UpdateStreak();

        isCounting = true;

        HideStreak();
    }

    protected void PenalizeScore()
    {
        if (!isCounting)
        {
            return;
        }

        Score = Score - ArbitraryPenalty;
        if (Streak > 9)
        {
            HideStreak();
        }
        Streak = 0;
        if (Score <= 0)
        {
            ReportDeath();
            Score = 0;
            isCounting = false;
        }
        UpdateScore();
    }
    protected void AwardToScore()
    {
        if (!isCounting)
        {
            return;
        }

        Score = Score + ArbitraryAward;
        Streak++;
        if (Streak > 9)
        {
            ShowStreak();
        }
        UpdateScore();
        UpdateStreak();
    }

    protected void UpdateScore()
    {
        scoreHandler.ScoreText.text = "SCORE: "+Score.ToString() + "/"+MaxPossibleScore;
    }
    protected void UpdateStreak()
    {
        scoreHandler.ComboText.text = Streak + " COMBOS";
    }

    protected void ShowStreak()
    {
        scoreHandler.ComboText.gameObject.SetActive(true);
    }
    protected void HideStreak()
    {
        scoreHandler.ComboText.gameObject.SetActive(false);
    }

    public void FinalizeRank()
    {
        var finalPercentile = (Score * 1f) / MaxPossibleScore;
        var rank = "SS";

        scoreHandler.accuracyholder.text = string.Format("Accuracy: {0:F2}%",finalPercentile*100f);

        if (finalPercentile < .60f)
        {
            rank = "F";
        }
        else if (finalPercentile < .70f)
        {
            rank = "D";
        }
        else if (finalPercentile < .80f)
        {
            rank = "C";
        }
        else if (finalPercentile < .90f)
        {
            rank = "B";
        }
        else if (finalPercentile < .99f)
        {
            rank = "A";
        }
        scoreHandler.InitiateAward(rank);
    }

    private void OnDestroy()
    {
        StarBlockHandler.OnCollected -= AwardToScore;
        StarBlockHandler.OnDeath -= PenalizeScore;
    }
}
