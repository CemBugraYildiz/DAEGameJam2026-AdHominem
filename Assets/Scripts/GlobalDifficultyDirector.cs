using UnityEngine;

public class GlobalDifficultyDirector : MonoBehaviour
{
    public static GlobalDifficultyDirector Instance { get; private set; }

    private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    public void ResetDifficulty()
    {
        elapsedTime = 0f;
    }
}