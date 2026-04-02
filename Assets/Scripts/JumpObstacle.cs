using UnityEngine;

public class JumpObstacle : MonoBehaviour
{
    private JumpObstacleSpawner ownerPool;
    private float moveSpeed;
    private Vector3 moveDirection;
    private bool isActive;

    [SerializeField] private AudioClip[] playerhitClips;
    [SerializeField] private float playerhitSoundVolume = 1f;

    public void Initialize(JumpObstacleSpawner pool)
    {
        ownerPool = pool;
    }

    public void Spawn(Vector3 spawnPosition, Vector3 direction, float speed)
    {
        transform.position = spawnPosition;
        moveDirection = direction.normalized;
        moveSpeed = speed;
        isActive = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive) return;

        float currentSpeed = moveSpeed;

        if (ownerPool != null)
            currentSpeed = ownerPool.GetCurrentMoveSpeed();

        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        PlayerIdentity player = other.GetComponent<PlayerIdentity>();

        if (player == null)
            player = other.GetComponentInParent<PlayerIdentity>();

        if (player == null)
            return;

        if (!player.IsJoined)
            return;

        if (SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(playerhitClips, playerhitSoundVolume);
        }


        GameFlowManager.Instance?.ReportJumpObstacleHit();
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (!isActive) return;

        isActive = false;
        ownerPool.ReturnObstacle(this);
    }

    private void OnDisable()
    {
        isActive = false;
    }
}