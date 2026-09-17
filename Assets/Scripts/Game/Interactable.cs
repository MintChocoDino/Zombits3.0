using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public int cost = 0;
    public KeyCode interactKey = KeyCode.F;
    public bool oneTimeUse = false;

    private PlayerState nearbyPlayer;
    private bool used;

    public abstract string PromptText { get; }
    protected abstract void OnInteract(PlayerState player);

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (nearbyPlayer == null) return;
        if (used && oneTimeUse) return;

        if (Input.GetKeyDown(interactKey))
            TryInteract(nearbyPlayer);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerState ps = other.GetComponentInParent<PlayerState>();
        if (ps == null) return;

        nearbyPlayer = ps;
        if (!used || !oneTimeUse)
            InteractionPrompt.Show(PromptText);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerState ps = other.GetComponentInParent<PlayerState>();
        if (ps == null || ps != nearbyPlayer) return;

        nearbyPlayer = null;
        InteractionPrompt.Hide();
    }

    private void TryInteract(PlayerState player)
    {
        if (cost > 0 && !player.TrySpendCredits(cost))
        {
            InteractionPrompt.Flash("Not enough credits!");
            return;
        }

        OnInteract(player);
        used = true;

        if (oneTimeUse)
            InteractionPrompt.Hide();
        else
            InteractionPrompt.Show(PromptText);
    }

    protected void RefreshPrompt()
    {
        if (nearbyPlayer != null && !(used && oneTimeUse))
            InteractionPrompt.Show(PromptText);
    }
}