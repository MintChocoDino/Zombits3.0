using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("UI")]
    public Sprite icon;

    [Header("Audio")]
    public AudioClip fireSound;
    [Range(0f, 1f)] public float fireVolume = 0.8f;
    public float firePitchJitter = 0.05f;

    [Header("Bullet")]
    public Bullet bulletPrefab;
    public Transform muzzle;

    [Header("Damage / Speed")]
    public float bulletDamage = 10f;
    public float bulletSpeed = 20f;

    [Header("Fire Rate")]
    public float fireRate = 5f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int reserveAmmoMax = 60;
    public float reloadTime = 1.5f;

    protected int currentAmmo;
    protected int reserveAmmo;
    protected float nextFireTime;
    protected bool isReloading;

    protected PlayerState playerState;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;

    public void Bind(PlayerState state)
    {
        playerState = state;
    }

    private float FireRateMult => playerState != null ? playerState.FireRateMultiplier : 1f;
    private float ReloadMult => playerState != null ? playerState.ReloadSpeedMultiplier : 1f;

    protected virtual void Start()
    {
        currentAmmo = magazineSize;
        reserveAmmo = reserveAmmoMax;
    }

    public void TryFire(Vector2 aimDirection)
    {
        if (isReloading) return;
        if (Time.time < nextFireTime) return;
        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        Fire(aimDirection);
        currentAmmo--;
        nextFireTime = Time.time + 1f / (fireRate * FireRateMult);

        if (fireSound != null && AudioManager.Instance != null)
        {
            Vector3 pos = muzzle != null ? muzzle.position : transform.position;
            float pitch = 1f + Random.Range(-firePitchJitter, firePitchJitter);
            AudioManager.Instance.PlayAt(fireSound, pos, fireVolume, pitch);
        }
    }

    protected virtual void Fire(Vector2 aimDirection)
    {
        SpawnBullet(aimDirection);
    }

    protected void SpawnBullet(Vector2 direction)
    {
        Bullet b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        GameObject owner = playerState != null ? playerState.gameObject : null;
        b.Init(direction, bulletSpeed, bulletDamage, owner);
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo >= magazineSize) return;
        if (reserveAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime / ReloadMult);

        int needed = magazineSize - currentAmmo;
        int taken = Mathf.Min(needed, reserveAmmo);
        currentAmmo += taken;
        reserveAmmo -= taken;
        isReloading = false;
    }

    protected virtual void OnDisable()
    {
        isReloading = false;
    }

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(reserveAmmo + amount, reserveAmmoMax);
    }
}