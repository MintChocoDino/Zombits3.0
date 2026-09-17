using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
public class PlayerController : MonoBehaviour
{
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";

    private Rigidbody2D rb;
    private PlayerState state;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<PlayerState>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw(horizontalAxis);
        input.y = Input.GetAxisRaw(verticalAxis);
        input = input.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * state.MoveSpeed;
    }
}