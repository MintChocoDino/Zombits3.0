using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Zombie : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 0.8f;
    public float attackCooldown = 1.0f;

    [Header("Rewards")]
    [Tooltip("Flat bonus paid to whoever lands the killing blow.")]
    public int creditsOnKill = 50;
    [Tooltip("Credits paid per point of damage dealt, so income scales with zombie health.")]
    public float creditsPerDamage = 0.1f;

    [Header("Powerup Drops")]
    [Range(0f, 1f)]
    public float powerupDropChance = 0.02f;
    public Powerup[] possiblePowerups;

    [Header("Audio")]
    public AudioClip[] idleSounds;
    public Vector2 idleSoundInterval = new Vector2(4f, 9f);
    public AudioClip deathSound;
    public AudioClip attackSound;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Pathfinding")]
    public float repathInterval = 0.4f;
    public float waypointReachedDistance = 0.15f;

    private Transform player;
    private Rigidbody2D rb;
    private float health;
    private float nextRepathTime;
    private float nextAttackTime;
    private float nextIdleSoundTime;
    private List<Vector3> path;
    private int pathIndex;
    private GameObject lastAttacker;
    private float creditFraction;
    private bool dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
    }

    public void ResetHealth()
    {
        health = maxHealth;
        creditFraction = 0f;
        dead = false;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        Powerup.OnNukeActivated += HandleNuke;
        nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval.x, idleSoundInterval.y);
    }

    void OnDestroy()
    {
        Powerup.OnNukeActivated -= HandleNuke;
    }

    private void HandleNuke(GameObject picker)
    {
        TakeDamage(maxHealth * 999f, picker);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (idleSounds != null && idleSounds.Length > 0 && Time.time >= nextIdleSoundTime)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRandomAt(idleSounds, transform.position, sfxVolume);
            nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval.x, idleSoundInterval.y);
        }

        if (Time.time >= nextRepathTime)
        {
            path = GridPathfinder.Instance?.FindPath(transform.position, player.position);
            pathIndex = 0;
            nextRepathTime = Time.time + repathInterval;

            if (path != null && path.Count >= 2)
            {
                Vector2 toFirst = (Vector2)path[0] - (Vector2)transform.position;
                Vector2 firstToSecond = (Vector2)path[1] - (Vector2)path[0];
                if (Vector2.Dot(toFirst, firstToSecond) < 0f)
                    pathIndex = 1;
            }
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack();
            return;
        }

        Move();
    }

    private void Move()
    {
        if (path == null || pathIndex >= path.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 target = path[pathIndex];
        Vector2 toTarget = (Vector2)(target - transform.position);

        if (toTarget.magnitude <= waypointReachedDistance)
        {
            pathIndex++;
            return;
        }

        rb.linearVelocity = toTarget.normalized * moveSpeed;
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        if (attackSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayAt(attackSound, transform.position, sfxVolume);

        if (player.TryGetComponent<IDamageable>(out var target))
            target.TakeDamage(attackDamage, gameObject);
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (dead) return;
        if (source != null) lastAttacker = source;

        if (Powerup.InstaKillActive && source != null && source.GetComponent<PlayerState>() != null)
            amount = maxHealth;

        float applied = Mathf.Min(amount, Mathf.Max(0f, health));
        health -= amount;

        if (source != null && source.TryGetComponent<PlayerState>(out var damager))
            AwardDamageCredits(damager, applied);

        if (health <= 0f)
            Die();
    }

    private void AwardDamageCredits(PlayerState player, float damageDealt)
    {
        if (damageDealt <= 0f || creditsPerDamage <= 0f) return;

        creditFraction += damageDealt * creditsPerDamage;
        int whole = Mathf.FloorToInt(creditFraction);
        if (whole <= 0) return;

        creditFraction -= whole;
        player.AddCredits(whole);
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        if (deathSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayAt(deathSound, transform.position, sfxVolume);

        if (lastAttacker != null && lastAttacker.TryGetComponent<PlayerState>(out var state))
            state.AddCredits(creditsOnKill);

        if (possiblePowerups != null && possiblePowerups.Length > 0
            && Random.value < powerupDropChance)
        {
            Powerup prefab = possiblePowerups[Random.Range(0, possiblePowerups.Length)];
            if (prefab != null)
                Instantiate(prefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}