using UnityEngine;
using UnityEngine.SceneManagement;

public enum BallType
{
    Good,
    Bad
}

public class PooledBall : MonoBehaviour
{
    [SerializeField] private BallType ballType;

    private BallPoolSpawner ownerPool;
    private float moveSpeed;
    private bool isActive;

    public BallType Type => ballType;

    public void Initialize(BallPoolSpawner pool)
    {
        ownerPool = pool;
    }

    public void Spawn(Vector3 spawnPosition, float speed)
    {
        transform.position = spawnPosition;
        moveSpeed = speed;
        isActive = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive) return;

        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            ScoreManager.Instance.AddScore(ballType);
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (!isActive) return;

        isActive = false;
        ownerPool.ReturnBall(this);
    }

    private void OnDisable()
    {
        isActive = false;
    }
}