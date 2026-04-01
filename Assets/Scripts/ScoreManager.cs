using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    //[SerializeField] private TMP_Text goodScoreText;
    //[SerializeField] private TMP_Text badScoreText;

    public static event Action<int, int> OnScoreChanged;

    public int GoodScore { get; private set; }
    public int BadScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //UpdateUI();
    }

    public void AddScore(BallKind ballType)
    {
        if (ballType == BallKind.Good)
        {
            GoodScore++;
            ScoreMeter.Instance?.AddGoodBall();
        }
        else
        {
            BadScore++;
            ScoreMeter.Instance?.AddBadBall();
        }

        OnScoreChanged?.Invoke(GoodScore, BadScore);
        float balance = ScoreMeter.Instance != null ? ScoreMeter.Instance.GetCurrentValue() : 0f;

        //UpdateUI();
        Debug.Log($"Good: {GoodScore} | Bad: {BadScore} | Balance: {ScoreMeter.Instance.GetCurrentValue()}");

    }

    public void ResetScores()
    {
        GoodScore = 0;
        BadScore = 0;
        OnScoreChanged?.Invoke(GoodScore, BadScore);
    }

    //private void UpdateUI()
    //{
    //    if (goodScoreText != null)
    //        goodScoreText.text = $"Good: {GoodScore}";

    //    if (badScoreText != null)
    //        badScoreText.text = $"Bad: {BadScore}";
    //}
}