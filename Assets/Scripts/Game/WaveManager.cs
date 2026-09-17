using System;
using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Timing")]
    public float initialDelay = 3f;

    [Header("Zombies Per Wave")]
    public int baseZombiesPerWave = 6;
    public int zombiesAddedPerWave = 4;

    [Header("Zombie Damage Scaling")]
    public float baseDamage = 10f;
    public float damagePerWave = 2f;
    public float maxDamage = 40f;

    [Header("Zombie Speed Scaling")]
    public float baseSpeed = 2f;
    public float speedPerWave = 0.15f;
    public float maxSpeed = 5f;

    [Header("Zombie Health Scaling")]
    public float baseHealth = 100f;
    public float healthPerWave = 30f;
    public float maxHealth = 600f;

    public int CurrentWave { get; private set; }
    public int ZombiesRemainingThisWave { get; private set; }
    public bool WaveInProgress { get; private set; }

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveEnded;

    public float CurrentZombieDamage => Mathf.Min(baseDamage + damagePerWave * (CurrentWave - 1), maxDamage);
    public float CurrentZombieSpeed => Mathf.Min(baseSpeed + speedPerWave * (CurrentWave - 1), maxSpeed);
    public float CurrentZombieHealth => Mathf.Min(baseHealth + healthPerWave * (CurrentWave - 1), maxHealth);
    public int ZombiesForCurrentWave => baseZombiesPerWave + zombiesAddedPerWave * (CurrentWave - 1);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            StartWave(CurrentWave + 1);

            while (WaveInProgress)
                yield return null;

            yield return null;
        }
    }

    private void StartWave(int wave)
    {
        CurrentWave = wave;
        ZombiesRemainingThisWave = ZombiesForCurrentWave;
        WaveInProgress = true;
        MainMenu.RecordHighestWave(wave);
        OnWaveStarted?.Invoke(CurrentWave);
    }

    public void NotifyZombieKilled()
    {
        if (!WaveInProgress) return;
        ZombiesRemainingThisWave--;
        if (ZombiesRemainingThisWave <= 0)
            EndWave();
    }

    private void EndWave()
    {
        WaveInProgress = false;
        OnWaveEnded?.Invoke(CurrentWave);
    }

    public int GetSpawnBudget(int currentlyAlive)
    {
        return Mathf.Max(0, ZombiesRemainingThisWave - currentlyAlive);
    }
}