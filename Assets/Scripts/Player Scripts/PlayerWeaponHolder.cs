using UnityEngine;

[RequireComponent(typeof(PlayerState))]
public class PlayerWeaponHolder : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public Transform weaponSocket;
    public Weapon[] startingWeapons = new Weapon[2];

    [Header("Input")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode swapKey = KeyCode.E;

    [Header("Aiming")]
    public float orbitRadius = 0.6f;

    private Camera cam;

    void Awake()
    {
        if (playerState == null) playerState = GetComponent<PlayerState>();
        if (weaponSocket == null) weaponSocket = transform;
    }

    void Start()
    {
        cam = Camera.main;

        for (int i = 0; i < startingWeapons.Length && i < playerState.WeaponSlotCount; i++)
        {
            if (startingWeapons[i] != null)
                GiveWeapon(startingWeapons[i], i);
        }

        for (int i = 0; i < playerState.WeaponSlotCount; i++)
        {
            if (playerState.GetWeapon(i) != null)
            {
                playerState.SetActiveSlot(i);
                break;
            }
        }

        playerState.OnActiveSlotChanged += RefreshVisibleWeapon;
        playerState.OnWeaponChanged += (slot, w) => RefreshVisibleWeapon(playerState.ActiveSlot);
        RefreshVisibleWeapon(playerState.ActiveSlot);
    }

    void Update()
    {
        Weapon active = playerState.ActiveWeapon;
        if (active == null) return;

        Vector2 aim = GetAimDirection();
        AimWeapon(active, aim);
        HandleInput(active, aim);
    }

    private void HandleInput(Weapon active, Vector2 aim)
    {
        if (Input.GetMouseButton(0))
            active.TryFire(aim);

        if (Input.GetKeyDown(reloadKey))
            active.TryReload();

        for (int i = 0; i < playerState.WeaponSlotCount && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                playerState.SetActiveSlot(i);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(swapKey) || Mathf.Abs(scroll) > 0.01f)
        {
            int next = (playerState.ActiveSlot + 1) % playerState.WeaponSlotCount;
            int safety = playerState.WeaponSlotCount;
            while (playerState.GetWeapon(next) == null && safety-- > 0)
                next = (next + 1) % playerState.WeaponSlotCount;
            playerState.SetActiveSlot(next);
        }
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        return ((Vector2)mouseWorld - (Vector2)weaponSocket.position).normalized;
    }

    private void AimWeapon(Weapon w, Vector2 aim)
    {
        w.transform.position = weaponSocket.position + (Vector3)(aim * orbitRadius);

        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        w.transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 scale = w.transform.localScale;
        scale.y = Mathf.Abs(scale.y) * (aim.x < 0 ? -1 : 1);
        w.transform.localScale = scale;
    }

    public void GiveWeapon(Weapon prefab, int slot = -1)
    {
        if (slot < 0)
        {
            slot = playerState.GetFirstEmptySlot();
            if (slot < 0) slot = playerState.ActiveSlot;
        }

        Weapon existing = playerState.GetWeapon(slot);
        if (existing != null) Destroy(existing.gameObject);

        Weapon instance = Instantiate(prefab, weaponSocket);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.Bind(playerState);

        playerState.SetWeapon(slot, instance);
    }

    private void RefreshVisibleWeapon(int activeSlot)
    {
        for (int i = 0; i < playerState.WeaponSlotCount; i++)
        {
            Weapon w = playerState.GetWeapon(i);
            if (w != null) w.gameObject.SetActive(i == activeSlot);
        }
    }
}