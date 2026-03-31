using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text goodScoreText;
    [SerializeField] private TMP_Text badScoreText;

    public int GoodScore { get; private set; }
    public int BadScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
        UpdateUI();
    }

    public void AddScore(BallType ballType)
    {
        if (ballType == BallType.Good)
            GoodScore++;
        else
            BadScore++;

        UpdateUI();

        Debug.Log($"Good: {GoodScore} | Bad: {BadScore}");
    }

    private void UpdateUI()
    {
        if (goodScoreText != null)
            goodScoreText.text = $"Good: {GoodScore}";

        if (badScoreText != null)
            badScoreText.text = $"Bad: {BadScore}";
    }
}