using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;
    private float damage;
    private float speed;
    private Rigidbody2D rb;
    private float spawnTime;
    private GameObject owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, float speed, float damage, GameObject owner)
    {
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;
        spawnTime = Time.time;

        rb.linearVelocity = direction.normalized * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() != null) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
            target.TakeDamage(damage, owner);

        Destroy(gameObject);
    }
}

public interface IDamageable
{
    void TakeDamage(float amount, GameObject source = null);
}