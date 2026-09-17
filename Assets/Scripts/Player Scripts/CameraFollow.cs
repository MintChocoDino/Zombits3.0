using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Smoothing")]
    public float smoothTimeX = 0.5f;
    public float smoothTimeY = 0.5f;

    [Header("Cursor Look-Ahead")]
    [Range(0f, 1f)]
    public float lookAheadRatio = 0.35f;
    public float maxLookAheadDistance = 5f;

    private float referenceVelocityX = 0;
    private float referenceVelocityY = 0;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null) cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (player == null || cam == null) return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);

        Vector2 offset = ((Vector2)mouseWorld - (Vector2)player.position) * lookAheadRatio;
        if (offset.magnitude > maxLookAheadDistance)
            offset = offset.normalized * maxLookAheadDistance;

        Vector2 target = (Vector2)player.position + offset;

        transform.position = new Vector3(
            Mathf.SmoothDamp(transform.position.x, target.x, ref referenceVelocityX, smoothTimeX),
            Mathf.SmoothDamp(transform.position.y, target.y, ref referenceVelocityY, smoothTimeY),
            transform.position.z
        );
    }
}