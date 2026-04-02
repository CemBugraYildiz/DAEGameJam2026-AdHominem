using UnityEngine;
using UnityEngine.UI;

public class ScoreMeter : MonoBehaviour
{
    public static ScoreMeter Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform fillTrack;
    [SerializeField] private Image redBar;
    [SerializeField] private Image blueBar;
    [SerializeField] private RectTransform centerMarker;

    [Header("Settings")]
    [SerializeField] private float minValue = -100f;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float smoothSpeed = 7f;
    [SerializeField] private float goodValuePerBall = 12f;
    [SerializeField] private float badValuePerBall = 12f;

    private float currentValue = 0f;
    private float targetValue = 0f;

    private readonly Vector3[] corners = new Vector3[4];

    public bool IsAtMax() => Mathf.Approximately(currentValue, maxValue);
    public bool IsAtMin() => Mathf.Approximately(currentValue, minValue);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);

        if (Mathf.Abs(currentValue - targetValue) < 0.01f)
            currentValue = targetValue;

        UpdateVisuals();
    }

    public void AddGoodBall()
    {
        AddReward(goodValuePerBall);
    }

    public void AddBadBall()
    {
        AddPenalty(badValuePerBall);
    }

    private void UpdateVisuals()
    {
        float normalized = Mathf.InverseLerp(minValue, maxValue, currentValue);

        if (redBar != null)
            redBar.fillAmount = normalized;

        if (blueBar != null)
            blueBar.fillAmount = 1f - normalized;

        if (fillTrack != null && centerMarker != null)
        {
            fillTrack.GetWorldCorners(corners);

            Vector3 leftMid = (corners[0] + corners[1]) * 0.5f;
            Vector3 rightMid = (corners[3] + corners[2]) * 0.5f;

            float targetX = Mathf.Lerp(leftMid.x, rightMid.x, normalized);

            Vector3 pos = centerMarker.position;
            pos.x = targetX;
            centerMarker.position = pos;
        }
    }

    public float GetCurrentValue() => currentValue;

    public float GetNormalized01()
    {
        return Mathf.InverseLerp(minValue, maxValue, currentValue);
    }

    public bool IsInLeftThird()
    {
        return GetNormalized01() < 1f / 3f;
    }

    public bool IsInCenterThird()
    {
        float n = GetNormalized01();
        return n >= 1f / 3f && n < 2f / 3f;
    }

    public bool IsInRightThird()
    {
        return GetNormalized01() >= 2f / 3f;
    }

    public void ResetMeter()
    {
        currentValue = 0f;
        targetValue = 0f;
        UpdateVisuals();
    }

    public void AddValue(float amount)
    {
        targetValue = Mathf.Clamp(targetValue + amount, minValue, maxValue);
    }

    public void AddPenalty(float amount)
    {
        targetValue = Mathf.Clamp(targetValue - Mathf.Abs(amount), minValue, maxValue);
    }

    public void AddReward(float amount)
    {
        targetValue = Mathf.Clamp(targetValue + Mathf.Abs(amount), minValue, maxValue);
    }
}