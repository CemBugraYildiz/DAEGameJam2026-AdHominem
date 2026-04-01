using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private enum GamePhase
    {
        BallCollect,
        TransitionToJump,
        JumpMiniGame,
        Ended
    }

    [Header("Bubbles")]
    [SerializeField] private BubbleProgressUI playerBubble;
    [SerializeField] private BubbleProgressUI npcBubble;

    [Header("Expressions")]
    [SerializeField] private ExpressionUIController expressionController;
    [SerializeField] private float hurtExpressionDuration = 1f;

    [Header("Spawners")]
    [SerializeField] private BallPoolSpawner ballSpawner;
    [SerializeField] private JumpObstacleSpawner jumpSpawner;

    [Header("Phase Roots")]
    [SerializeField] private GameObject ballPhaseRoot;
    [SerializeField] private GameObject jumpPhaseRoot;

    [Header("End Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Selectable winFirstButton;
    [SerializeField] private Selectable loseFirstButton;

    [Header("Players")]
    [SerializeField] private PlayerInput[] playerInputs;

    private GamePhase currentPhase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        HideEndScreens();

        if (playerInputs == null || playerInputs.Length == 0)
            playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        if (playerBubble != null)
        {
            playerBubble.OnAllSlotsFilled += HandlePlayerBubbleFilled;
            playerBubble.OnTimerExpired += HandlePlayerBubbleTimerExpired;
        }

        if (npcBubble != null)
        {
            npcBubble.OnAllSlotsFilled += HandleNpcBubbleFilled;
            npcBubble.OnTimerExpired += HandleNpcBubbleTimerExpired;
        }

        ScoreManager.OnBallScored += HandleBallScored;
    }

    private void OnDisable()
    {
        if (playerBubble != null)
        {
            playerBubble.OnAllSlotsFilled -= HandlePlayerBubbleFilled;
            playerBubble.OnTimerExpired -= HandlePlayerBubbleTimerExpired;
        }

        if (npcBubble != null)
        {
            npcBubble.OnAllSlotsFilled -= HandleNpcBubbleFilled;
            npcBubble.OnTimerExpired -= HandleNpcBubbleTimerExpired;
        }

        ScoreManager.OnBallScored -= HandleBallScored;
    }

    private void Start()
    {
        BeginBallPhase();
    }

    private void Update()
    {
        if (currentPhase == GamePhase.Ended)
            return;

        if (ScoreMeter.Instance != null && ScoreMeter.Instance.IsAtMax())
        {
            WinGame();
            return;
        }

        if (currentPhase == GamePhase.JumpMiniGame &&
            ScoreMeter.Instance != null &&
            ScoreMeter.Instance.IsAtMin())
        {
            LoseGame();
            return;
        }

        if (currentPhase == GamePhase.BallCollect)
        {
            UpdateExpressionFromScoreMeter();
        }
    }

    private void BeginBallPhase()
    {
        currentPhase = GamePhase.BallCollect;

        ShowBallPhase();

        if (npcBubble != null)
        {
            npcBubble.StopTimer();
            npcBubble.gameObject.SetActive(false);
        }

        if (playerBubble != null)
        {
            playerBubble.gameObject.SetActive(true);
            playerBubble.ResetProgress(true);
        }

        UpdateExpressionFromScoreMeter();
        OpenAllPlayerInputs();
    }

    private IEnumerator BeginJumpPhaseRoutine(ExpressionState preJumpExpression)
    {
        currentPhase = GamePhase.TransitionToJump;

        if (ballSpawner != null)
            ballSpawner.StopAndClearAll();

        if (ballPhaseRoot != null)
            ballPhaseRoot.SetActive(false);

        if (playerBubble != null)
            playerBubble.StopTimer();

        if (expressionController != null)
            expressionController.Show(preJumpExpression);

        yield return new WaitForSeconds(hurtExpressionDuration);

        if (currentPhase == GamePhase.Ended)
            yield break;

        if (playerBubble != null)
            playerBubble.gameObject.SetActive(false);

        if (npcBubble != null)
        {
            npcBubble.gameObject.SetActive(true);
            npcBubble.ResetProgress(true);
        }

        ShowJumpPhase();

        currentPhase = GamePhase.JumpMiniGame;

        if (expressionController != null)
            expressionController.Show(ExpressionState.Talking);
    }

    private void ReturnFromJumpToBallPhase()
    {
        if (jumpSpawner != null)
            jumpSpawner.StopAndClearAll();

        if (jumpPhaseRoot != null)
            jumpPhaseRoot.SetActive(false);

        if (npcBubble != null)
        {
            npcBubble.StopTimer();
            npcBubble.gameObject.SetActive(false);
        }

        if (playerBubble != null)
        {
            playerBubble.gameObject.SetActive(true);
            playerBubble.ResetProgress(true);
        }

        ShowBallPhase();

        currentPhase = GamePhase.BallCollect;
        UpdateExpressionFromScoreMeter();
    }

    private void HandleBallScored(BallKind ballKind)
    {
        if (currentPhase != GamePhase.BallCollect)
            return;

        if (ballKind == BallKind.Good)
        {
            playerBubble?.AddStep();
        }
    }

    private void HandlePlayerBubbleFilled(BubbleProgressUI bubble)
    {
        if (currentPhase != GamePhase.BallCollect)
            return;

        StartCoroutine(BeginJumpPhaseRoutine(ExpressionState.Hurt));
    }

    private void HandlePlayerBubbleTimerExpired(BubbleProgressUI bubble)
    {
        if (currentPhase != GamePhase.BallCollect)
            return;

        if (bubble == null)
            return;

        ExpressionState nextExpression =
            bubble.HasHalfOrMoreFilled()
            ? ExpressionState.Hurt
            : ExpressionState.NotHurt;

        StartCoroutine(BeginJumpPhaseRoutine(nextExpression));
    }

    private void HandleNpcBubbleFilled(BubbleProgressUI bubble)
    {
        if (currentPhase != GamePhase.JumpMiniGame)
            return;

        LoseGame();
    }

    private void HandleNpcBubbleTimerExpired(BubbleProgressUI bubble)
    {
        if (currentPhase != GamePhase.JumpMiniGame)
            return;

        if (ScoreMeter.Instance != null && ScoreMeter.Instance.IsAtMin())
        {
            LoseGame();
            return;
        }

        ReturnFromJumpToBallPhase();
    }

    public void ReportJumpObstacleHit()
    {
        if (currentPhase != GamePhase.JumpMiniGame)
            return;

        ScoreMeter.Instance?.AddBadBall();
        npcBubble?.AddStep();

        if (ScoreMeter.Instance != null && ScoreMeter.Instance.IsAtMin())
        {
            LoseGame();
        }
    }

    private void UpdateExpressionFromScoreMeter()
    {
        if (expressionController == null || ScoreMeter.Instance == null)
            return;

        if (ScoreMeter.Instance.IsInCenterThird())
        {
            expressionController.Show(ExpressionState.Neutral);
        }
        else if (ScoreMeter.Instance.IsInRightThird())
        {
            expressionController.Show(ExpressionState.Losing);
        }
        else
        {
            expressionController.Show(ExpressionState.Winning);
        }
    }

    private void WinGame()
    {
        if (currentPhase == GamePhase.Ended)
            return;

        currentPhase = GamePhase.Ended;

        HideAllPhases();

        playerBubble?.StopTimer();
        npcBubble?.StopTimer();

        CloseAllPlayerInputs();

        if (winScreen != null)
        {
            winScreen.SetActive(true);
            StartCoroutine(SelectButtonNextFrame(winFirstButton));
        }
    }

    private void LoseGame()
    {
        if (currentPhase == GamePhase.Ended)
            return;

        currentPhase = GamePhase.Ended;

        HideAllPhases();

        playerBubble?.StopTimer();
        npcBubble?.StopTimer();

        CloseAllPlayerInputs();

        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
            StartCoroutine(SelectButtonNextFrame(loseFirstButton));
        }
    }

    private IEnumerator SelectButtonNextFrame(Selectable button)
    {
        yield return null;

        if (EventSystem.current != null && button != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            button.Select();
        }
    }

    private void HideEndScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenAllPlayerInputs()
    {
        foreach (var player in playerInputs)
        {
            if (player == null) continue;
            player.ActivateInput();
            player.SwitchCurrentActionMap("Player");
        }
    }

    private void CloseAllPlayerInputs()
    {
        foreach (var player in playerInputs)
        {
            if (player == null) continue;
            player.DeactivateInput();
        }
    }

    private void ShowBallPhase()
    {
        if (jumpSpawner != null)
            jumpSpawner.StopAndClearAll();

        if (jumpPhaseRoot != null)
            jumpPhaseRoot.SetActive(false);

        if (ballPhaseRoot != null)
            ballPhaseRoot.SetActive(true);

        if (ballSpawner != null)
            ballSpawner.StartSpawning();
    }

    private void ShowJumpPhase()
    {
        if (ballSpawner != null)
            ballSpawner.StopAndClearAll();

        if (ballPhaseRoot != null)
            ballPhaseRoot.SetActive(false);

        if (jumpPhaseRoot != null)
            jumpPhaseRoot.SetActive(true);

        if (jumpSpawner != null)
            jumpSpawner.StartSpawning();
    }

    private void HideAllPhases()
    {
        if (ballSpawner != null)
            ballSpawner.StopAndClearAll();

        if (jumpSpawner != null)
            jumpSpawner.StopAndClearAll();

        if (ballPhaseRoot != null)
            ballPhaseRoot.SetActive(false);

        if (jumpPhaseRoot != null)
            jumpPhaseRoot.SetActive(false);
    }
}