using UnityEngine;

public class BallReturnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PooledBall>(out var ball))
        {
            ball.ReturnToPool();
        }
    }
}