using UnityEngine;
using UnityEngine.InputSystem;

public class SpeechBubbleProgressTimer : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Transform[] slots;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Sprite[] textSprites;

    [Header("Colors")]
    [SerializeField] private Color collectedColor = Color.white;
    [SerializeField] private Color timerColor = new Color(0.4f, 0.9f, 1f, 0.25f);

    [Header("Sorting Orders")]
    [SerializeField] private int timerSortingOrder = 0;
    [SerializeField] private int collectedSortingOrder = 1;

    [Header("Round Timer")]
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private GameObject timeOutObject;
    [SerializeField] private PlayerInput[] playerInputsToDisable;

    private SpriteRenderer[] collectedRenderers;
    private SpriteRenderer[] timerRenderers;

    private float elapsedTime;
    private bool roundEnded;

    private void Awake()
    {
        BuildVisuals();

        if (timeOutObject != null)
            timeOutObject.SetActive(false);

        if (playerInputsToDisable == null || playerInputsToDisable.Length == 0)
            playerInputsToDisable = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        ScoreManager.OnScoreChanged += HandleScoreChanged;

        int currentGoodScore = ScoreManager.Instance != null ? ScoreManager.Instance.GoodScore : 0;
        RefreshCollectedTexts(currentGoodScore);
        RefreshTimerVisual(0f);
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
        RefreshTimerVisual(progress);

        if (progress >= 1f)
        {
            EndRound();
        }
    }

    public void ResetBubbleAndTimer()
    {
        elapsedTime = 0f;
        roundEnded = false;

        if (timeOutObject != null)
            timeOutObject.SetActive(false);

        int currentGoodScore = ScoreManager.Instance != null ? ScoreManager.Instance.GoodScore : 0;
        RefreshCollectedTexts(currentGoodScore);
        RefreshTimerVisual(0f);
    }

    private void BuildVisuals()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("SpeechBubbleProgressTimer: Slot empty");
            return;
        }

        if (textPrefab == null)
        {
            Debug.LogWarning("SpeechBubbleProgressTimer: textPrefab didnt assign");
            return;
        }

        collectedRenderers = new SpriteRenderer[slots.Length];
        timerRenderers = new SpriteRenderer[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            Sprite sprite = GetSpriteForIndex(i);

            timerRenderers[i] = CreateTextInstance(
                $"TimerText_{i}",
                slots[i],
                sprite,
                timerColor,
                timerSortingOrder
            );

            collectedRenderers[i] = CreateTextInstance(
                $"CollectedText_{i}",
                slots[i],
                sprite,
                collectedColor,
                collectedSortingOrder
            );

            if (collectedRenderers[i] != null)
                collectedRenderers[i].gameObject.SetActive(false);
        }
    }

    private SpriteRenderer CreateTextInstance(
        string objectName,
        Transform slot,
        Sprite sprite,
        Color color,
        int sortingOrder)
    {
        GameObject instance = Instantiate(textPrefab, slot);
        instance.name = objectName;

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = instance.GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogWarning($"SpeechBubbleProgressTimer: {objectName} has no sprite renderer");
            return null;
        }

        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        return sr;
    }

    private Sprite GetSpriteForIndex(int index)
    {
        if (textSprites == null || textSprites.Length == 0)
            return null;

        return textSprites[index % textSprites.Length];
    }

    private void HandleScoreChanged(int goodScore, int badScore)
    {
        RefreshCollectedTexts(goodScore);
    }

    private void RefreshCollectedTexts(int goodScore)
    {
        if (collectedRenderers == null || collectedRenderers.Length == 0)
            return;

        int visibleCount = GetWrappedVisibleCount(goodScore, collectedRenderers.Length);

        for (int i = 0; i < collectedRenderers.Length; i++)
        {
            if (collectedRenderers[i] == null) continue;
            collectedRenderers[i].gameObject.SetActive(i < visibleCount);
        }
    }

    private int GetWrappedVisibleCount(int totalGood, int slotCount)
    {
        if (slotCount <= 0 || totalGood <= 0)
            return 0;

        int wrapped = totalGood % slotCount;

        return wrapped == 0 ? slotCount : wrapped;
    }

    private void RefreshTimerVisual(float progress)
    {
        if (timerRenderers == null || timerRenderers.Length == 0)
            return;

        int count = timerRenderers.Length;

        for (int i = 0; i < count; i++)
        {
            if (timerRenderers[i] == null) continue;

            float segmentStart = (float)i / count;
            float segmentEnd = (float)(i + 1) / count;

            float t = Mathf.InverseLerp(segmentStart, segmentEnd, progress);

            Color c = timerColor;
            c.a = timerColor.a * Mathf.Clamp01(t);

            timerRenderers[i].color = c;
        }
    }

    private void EndRound()
    {
        if (roundEnded) return;

        roundEnded = true;

        if (timeOutObject != null)
            timeOutObject.SetActive(true);

        foreach (var player in playerInputsToDisable)
        {
            if (player != null)
                player.DeactivateInput();
        }

        Time.timeScale = 0f;
    }
}