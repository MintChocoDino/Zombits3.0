using UnityEngine;

public class PackAPunchInteractable : Interactable
{
    [Header("Upgrade Stats")]
    public float damageMultiplier = 2f;
    public float magazineMultiplier = 1.5f;
    public float reserveMultiplier = 1.5f;

    [Header("Repeat Cost")]
    [Tooltip("Cost is multiplied by this after each successful upgrade, so stacking Pack-a-Punch gets steeply more expensive.")]
    public float costMultiplierPerUse = 2f;

    public override string PromptText => $"Press {interactKey} to Pack-a-Punch [{cost}]";

    protected override void OnInteract(PlayerState player)
    {
        Weapon active = player.ActiveWeapon;
        if (active == null)
        {
            InteractionPrompt.Flash("No weapon equipped!");
            player.AddCredits(cost);
            return;
        }

        active.bulletDamage *= damageMultiplier;
        active.magazineSize = Mathf.RoundToInt(active.magazineSize * magazineMultiplier);
        active.reserveAmmoMax = Mathf.RoundToInt(active.reserveAmmoMax * reserveMultiplier);
        active.AddReserveAmmo(active.reserveAmmoMax);

        cost = Mathf.RoundToInt(cost * costMultiplierPerUse);
        RefreshPrompt();

        InteractionPrompt.Flash($"{active.name} upgraded!");
    }
}