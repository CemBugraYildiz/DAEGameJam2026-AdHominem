using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpeechBubbleProgressTimer : MonoBehaviour
{
    [Header("Text Slots")]
    [SerializeField] private RectTransform[] slots;
    [SerializeField] private Image textPrefab;
    [SerializeField] private Sprite[] textSprites;
    [SerializeField] private Color collectedColor = Color.white;

    [Header("Bubble Timer")]
    [SerializeField] private Image bubbleTimerFillImage;
    [SerializeField] private float roundDuration = 60f;

    [Header("End Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Selectable winScreenFirstButton;
    [SerializeField] private Selectable loseScreenFirstButton;
    //[SerializeField] private GameObject timeOutObject; 

    [Header("Players")]
    [SerializeField] private PlayerInput[] playerInputsToDisable;

    private Image[] collectedImages;
    private float elapsedTime;
    private bool roundEnded;

    private void Awake()
    {
        BuildCollectedTexts();
        SetupBubbleFill();

        HideAllEndScreens();

        if (playerInputsToDisable == null || playerInputsToDisable.Length == 0)
            playerInputsToDisable = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        ScoreManager.OnScoreChanged += HandleScoreChanged;

        int currentGoodScore = ScoreManager.Instance != null ? ScoreManager.Instance.GoodScore : 0;
        RefreshCollectedTexts(currentGoodScore);
        RefreshBubbleTimer(0f);
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    private void Update()
    {
        if (roundEnded) return;
        if (roundDuration <= 0f) return;

        elapsedTime += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsedTime / roundDuration);
        RefreshBubbleTimer(progress);

        if (progress >= 1f)
            EndRound();
    }

    public void ResetBubbleAndTimer()
    {
        elapsedTime = 0f;
        roundEnded = false;

        HideAllEndScreens();

        int currentGoodScore = ScoreManager.Instance != null ? ScoreManager.Instance.GoodScore : 0;
        RefreshCollectedTexts(currentGoodScore);
        RefreshBubbleTimer(0f);
    }

    public void RestartGame()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScores();

        ResetBubbleAndTimer();

        OpenAllPlayerInputs();

        BallPoolSpawner[] spawners = FindObjectsByType<BallPoolSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.enabled = false;
            spawner.enabled = true;
        }
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

    private void SetupBubbleFill()
    {
        if (bubbleTimerFillImage == null) return;

        bubbleTimerFillImage.type = Image.Type.Filled;
        bubbleTimerFillImage.fillMethod = Image.FillMethod.Horizontal;
        bubbleTimerFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        bubbleTimerFillImage.fillAmount = 0f;
        bubbleTimerFillImage.raycastTarget = false;
    }

    private void BuildCollectedTexts()
    {
        if (slots == null || slots.Length == 0 || textPrefab == null) return;

        collectedImages = new Image[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            Image img = Instantiate(textPrefab, slots[i]);
            img.name = $"CollectedText_{i}";
            img.transform.localPosition = Vector3.zero;
            img.transform.localRotation = Quaternion.identity;
            img.transform.localScale = Vector3.one;

            img.sprite = GetSpriteForIndex(i);
            img.color = collectedColor;
            img.raycastTarget = false;
            img.gameObject.SetActive(false);

            collectedImages[i] = img;
        }
    }

    private Sprite GetSpriteForIndex(int index)
    {
        if (textSprites == null || textSprites.Length == 0) return null;
        return textSprites[index % textSprites.Length];
    }

    private void HandleScoreChanged(int goodScore, int badScore)
    {
        RefreshCollectedTexts(goodScore);
    }

    private void RefreshCollectedTexts(int goodScore)
    {
        if (collectedImages == null || collectedImages.Length == 0) return;

        int visibleCount = GetWrappedVisibleCount(goodScore, collectedImages.Length);

        for (int i = 0; i < collectedImages.Length; i++)
        {
            if (collectedImages[i] == null) continue;
            collectedImages[i].gameObject.SetActive(i < visibleCount);
        }
    }

    private int GetWrappedVisibleCount(int totalGood, int slotCount)
    {
        if (slotCount <= 0 || totalGood <= 0) return 0;
        int wrapped = totalGood % slotCount;
        return wrapped == 0 ? slotCount : wrapped;
    }

    private void RefreshBubbleTimer(float progress)
    {
        if (bubbleTimerFillImage == null) return;
        bubbleTimerFillImage.fillAmount = progress;
    }

    private void EndRound()
    {
        if (roundEnded) return;

        roundEnded = true;

        CloseAllPlayerInputs();

        if (ScoreMeter.Instance != null)
        {
            if (ScoreMeter.Instance.IsAtMax())
            {
                ShowWinScreen();
            }
            else if (ScoreMeter.Instance.IsAtMin())
            {
                ShowLoseScreen();
            }
            //else
            //{
            //    if (timeOutObject != null)
            //        timeOutObject.SetActive(true);
            //}
        }
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);

            if (winScreenFirstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                winScreenFirstButton.Select();
            }
        }
    }

    private void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);

            if (loseScreenFirstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                loseScreenFirstButton.Select();
            }
        }
    }

    private void HideAllEndScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        //if (timeOutObject != null) timeOutObject.SetActive(false);
    }

    private void OpenAllPlayerInputs()
    {
        foreach (var player in playerInputsToDisable)
        {
            if (player != null)
            {
                player.ActivateInput();
                player.SwitchCurrentActionMap("Player");
            }
        }
    }

    private void CloseAllPlayerInputs()
    {
        foreach (var player in playerInputsToDisable)
        {
            if (player != null)
                player.DeactivateInput();
        }
    }
}