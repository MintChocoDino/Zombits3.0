using UnityEngine;

public class MysteryBoxInteractable : Interactable
{
    [Header("Loot Pool")]
    public Weapon[] possibleWeapons;

    [Header("References")]
    public PlayerWeaponHolder weaponHolder;

    public override string PromptText => $"Press {interactKey} to use Mystery Box [{cost}]";

    protected override void OnInteract(PlayerState player)
    {
        if (possibleWeapons == null || possibleWeapons.Length == 0)
        {
            player.AddCredits(cost);
            return;
        }

        if (weaponHolder == null)
            weaponHolder = player.GetComponent<PlayerWeaponHolder>();

        if (weaponHolder == null)
        {
            player.AddCredits(cost);
            return;
        }

        Weapon prize = possibleWeapons[Random.Range(0, possibleWeapons.Length)];
        weaponHolder.GiveWeapon(prize);
        InteractionPrompt.Flash($"Got {prize.name}!");
    }
}