using System;
using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    None,
    Juggernog,
    SpeedCola,
    DoubleTap,
    StaminUp,
    QuickRevive,
}

public class PlayerState : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseRegenPerSecond = 0f;
    [SerializeField] private float regenDelayAfterHit = 3f;

    public float MaxHealth => baseMaxHealth + healthBonus;
    public float Health { get; private set; }
    public float RegenPerSecond => baseRegenPerSecond + regenBonus;

    private float healthBonus;
    private float regenBonus;
    private float lastDamageTime;

    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;

    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;

    private float moveSpeedMultiplier = 1f;

    [Header("Weapon Multipliers")]
    public float FireRateMultiplier { get; private set; } = 1f;
    public float ReloadSpeedMultiplier { get; private set; } = 1f;

    [Header("Economy")]
    [SerializeField] private int startingCredits = 500;
    public int Credits { get; private set; }

    [Header("Weapons")]
    [SerializeField] private int weaponSlotCount = 2;
    private Weapon[] weaponSlots;
    private int activeSlot;

    public int WeaponSlotCount => weaponSlotCount;
    public int ActiveSlot => activeSlot;
    public Weapon ActiveWeapon => weaponSlots[activeSlot];
    public Weapon GetWeapon(int slot) => weaponSlots[slot];

    private readonly HashSet<PerkType> perks = new HashSet<PerkType>();
    public IReadOnlyCollection<PerkType> Perks => perks;
    public bool HasPerk(PerkType perk) => perks.Contains(perk);

    public event Action<float, float> OnHealthChanged;
    public event Action<int> OnCreditsChanged;
    public event Action<PerkType> OnPerkAdded;
    public event Action<PerkType> OnPerkRemoved;
    public event Action<int, Weapon> OnWeaponChanged;
    public event Action<int> OnActiveSlotChanged;
    public event Action OnDeath;

    void Awake()
    {
        weaponSlots = new Weapon[weaponSlotCount];
        Health = MaxHealth;
        Credits = startingCredits;
    }

    void Update()
    {
        if (Health < MaxHealth && Time.time - lastDamageTime >= regenDelayAfterHit && RegenPerSecond > 0f)
        {
            SetHealth(Mathf.Min(MaxHealth, Health + RegenPerSecond * Time.deltaTime));
        }
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        lastDamageTime = Time.time;
        SetHealth(Mathf.Max(0f, Health - amount));
        if (Health <= 0f) Die();
    }

    public void Heal(float amount) => SetHealth(Mathf.Min(MaxHealth, Health + amount));

    private void SetHealth(float v)
    {
        Health = v;
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    public void AddCredits(int amount)
    {
        Credits += amount;
        OnCreditsChanged?.Invoke(Credits);
    }

    public bool TrySpendCredits(int amount)
    {
        if (Credits < amount) return false;
        Credits -= amount;
        OnCreditsChanged?.Invoke(Credits);
        return true;
    }

    public void SetWeapon(int slot, Weapon weapon)
    {
        if (slot < 0 || slot >= weaponSlotCount) return;
        weaponSlots[slot] = weapon;
        OnWeaponChanged?.Invoke(slot, weapon);
    }

    public void SetActiveSlot(int slot)
    {
        if (slot < 0 || slot >= weaponSlotCount) return;
        if (weaponSlots[slot] == null) return;
        activeSlot = slot;
        OnActiveSlotChanged?.Invoke(slot);
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
            if (weaponSlots[i] == null) return i;
        return -1;
    }

    public bool AddPerk(PerkType perk)
    {
        if (!perks.Add(perk)) return false;
        ApplyPerkModifiers(perk, apply: true);
        OnPerkAdded?.Invoke(perk);
        return true;
    }

    public bool RemovePerk(PerkType perk)
    {
        if (!perks.Remove(perk)) return false;
        ApplyPerkModifiers(perk, apply: false);
        OnPerkRemoved?.Invoke(perk);
        return true;
    }

    private void ApplyPerkModifiers(PerkType perk, bool apply)
    {
        float sign = apply ? 1f : -1f;

        switch (perk)
        {
            case PerkType.Juggernog:
                healthBonus += 100f * sign;
                if (apply) Heal(100f);
                else SetHealth(Mathf.Min(Health, MaxHealth));
                break;

            case PerkType.SpeedCola:
                ReloadSpeedMultiplier += 1f * sign;
                break;

            case PerkType.DoubleTap:
                FireRateMultiplier += 0.5f * sign;
                break;

            case PerkType.StaminUp:
                moveSpeedMultiplier += 0.25f * sign;
                break;

            case PerkType.QuickRevive:
                regenBonus += 5f * sign;
                break;
        }

        OnHealthChanged?.Invoke(Health, MaxHealth);
    }
}