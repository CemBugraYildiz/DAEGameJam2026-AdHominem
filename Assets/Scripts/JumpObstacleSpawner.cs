using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpObstacleSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private JumpObstacle obstaclePrefab;

    [Header("Lane")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform laneReference;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Spawn Timing")]
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float minSpawnDelay = 1f;
    [SerializeField] private float maxSpawnDelay = 2f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float targetMoveSpeed = 8f;
    [SerializeField] private float targetMinSpawnDelay = 0.4f;
    [SerializeField] private float targetMaxSpawnDelay = 1.0f;
    [SerializeField] private float difficultyRampDuration = 60f;

    [Header("Pool")]
    [SerializeField] private int initialCount = 10;

    private readonly Queue<JumpObstacle> pool = new();
    private readonly List<JumpObstacle> allObstacles = new();

    private Coroutine spawnRoutine;
    private Transform poolParent;

    private void Awake()
    {
        if (poolParent == null)
        {
            GameObject poolObj = new GameObject("JumpObstaclePool");
            poolParent = poolObj.transform;
            poolParent.SetParent(transform, false);
        }

        PrewarmPool();
    }

    public void StartSpawning()
    {
        StopAndClearAll();
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopAndClearAll()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        foreach (var obstacle in allObstacles)
        {
            if (obstacle != null && obstacle.gameObject.activeSelf)
                obstacle.ReturnToPool();
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            float difficulty01 = GetDifficulty01();

            float currentMoveSpeed = Mathf.Lerp(moveSpeed, targetMoveSpeed, difficulty01);
            float currentMinDelay = Mathf.Lerp(minSpawnDelay, targetMinSpawnDelay, difficulty01);
            float currentMaxDelay = Mathf.Lerp(maxSpawnDelay, targetMaxSpawnDelay, difficulty01);

            SpawnObstacle(currentMoveSpeed);

            float wait = Random.Range(currentMinDelay, currentMaxDelay);
            yield return new WaitForSeconds(wait);
        }
    }

    private void SpawnObstacle(float speed)
    {
        Vector3 direction = laneReference != null ? -laneReference.right : Vector3.left;
        JumpObstacle obstacle = GetFromPool();
        obstacle.Spawn(spawnPoint.position, direction, speed);
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < initialCount; i++)
        {
            JumpObstacle obstacle = Instantiate(obstaclePrefab, poolParent);
            obstacle.Initialize(this);
            obstacle.gameObject.SetActive(false);
            pool.Enqueue(obstacle);
            allObstacles.Add(obstacle);
        }
    }

    private JumpObstacle GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        JumpObstacle obstacle = Instantiate(obstaclePrefab, poolParent);
        obstacle.Initialize(this);
        obstacle.gameObject.SetActive(false);
        allObstacles.Add(obstacle);
        return obstacle;
    }

    public void ReturnObstacle(JumpObstacle obstacle)
    {
        obstacle.transform.SetParent(poolParent);
        obstacle.gameObject.SetActive(false);
        pool.Enqueue(obstacle);
    }

    private float GetDifficulty01()
    {
        if (difficultyRampDuration <= 0f)
            return 1f;

        float elapsed = 0f;

        if (GlobalDifficultyDirector.Instance != null)
            elapsed = GlobalDifficultyDirector.Instance.ElapsedTime;

        return Mathf.Clamp01(elapsed / difficultyRampDuration);
    }

    public float GetCurrentMoveSpeed()
    {
        float difficulty01 = GetDifficulty01();
        return Mathf.Lerp(moveSpeed, targetMoveSpeed, difficulty01);
    }
}