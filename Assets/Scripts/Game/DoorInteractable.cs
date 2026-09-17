using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorInteractable : Interactable
{
    [Header("Door")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;

    public override string PromptText => $"Press {interactKey} to open door [{cost}]";

    void Start()
    {
        oneTimeUse = true;
    }

    protected override void OnInteract(PlayerState player)
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        wallTilemap.SetTile(cell, null);
        if (floorTilemap != null && floorTile != null)
            floorTilemap.SetTile(cell, floorTile);

        Destroy(gameObject);
    }
}