using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Player")]
    public PlayerState playerState;

    [Header("Weapon Slots")]
    public Image[] slotBackgrounds;
    public Image[] slotIcons;
    public Color activeSlotColor = new Color(1f, 0.85f, 0.2f, 1f);
    public Color inactiveSlotColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    public Color emptyIconColor = new Color(1f, 1f, 1f, 0f);
    public Color filledIconColor = Color.white;

    [Header("Ammo")]
    public TMP_Text ammoText;

    [Header("Credits")]
    public TMP_Text creditsText;

    [Header("Health")]
    public Image healthFill;
    public TMP_Text healthText;

    [Header("Perks")]
    public Transform perksPanel;
    public Image perkIconPrefab;
    public PerkIcon[] perkIcons;

    [Header("Wave Banner")]
    public TMP_Text waveText;
    public GameObject waveBanner;
    public float waveBannerDuration = 2.5f;

    [Header("Wave Counter")]
    public TMP_Text waveCounterText;
    public TMP_Text zombiesRemainingText;

    private Dictionary<PerkType, Sprite> perkSpriteLookup;
    private Dictionary<PerkType, Image> activePerkIcons;

    void OnEnable()
    {
        if (playerState == null) return;

        playerState.OnHealthChanged += HandleHealthChanged;
        playerState.OnCreditsChanged += HandleCreditsChanged;
        playerState.OnWeaponChanged += HandleWeaponChanged;
        playerState.OnActiveSlotChanged += HandleActiveSlotChanged;
        playerState.OnPerkAdded += HandlePerkAdded;
        playerState.OnPerkRemoved += HandlePerkRemoved;
    }

    void OnDisable()
    {
        if (playerState == null) return;
        playerState.OnHealthChanged -= HandleHealthChanged;
        playerState.OnCreditsChanged -= HandleCreditsChanged;
        playerState.OnWeaponChanged -= HandleWeaponChanged;
        playerState.OnActiveSlotChanged -= HandleActiveSlotChanged;
        playerState.OnPerkAdded -= HandlePerkAdded;
        playerState.OnPerkRemoved -= HandlePerkRemoved;
    }

    void Start()
    {
        if (playerState == null) return;

        perkSpriteLookup = new Dictionary<PerkType, Sprite>();
        if (perkIcons != null)
            foreach (var entry in perkIcons)
                perkSpriteLookup[entry.perk] = entry.icon;
        activePerkIcons = new Dictionary<PerkType, Image>();

        HandleHealthChanged(playerState.Health, playerState.MaxHealth);
        HandleCreditsChanged(playerState.Credits);
        for (int i = 0; i < playerState.WeaponSlotCount; i++)
            HandleWeaponChanged(i, playerState.GetWeapon(i));
        HandleActiveSlotChanged(playerState.ActiveSlot);

        foreach (var perk in playerState.Perks)
            HandlePerkAdded(perk);

        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
        if (waveBanner != null) waveBanner.SetActive(false);
    }

    void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveStarted -= HandleWaveStarted;
    }

    void Update()
    {
        Weapon w = playerState != null ? playerState.ActiveWeapon : null;
        if (w != null)
        {
            ammoText.text = w.IsReloading
                ? $"-- / {w.ReserveAmmo}"
                : $"{w.CurrentAmmo} / {w.ReserveAmmo}";
        }
        else
        {
            ammoText.text = "";
        }

        if (zombiesRemainingText != null && WaveManager.Instance != null)
        {
            int remaining = WaveManager.Instance.ZombiesRemainingThisWave;
            zombiesRemainingText.text = $"Zombies: {remaining}";
        }
    }

    private void HandleWaveStarted(int wave)
    {
        if (waveText != null) waveText.text = $"Wave {wave}";
        if (waveCounterText != null) waveCounterText.text = $"Wave {wave}";
        if (waveBanner != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashWaveBanner());
        }
    }

    private IEnumerator FlashWaveBanner()
    {
        waveBanner.SetActive(true);
        yield return new WaitForSeconds(waveBannerDuration);
        waveBanner.SetActive(false);
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (healthFill != null)
            healthFill.fillAmount = max > 0 ? current / max : 0f;
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void HandleCreditsChanged(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"${credits}";
    }

    private void HandleWeaponChanged(int slot, Weapon weapon)
    {
        if (slotIcons == null || slot < 0 || slot >= slotIcons.Length) return;
        if (slotIcons[slot] == null) return;

        if (weapon != null && weapon.icon != null)
        {
            slotIcons[slot].sprite = weapon.icon;
            slotIcons[slot].color = filledIconColor;
        }
        else
        {
            slotIcons[slot].sprite = null;
            slotIcons[slot].color = emptyIconColor;
        }
    }

    private void HandleActiveSlotChanged(int active)
    {
        if (slotBackgrounds == null) return;
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;
            slotBackgrounds[i].color = (i == active) ? activeSlotColor : inactiveSlotColor;
        }
    }

    private void HandlePerkAdded(PerkType perk)
    {
        if (perksPanel == null || perkIconPrefab == null) return;
        if (activePerkIcons.ContainsKey(perk)) return;

        Image icon = Instantiate(perkIconPrefab, perksPanel);
        if (perkSpriteLookup.TryGetValue(perk, out Sprite sprite))
            icon.sprite = sprite;
        activePerkIcons[perk] = icon;
    }

    private void HandlePerkRemoved(PerkType perk)
    {
        if (activePerkIcons.TryGetValue(perk, out Image icon))
        {
            Destroy(icon.gameObject);
            activePerkIcons.Remove(perk);
        }
    }
}

[System.Serializable]
public struct PerkIcon
{
    public PerkType perk;
    public Sprite icon;
}