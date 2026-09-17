using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZombieSpawner : MonoBehaviour
{
    [Header("References")]
    public Tilemap floorTilemap;
    public TileBase zombieSpawnTile;
    public GameObject zombiePrefab;

    [Header("Spawn Rules")]
    public float spawnRange = 15f;
    public float spawnInterval = 3f;
    public int maxZombies = 20;
    public int zombiesPerInterval = 1;

    [Header("Startup")]
    public float startupDelay = 0.5f;

    private readonly List<Vector3Int> spawnCells = new List<Vector3Int>();
    private Transform player;
    private float nextSpawnTime;
    private int liveZombies;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        nextSpawnTime = Time.time + startupDelay;
        Invoke(nameof(CacheSpawnTiles), startupDelay);
    }

    private void CacheSpawnTiles()
    {
        spawnCells.Clear();
        BoundsInt bounds = floorTilemap.cellBounds;
        foreach (var cell in bounds.allPositionsWithin)
        {
            if (floorTilemap.GetTile(cell) == zombieSpawnTile)
                spawnCells.Add(cell);
        }
    }

    void Update()
    {
        if (player == null || spawnCells.Count == 0) return;
        if (Time.time < nextSpawnTime) return;
        if (liveZombies >= maxZombies) return;

        int waveBudget = WaveManager.Instance != null
            ? WaveManager.Instance.GetSpawnBudget(liveZombies)
            : int.MaxValue;
        if (waveBudget <= 0) return;

        nextSpawnTime = Time.time + spawnInterval;

        List<Vector3Int> eligible = new List<Vector3Int>();
        foreach (var cell in spawnCells)
        {
            Vector3 world = floorTilemap.GetCellCenterWorld(cell);
            if (Vector2.Distance(world, player.position) > spawnRange) continue;
            if (GridPathfinder.Instance == null) continue;
            if (GridPathfinder.Instance.FindPath(world, player.position) == null) continue;

            eligible.Add(cell);
        }

        int toSpawn = Mathf.Min(zombiesPerInterval, eligible.Count, maxZombies - liveZombies, waveBudget);
        for (int i = 0; i < toSpawn; i++)
        {
            Vector3Int cell = eligible[Random.Range(0, eligible.Count)];
            Vector3 pos = floorTilemap.GetCellCenterWorld(cell);
            GameObject z = Instantiate(zombiePrefab, pos, Quaternion.identity);

            if (WaveManager.Instance != null && z.TryGetComponent<Zombie>(out var zombie))
            {
                zombie.moveSpeed = WaveManager.Instance.CurrentZombieSpeed;
                zombie.attackDamage = WaveManager.Instance.CurrentZombieDamage;
                zombie.maxHealth = WaveManager.Instance.CurrentZombieHealth;
                zombie.ResetHealth();
            }

            var tracker = z.AddComponent<ZombieDeathNotifier>();
            tracker.spawner = this;
            liveZombies++;
        }
    }

    public void OnZombieKilled()
    {
        liveZombies = Mathf.Max(0, liveZombies - 1);
        if (WaveManager.Instance != null)
            WaveManager.Instance.NotifyZombieKilled();
    }
}

public class ZombieDeathNotifier : MonoBehaviour
{
    [HideInInspector] public ZombieSpawner spawner;

    void OnDestroy()
    {
        if (spawner != null) spawner.OnZombieKilled();
    }
}