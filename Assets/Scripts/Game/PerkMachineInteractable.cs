using UnityEngine;

public class PerkMachineInteractable : Interactable
{
    [Header("Perk")]
    public PerkType perk = PerkType.Juggernog;

    public override string PromptText
    {
        get
        {
            string perkName = perk.ToString();
            return $"Press {interactKey} to buy {perkName} [{cost}]";
        }
    }

    protected override void OnInteract(PlayerState player)
    {
        if (!player.AddPerk(perk))
        {
            player.AddCredits(cost);
            InteractionPrompt.Flash("Already owned!");
        }
    }
}