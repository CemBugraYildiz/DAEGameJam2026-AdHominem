using UnityEngine;
using UnityEngine.UI;

public class ScoreMeter : MonoBehaviour
{
    public static ScoreMeter Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Image leftRedBar;
    [SerializeField] private Image rightBlueBar;
    [SerializeField] private RectTransform indicator;

    [Header("Settings")]
    [SerializeField] private float minValue = -100f;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float smoothSpeed = 7f;
    [SerializeField] private float goodValuePerBall = 12f;
    [SerializeField] private float badValuePerBall = 12f;

    private float currentValue = 0f;
    private float targetValue = 0f;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);
        UpdateVisuals();
    }

    public void AddGoodBall()
    {
        targetValue = Mathf.Clamp(targetValue + goodValuePerBall, minValue, maxValue);
    }

    public void AddBadBall()
    {
        targetValue = Mathf.Clamp(targetValue - badValuePerBall, minValue, maxValue);
    }

    private void UpdateVisuals()
    {
        float normalized = (currentValue - minValue) / (maxValue - minValue);

        leftRedBar.fillAmount = normalized;

        rightBlueBar.fillAmount = 1f - normalized;

        if (indicator != null)
        {
            float barWidth = rectTransform.rect.width;
            float xPos = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, normalized);
            Vector2 pos = indicator.anchoredPosition;
            pos.x = xPos;
            indicator.anchoredPosition = pos;
        }
    }

    public float GetCurrentValue() => currentValue;

}