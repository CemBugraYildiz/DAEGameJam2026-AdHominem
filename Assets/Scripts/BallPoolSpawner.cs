using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPoolSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private PooledBall goodBallPrefab;
    [SerializeField] private PooledBall badBallPrefab;

    [Header("Players")]
    [SerializeField] private PlayerIdentity[] players;

    [Header("Single Lane")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform laneReference;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Spawn Timing")]
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float minSpawnDelay = 0.75f;
    [SerializeField] private float maxSpawnDelay = 1.5f;

    [Range(0f, 1f)]
    [SerializeField] private float goodBallChance = 0.75f;

    [Header("Pool")]
    [SerializeField] private int initialGoodBallCount = 12;
    [SerializeField] private int initialBadBallCount = 8;
    

    [Header("Ball Colors")]
    [SerializeField] private Color redBallColor = Color.red;
    [SerializeField] private Color blueBallColor = Color.blue;
    [SerializeField] private Color greenBallColor = Color.green;
    [SerializeField] private Color yellowBallColor = Color.yellow;

    private readonly Queue<PooledBall> goodPool = new();
    private readonly Queue<PooledBall> badPool = new();

    private Coroutine spawnRoutine;
    private Transform poolParent;

    private void Awake()
    {
        if (poolParent == null)
        {
            GameObject poolObj = new GameObject("BallPool");
            poolParent = poolObj.transform;
        }

        PrewarmPool(goodBallPrefab, initialGoodBallCount, goodPool);
        PrewarmPool(badBallPrefab, initialBadBallCount, badPool);
    }

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            SpawnRandomBall();

            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnRandomBall()
    {
        List<PlayerIdentity> activePlayers = GetActivePlayers();

        if (activePlayers.Count == 0)
            return;

        Vector3 direction = GetMoveDirection();
        bool spawnGood = Random.value < goodBallChance;

        if (spawnGood)
        {
            PlayerIdentity targetPlayer = activePlayers[Random.Range(0, activePlayers.Count)];
            PooledBall ball = GetBallFromPool(goodPool, goodBallPrefab);

            ball.SpawnGood(
                spawnPoint.position,
                direction,
                moveSpeed,
                targetPlayer.Color,
                GetColorFromPlayerColor(targetPlayer.Color)
            );
        }
        else
        {
            PooledBall ball = GetBallFromPool(badPool, badBallPrefab);
            ball.SpawnBad(spawnPoint.position, direction, moveSpeed);
        }
    }

    private Vector3 GetMoveDirection()
    {
        if (laneReference != null)
            return -laneReference.right;

        return Vector3.left;
    }

    private List<PlayerIdentity> GetActivePlayers()
    {
        List<PlayerIdentity> activePlayers = new();

        foreach (var player in players)
        {
            if (player != null && player.IsJoined)
                activePlayers.Add(player);
        }

        return activePlayers;
    }

    private void PrewarmPool(PooledBall prefab, int count, Queue<PooledBall> pool)
    {
        for (int i = 0; i < count; i++)
        {
            PooledBall ball = Instantiate(prefab, poolParent);
            ball.Initialize(this);
            ball.gameObject.SetActive(false);
            pool.Enqueue(ball);
        }
    }

    private PooledBall GetBallFromPool(Queue<PooledBall> pool, PooledBall prefab)
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        PooledBall newBall = Instantiate(prefab, poolParent);
        newBall.Initialize(this);
        newBall.gameObject.SetActive(false);
        return newBall;
    }

    public void ReturnBall(PooledBall ball)
    {
        ball.transform.SetParent(poolParent);
        ball.gameObject.SetActive(false);

        if (ball.Kind == BallKind.Good)
            goodPool.Enqueue(ball);
        else
            badPool.Enqueue(ball);
    }

    private Color GetColorFromPlayerColor(PlayerColor color)
    {
        return color switch
        {
            PlayerColor.Red => redBallColor,
            PlayerColor.Blue => blueBallColor,
            PlayerColor.Green => greenBallColor,
            PlayerColor.Yellow => yellowBallColor,
            _ => Color.white
        };
    }
}