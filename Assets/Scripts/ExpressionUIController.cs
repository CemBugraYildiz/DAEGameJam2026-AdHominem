using UnityEngine;

public enum ExpressionState
{
    Neutral,
    Winning,
    Losing,
    Hurt,
    NotHurt,
    Talking
}

public class ExpressionUIController : MonoBehaviour
{
    [Header("Expressions")]
    [SerializeField] private GameObject neutral;
    [SerializeField] private GameObject winning;
    [SerializeField] private GameObject losing;
    [SerializeField] private GameObject hurt;
    [SerializeField] private GameObject notHurt;
    [SerializeField] private GameObject talking;

    [Header("Backgrounds")]
    [SerializeField] private GameObject angryBackground;
    [SerializeField] private GameObject happyBackground;
    [SerializeField] private GameObject defeatedBackground;

    public void Show(ExpressionState state)
    {
        if (neutral != null) neutral.SetActive(state == ExpressionState.Neutral);
        if (winning != null) winning.SetActive(state == ExpressionState.Winning);
        if (losing != null) losing.SetActive(state == ExpressionState.Losing);
        if (hurt != null) hurt.SetActive(state == ExpressionState.Hurt);
        if (notHurt != null) notHurt.SetActive(state == ExpressionState.NotHurt);
        if (talking != null) talking.SetActive(state == ExpressionState.Talking);

        UpdateBackground(state);
    }

    private void UpdateBackground(ExpressionState state)
    {
        if (angryBackground != null)
            angryBackground.SetActive(state == ExpressionState.Losing);

        if (happyBackground != null)
            happyBackground.SetActive(
                state == ExpressionState.NotHurt ||
                state == ExpressionState.Talking
            );

        if (defeatedBackground != null)
            defeatedBackground.SetActive(state == ExpressionState.Hurt);
    }
}