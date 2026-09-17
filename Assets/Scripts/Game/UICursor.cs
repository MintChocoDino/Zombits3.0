using UnityEngine;

public class UICursor : MonoBehaviour
{
    public static UICursor Instance { get; private set; }

    public RectTransform cursorImage;
    public Canvas canvas;

    private bool customEnabled = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        SetCustomCursor(true);
    }

    void Update()
    {
        if (!customEnabled || cursorImage == null) return;

        Vector2 mouseScreen = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mouseScreen,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );
        cursorImage.anchoredPosition = localPoint;
    }

    public void SetCustomCursor(bool useCustom)
    {
        customEnabled = useCustom;
        Cursor.visible = !useCustom;
        if (cursorImage != null) cursorImage.gameObject.SetActive(useCustom);
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}