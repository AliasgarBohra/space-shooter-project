using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private int minRows = 2;
    [SerializeField] private int maxRows = 5;

    [SerializeField] private int minColumns = 3;
    [SerializeField] private int maxColumns = 8;

    [SerializeField] private float waveInterval = 5f;

    [Header("Grid Settings")]
    [SerializeField] private float startX = 0f;
    [SerializeField] private float horizontalSpacing = 2f;
    [SerializeField] private float verticalSpacing = 1.5f;

    private float nextWaveTime;
    private float endTime;
    private bool hasEnded = false;
    private bool startWaves = false;

    private System.Random rng;

    public void StartWaves(int duration, int seed)
    {
        rng = new System.Random(seed);
        nextWaveTime = Time.time + 1;
        endTime = Time.time + duration - 5;
        hasEnded = false;
        startWaves = true;
    }

    public void StopWaves()
    {
        hasEnded = true;
    }

    private void Update()
    {
        if (!startWaves || hasEnded || GameplayHandler.Instance.isGameEnded)
            return;

        if (Time.time >= nextWaveTime && Time.time < endTime)
        {
            SpawnWave();
            nextWaveTime = Time.time + waveInterval;
        }
        if (!hasEnded && Time.time >= endTime)
        {
            StopWaves();
        }
    }

    private void SpawnWave()
    {
        int rows = rng.Next(minRows, maxRows + 1);
        int columns = rng.Next(minColumns, maxColumns + 1);

        float totalWidth = (columns - 1) * horizontalSpacing;
        float startXPos = startX - totalWidth / 2f;

        float totalHeight = (rows - 1) * verticalSpacing;
        float startY = totalHeight / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 spawnPos = new Vector3(
                    startXPos + c * horizontalSpacing,
                    startY - r * verticalSpacing,
                    0f);

                Instantiate(enemyPrefab, spawnPos, enemyPrefab.transform.rotation);
            }
        }
    }
}