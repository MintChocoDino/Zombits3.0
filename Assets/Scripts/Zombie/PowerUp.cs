using System;
using UnityEngine;

public enum PowerupType
{
    MaxAmmo,
    InstaKill,
    Nuke
}

[RequireComponent(typeof(Collider2D))]
public class Powerup : MonoBehaviour
{
    [Header("Type")]
    public PowerupType type;

    [Header("Lifetime")]
    public float lifetime = 20f;
    public float blinkWarning = 4f;

    [Header("Effect Durations")]
    public float instaKillDuration = 30f;

    private float spawnTime;
    private SpriteRenderer sprite;

    public static bool InstaKillActive { get; private set; }
    private static float instaKillEndTime;

    public static event Action<GameObject> OnNukeActivated;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        spawnTime = Time.time;
    }

    void Update()
    {
        float age = Time.time - spawnTime;

        if (sprite != null && age > lifetime - blinkWarning)
        {
            bool visible = Mathf.FloorToInt(Time.time * 8f) % 2 == 0;
            sprite.enabled = visible;
        }

        if (age >= lifetime)
            Destroy(gameObject);

        if (InstaKillActive && Time.time >= instaKillEndTime)
            InstaKillActive = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerState picker = other.GetComponentInParent<PlayerState>();
        if (picker == null) return;
        Activate(picker);
        Destroy(gameObject);
    }

    private void Activate(PlayerState picker)
    {
        switch (type)
        {
            case PowerupType.MaxAmmo:
                ApplyMaxAmmoToAllPlayers();
                break;
            case PowerupType.InstaKill:
                InstaKillActive = true;
                instaKillEndTime = Time.time + instaKillDuration;
                break;
            case PowerupType.Nuke:
                OnNukeActivated?.Invoke(picker.gameObject);
                break;
        }
    }

    private static void ApplyMaxAmmoToAllPlayers()
    {
        foreach (var player in UnityEngine.Object.FindObjectsByType<PlayerState>(FindObjectsSortMode.None))
        {
            for (int i = 0; i < player.WeaponSlotCount; i++)
            {
                Weapon w = player.GetWeapon(i);
                if (w != null)
                    w.AddReserveAmmo(w.reserveAmmoMax);
            }
        }
    }
}