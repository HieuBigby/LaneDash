using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private List<Transform> _spawnLanes; // The 3 pole positions for spawning
    
    [Header("Spawn Timing - Starting Values")]
    [SerializeField] private float _spawnIntervalMin = 2f;
    [SerializeField] private float _spawnIntervalMax = 3f;
    
    [Header("Movement Settings - Starting Values")]
    [SerializeField] private float _moveSpeedMin = 3f;
    [SerializeField] private float _moveSpeedMax = 6f;
    [SerializeField] private float _spawnOffsetAboveScreen = 2f;
    
    [Header("Difficulty Progression")]
    [SerializeField] private float _difficultyIncreaseInterval = 10f; // Increase difficulty every X seconds
    [SerializeField] private float _spawnIntervalDecreaseRate = 0.05f; // How much to decrease spawn interval per step
    [SerializeField] private float _minSpawnInterval = 0.5f; // Minimum time between spawns
    [SerializeField] private float _speedIncreaseRate = 0.2f; // How much to increase speed per step
    [SerializeField] private float _maxObstacleSpeed = 12f; // Maximum obstacle speed
    [SerializeField] private bool _enableDifficultyScaling = true;
    
    [Header("Safe Pattern Generation")]
    [SerializeField] private int _preventSameLaneConsecutive = 2; // Prevent X obstacles in same lane consecutively
    [SerializeField] private float _minGapBetweenObstacles = 1.5f; // Minimum time to guarantee a gap
    
    private Camera mainCamera;
    private float screenTopY;
    private float screenBottomY;
    private List<ObstacleController> activeObstacles = new List<ObstacleController>();
    private bool isSpawning = false;
    
    // Dynamic difficulty values
    private float currentSpawnIntervalMin;
    private float currentSpawnIntervalMax;
    private float currentMoveSpeedMin;
    private float currentMoveSpeedMax;
    private float gameTime = 0f;
    
    // Safe pattern tracking
    private List<int> recentLanes = new List<int>();

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
        
        // Initialize difficulty values
        currentSpawnIntervalMin = _spawnIntervalMin;
        currentSpawnIntervalMax = _spawnIntervalMax;
        currentMoveSpeedMin = _moveSpeedMin;
        currentMoveSpeedMax = _moveSpeedMax;
        
        StartCoroutine(SpawnObstacles());
        
        if (_enableDifficultyScaling)
        {
            StartCoroutine(IncreaseDifficulty());
        }
    }
    
    private void Update()
    {
        gameTime += Time.deltaTime;
    }

    private void CalculateScreenBounds()
    {
        screenTopY = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        screenBottomY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
    }

    private IEnumerator SpawnObstacles()
    {
        isSpawning = true;
        
        while (isSpawning)
        {
            // Pick a safe lane (avoid same lane repeatedly)
            int laneIndex = GetSafeLaneIndex();
            Transform spawnLane = _spawnLanes[laneIndex];
            
            // Track recent lanes for pattern safety
            recentLanes.Add(laneIndex);
            if (recentLanes.Count > _preventSameLaneConsecutive)
            {
                recentLanes.RemoveAt(0);
            }
            
            // Spawn position above screen
            float spawnY = screenTopY + _spawnOffsetAboveScreen;
            Vector3 spawnPosition = new Vector3(spawnLane.position.x, spawnY, spawnLane.position.z);
            
            // Create obstacle
            GameObject obstacleObj = Instantiate(_obstaclePrefab, spawnPosition, Quaternion.identity, transform);
            ObstacleController obstacle = obstacleObj.GetComponent<ObstacleController>();
            
            if (obstacle != null)
            {
                float speed = Random.Range(currentMoveSpeedMin, currentMoveSpeedMax);
                obstacle.Initialize(speed, screenBottomY, this);
                activeObstacles.Add(obstacle);
            }
            
            // Wait before spawning next obstacle (use current difficulty-adjusted values)
            float waitTime = Random.Range(currentSpawnIntervalMin, currentSpawnIntervalMax);
            
            // Ensure minimum gap for playability
            waitTime = Mathf.Max(waitTime, _minGapBetweenObstacles);
            
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private int GetSafeLaneIndex()
    {
        // If we haven't spawned enough obstacles yet, just pick random
        if (recentLanes.Count < _preventSameLaneConsecutive)
        {
            return Random.Range(0, _spawnLanes.Count);
        }
        
        // Check if all recent lanes are the same
        bool allSameLane = true;
        int lastLane = recentLanes[recentLanes.Count - 1];
        
        for (int i = 0; i < recentLanes.Count; i++)
        {
            if (recentLanes[i] != lastLane)
            {
                allSameLane = false;
                break;
            }
        }
        
        // If all recent obstacles in same lane, force different lane
        if (allSameLane)
        {
            List<int> availableLanes = new List<int>();
            for (int i = 0; i < _spawnLanes.Count; i++)
            {
                if (i != lastLane)
                {
                    availableLanes.Add(i);
                }
            }
            return availableLanes[Random.Range(0, availableLanes.Count)];
        }
        
        // Otherwise, random lane
        return Random.Range(0, _spawnLanes.Count);
    }
    
    private IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(_difficultyIncreaseInterval);
            
            // Decrease spawn interval (spawn faster)
            currentSpawnIntervalMin = Mathf.Max(_minSpawnInterval, currentSpawnIntervalMin - _spawnIntervalDecreaseRate);
            currentSpawnIntervalMax = Mathf.Max(_minSpawnInterval + 0.5f, currentSpawnIntervalMax - _spawnIntervalDecreaseRate);
            
            // Increase speed
            currentMoveSpeedMin = Mathf.Min(_maxObstacleSpeed, currentMoveSpeedMin + _speedIncreaseRate);
            currentMoveSpeedMax = Mathf.Min(_maxObstacleSpeed + 2f, currentMoveSpeedMax + _speedIncreaseRate);
            
            Debug.Log($"Difficulty increased! Time: {gameTime:F1}s | Spawn Interval: {currentSpawnIntervalMin:F2}-{currentSpawnIntervalMax:F2}s | Speed: {currentMoveSpeedMin:F1}-{currentMoveSpeedMax:F1}");
        }
    }

    public void RemoveObstacle(ObstacleController obstacle)
    {
        if (activeObstacles.Contains(obstacle))
        {
            activeObstacles.Remove(obstacle);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnObstacles());
        }
    }
}
