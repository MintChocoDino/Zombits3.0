using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float volume = 0.5f;
    public float pitchJitter = 0.08f;

    [Header("Timing")]
    public float baseInterval = 0.4f;
    public float minSpeedToStep = 0.5f;

    private Rigidbody2D rb;
    private float nextStepTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < minSpeedToStep)
        {
            nextStepTime = Time.time;
            return;
        }

        if (Time.time >= nextStepTime)
        {
            if (footstepClips != null && footstepClips.Length > 0 && AudioManager.Instance != null)
                AudioManager.Instance.PlayRandomAt(footstepClips, transform.position, volume, pitchJitter);

            float speedFactor = Mathf.Clamp(speed / 5f, 0.5f, 1.5f);
            nextStepTime = Time.time + baseInterval / speedFactor;
        }
    }
}