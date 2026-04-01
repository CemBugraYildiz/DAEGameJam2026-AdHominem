using System;
using UnityEngine;
using UnityEngine.UI;

public class BubbleProgressUI : MonoBehaviour
{
    [Header("Text Slots")]
    [SerializeField] private RectTransform[] slots;
    [SerializeField] private Image textPrefab;
    [SerializeField] private Sprite[] textSprites;
    [SerializeField] private Color collectedColor = Color.white;

    [Header("Bubble Timer")]
    [SerializeField] private Image bubbleTimerFillImage;
    [SerializeField] private float duration = 10f;

    private Image[] collectedImages;
    private int currentCount;
    private float elapsedTime;
    private bool timerRunning;

    public event Action<BubbleProgressUI> OnAllSlotsFilled;
    public event Action<BubbleProgressUI> OnTimerExpired;

    public int Capacity => collectedImages != null ? collectedImages.Length : 0;
    public int CurrentCount => currentCount;
    public bool IsFull => Capacity > 0 && currentCount >= Capacity;

    private void Awake()
    {
        BuildCollectedTexts();
        SetupBubbleFill();
    }

    private void Update()
    {
        if (!timerRunning || duration <= 0f)
            return;

        elapsedTime += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsedTime / duration);

        if (bubbleTimerFillImage != null)
            bubbleTimerFillImage.fillAmount = progress;

        if (progress >= 1f)
        {
            timerRunning = false;
            OnTimerExpired?.Invoke(this);
        }
    }

    public void ResetProgress(bool startTimer)
    {
        currentCount = 0;
        elapsedTime = 0f;
        timerRunning = startTimer;

        if (bubbleTimerFillImage != null)
            bubbleTimerFillImage.fillAmount = 0f;

        if (collectedImages != null)
        {
            for (int i = 0; i < collectedImages.Length; i++)
            {
                if (collectedImages[i] != null)
                    collectedImages[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;

        if (bubbleTimerFillImage != null)
            bubbleTimerFillImage.fillAmount = 0f;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void AddStep()
    {
        if (IsFull || collectedImages == null || collectedImages.Length == 0)
            return;

        if (currentCount < collectedImages.Length && collectedImages[currentCount] != null)
            collectedImages[currentCount].gameObject.SetActive(true);

        currentCount++;

        if (IsFull)
        {
            timerRunning = false;
            OnAllSlotsFilled?.Invoke(this);
        }
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
        if (slots == null || slots.Length == 0 || textPrefab == null)
            return;

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
        if (textSprites == null || textSprites.Length == 0)
            return null;

        return textSprites[index % textSprites.Length];
    }
}