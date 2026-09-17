using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    private static InteractionPrompt instance;

    public TMP_Text text;
    public GameObject container;
    public float flashDuration = 1.5f;

    private float hideFlashAt;
    private bool flashing;

    void Awake()
    {
        instance = this;
        if (container != null) container.SetActive(false);
    }

    void Update()
    {
        if (flashing && Time.time >= hideFlashAt)
        {
            flashing = false;
            if (container != null) container.SetActive(false);
        }
    }

    public static void Show(string message)
    {
        if (instance == null) return;
        instance.flashing = false;
        if (instance.container != null) instance.container.SetActive(true);
        if (instance.text != null) instance.text.text = message;
    }

    public static void Hide()
    {
        if (instance == null || instance.flashing) return;
        if (instance.container != null) instance.container.SetActive(false);
    }

    public static void Flash(string message)
    {
        if (instance == null) return;
        if (instance.container != null) instance.container.SetActive(true);
        if (instance.text != null) instance.text.text = message;
        instance.flashing = true;
        instance.hideFlashAt = Time.time + instance.flashDuration;
    }
}