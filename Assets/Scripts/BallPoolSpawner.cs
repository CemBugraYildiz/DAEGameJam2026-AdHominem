using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPoolSpawner : MonoBehaviour
{
    [Header("Ball Prefabs")]
    [SerializeField] private PooledBall goodBallPrefab;
    [SerializeField] private PooledBall badBallPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialGoodBallCount = 10;
    [SerializeField] private int initialBadBallCount = 10;
    [SerializeField] private Transform poolParent;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float minSpawnDelay = 0.75f;
    [SerializeField] private float maxSpawnDelay = 1.5f;
    [SerializeField] private float moveSpeed = 4f;

    [Range(0f, 1f)]
    [SerializeField] private float goodBallChance = 0.5f;

    private readonly Queue<PooledBall> goodPool = new();
    private readonly Queue<PooledBall> badPool = new();

    private Coroutine spawnRoutine;

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
        bool spawnGood = Random.value < goodBallChance;

        if (spawnGood)
        {
            PooledBall ball = GetBallFromPool(goodPool);
            if (ball != null)
                ball.Spawn(spawnPoint.position, moveSpeed);
        }
        else
        {
            PooledBall ball = GetBallFromPool(badPool);
            if (ball != null)
                ball.Spawn(spawnPoint.position, moveSpeed);
        }
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

    private PooledBall GetBallFromPool(Queue<PooledBall> pool)
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("Pool empty");
            return null;
        }

        return pool.Dequeue();
    }

    public void ReturnBall(PooledBall ball)
    {
        ball.transform.SetParent(poolParent);
        ball.gameObject.SetActive(false);

        if (ball.Type == BallType.Good)
            goodPool.Enqueue(ball);
        else
            badPool.Enqueue(ball);
    }
}