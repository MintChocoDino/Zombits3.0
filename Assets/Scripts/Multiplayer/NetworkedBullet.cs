using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkedBullet : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Vector2 dir = transform.right;
            rb.linearVelocity = dir * speed;
            Invoke(nameof(DespawnSelf), lifetime);
        }
    }

    private void DespawnSelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.GetComponent<NetworkedBullet>() != null) return;
        if (other.GetComponent<NetworkedPlayer>() != null) return;

        DespawnSelf();
    }
}