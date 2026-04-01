using UnityEngine;

public enum ExpressionState
{
    Neutral,
    Winning,
    Losing,
    Hurt,
    Talking
}

public class ExpressionUIController : MonoBehaviour
{
    [SerializeField] private GameObject neutral;
    [SerializeField] private GameObject winning;
    [SerializeField] private GameObject losing;
    [SerializeField] private GameObject hurt;
    [SerializeField] private GameObject talking;

    public void Show(ExpressionState state)
    {
        if (neutral != null) neutral.SetActive(state == ExpressionState.Neutral);
        if (winning != null) winning.SetActive(state == ExpressionState.Winning);
        if (losing != null) losing.SetActive(state == ExpressionState.Losing);
        if (hurt != null) hurt.SetActive(state == ExpressionState.Hurt);
        if (talking != null) talking.SetActive(state == ExpressionState.Talking);
    }
}