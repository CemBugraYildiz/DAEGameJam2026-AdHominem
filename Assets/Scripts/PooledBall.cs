using UnityEngine;

public enum BallKind
{
    Good,
    Bad
}

public class PooledBall : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color badBallColor = Color.black;

    private BallPoolSpawner ownerPool;
    private float moveSpeed;
    private Vector3 moveDirection;
    private bool isActive;

    private BallKind kind;
    private PlayerColor targetColor;

    public BallKind Kind => kind;

    public void Initialize(BallPoolSpawner pool)
    {
        ownerPool = pool;
    }

    public void SpawnGood(Vector3 spawnPosition, Vector3 direction, float speed, PlayerColor color, Color visualColor)
    {
        transform.position = spawnPosition;
        moveDirection = direction.normalized;
        moveSpeed = speed;

        kind = BallKind.Good;
        targetColor = color;
        isActive = true;

        if (spriteRenderer != null)
            spriteRenderer.color = visualColor;

        gameObject.SetActive(true);
    }

    public void SpawnBad(Vector3 spawnPosition, Vector3 direction, float speed)
    {
        transform.position = spawnPosition;
        moveDirection = direction.normalized;
        moveSpeed = speed;

        kind = BallKind.Bad;
        isActive = true;

        if (spriteRenderer != null)
            spriteRenderer.color = badBallColor;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive) return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (!other.TryGetComponent<PlayerIdentity>(out var player))
            return;

        ScoreManager.Instance.AddScore(this.Kind);

        if (kind == BallKind.Good)
        {
            if (player.Color != targetColor)
                return;

            Debug.Log($"{player.Color} player collected GOOD ball");


            ReturnToPool();
        }
        else if (kind == BallKind.Bad)
        {
            Debug.Log($"{player.Color} player hit BAD ball");

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