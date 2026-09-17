using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkedPlayer : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Visuals")]
    public SpriteRenderer sprite;
    public Transform pistolPivot;
    public Transform muzzle;
    public float pistolOrbitRadius = 0.6f;

    [Header("Shooting")]
    public NetworkedBullet bulletPrefab;
    public float fireRate = 4f;

    private Rigidbody2D rb;
    private Vector2 input;
    private Camera cam;
    private float nextFireTime;

    private static readonly Color[] PlayerColors =
    {
        new Color(0.3f, 0.7f, 1f),
        new Color(1f, 0.4f, 0.4f),
        new Color(0.5f, 1f, 0.5f),
        new Color(1f, 0.9f, 0.3f),
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (sprite != null)
        {
            int idx = (int)(OwnerClientId % (ulong)PlayerColors.Length);
            sprite.color = PlayerColors[idx];
        }

        if (IsOwner)
        {
            cam = Camera.main;
            if (cam != null && cam.TryGetComponent<CameraFollow>(out var follow))
                follow.player = transform;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;

        if (cam == null) cam = Camera.main;

        if (cam != null && pistolPivot != null)
        {
            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = -cam.transform.position.z;
            Vector2 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
            Vector2 aim = (mouseWorld - (Vector2)transform.position).normalized;
            UpdatePistolTransform(aim);

            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                FireServerRpc(muzzle.position, aim);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        rb.linearVelocity = input * moveSpeed;
    }

    private void UpdatePistolTransform(Vector2 aim)
    {
        pistolPivot.position = transform.position + (Vector3)(aim * pistolOrbitRadius);
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        pistolPivot.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 scale = pistolPivot.localScale;
        scale.y = Mathf.Abs(scale.y) * (aim.x < 0 ? -1 : 1);
        pistolPivot.localScale = scale;
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 muzzlePos, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        NetworkedBullet bullet = Instantiate(bulletPrefab, muzzlePos, Quaternion.Euler(0, 0, angle));
        bullet.NetworkObject.Spawn();
    }
}