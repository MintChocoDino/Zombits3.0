using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase zombieSpawnTile;

    [Header("Interactable Prefabs")]
    public DoorInteractable doorPrefab;
    public MysteryBoxInteractable mysteryBoxPrefab;
    public PackAPunchInteractable packAPunchPrefab;
    public PerkMachineInteractable[] perkPrefabs = new PerkMachineInteractable[5];

    [Header("Map Size")]
    public int mapWidth = 100;
    public int mapHeight = 100;

    [Header("Rooms")]
    public int roomCount = 10;
    public int tilesPerRoom = 120;
    public int spawnsPerRoom = 2;

    [Header("Seeding")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    private bool[,] floors;
    private bool[,] corridors;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();
    private List<HashSet<Vector2Int>> roomFloors = new List<HashSet<Vector2Int>>();
    private List<Vector2Int> doorPositions = new List<Vector2Int>();

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        int s = useRandomSeed ? System.DateTime.Now.Ticks.GetHashCode() : seed;
        Random.InitState(s);

        floors = new bool[mapWidth, mapHeight];
        corridors = new bool[mapWidth, mapHeight];
        roomCenters.Clear();
        roomFloors.Clear();
        doorPositions.Clear();
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        CarveRooms();
        ConnectRooms();
        Paint();
        PlaceZombieSpawns();
        PlaceDoors();
        PlaceInteractables();
    }

    private void CarveRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int pos = i == 0
                ? new Vector2Int(mapWidth / 2, mapHeight / 2)
                : new Vector2Int(
                    Random.Range(5, mapWidth - 5),
                    Random.Range(5, mapHeight - 5));
            roomCenters.Add(pos);

            HashSet<Vector2Int> thisRoom = new HashSet<Vector2Int>();
            roomFloors.Add(thisRoom);

            for (int step = 0; step < tilesPerRoom; step++)
            {
                floors[pos.x, pos.y] = true;
                thisRoom.Add(new Vector2Int(pos.x, pos.y));

                switch (Random.Range(0, 4))
                {
                    case 0: pos.x++; break;
                    case 1: pos.x--; break;
                    case 2: pos.y++; break;
                    case 3: pos.y--; break;
                }

                pos.x = Mathf.Clamp(pos.x, 1, mapWidth - 2);
                pos.y = Mathf.Clamp(pos.y, 1, mapHeight - 2);
            }
        }
    }

    private void ConnectRooms()
    {
        for (int i = 1; i < roomCenters.Count; i++)
        {
            Vector2Int a = roomCenters[i - 1];
            Vector2Int b = roomCenters[i];

            for (int x = Mathf.Min(a.x, b.x); x <= Mathf.Max(a.x, b.x); x++)
                CarveCorridor(x, a.y);
            for (int y = Mathf.Min(a.y, b.y); y <= Mathf.Max(a.y, b.y); y++)
                CarveCorridor(b.x, y);
        }
    }

    private void CarveCorridor(int x, int y)
    {
        if (!floors[x, y]) corridors[x, y] = true;
        floors[x, y] = true;
    }

    private void Paint()
    {
        int offsetX = mapWidth / 2;
        int offsetY = mapHeight / 2;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int cell = new Vector3Int(x - offsetX, y - offsetY, 0);
                if (floors[x, y])
                    floorTilemap.SetTile(cell, floorTile);
                else if (HasFloorNeighbor(x, y))
                    wallTilemap.SetTile(cell, wallTile);
            }
        }
    }

    private bool HasFloorNeighbor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight && floors[nx, ny])
                    return true;
            }
        return false;
    }

    private void PlaceZombieSpawns()
    {
        int offsetX = mapWidth / 2;
        int offsetY = mapHeight / 2;

        foreach (var room in roomFloors)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (var floor in room)
            {
                if (HasWallNeighbor(floor.x, floor.y))
                    candidates.Add(floor);
            }

            int count = Mathf.Min(spawnsPerRoom, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                int pick = Random.Range(0, candidates.Count);
                Vector2Int p = candidates[pick];
                candidates.RemoveAt(pick);

                Vector3Int cell = new Vector3Int(p.x - offsetX, p.y - offsetY, 0);
                floorTilemap.SetTile(cell, zombieSpawnTile);
            }
        }
    }

    private void PlaceDoors()
    {
        int offsetX = mapWidth / 2;
        int offsetY = mapHeight / 2;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!corridors[x, y]) continue;
                if (!TouchesRoomFloor(x, y)) continue;
                if (CountWallNeighbors(x, y) < 2) continue;

                Vector3Int cell = new Vector3Int(x - offsetX, y - offsetY, 0);
                SpawnDoor(cell);
                doorPositions.Add(new Vector2Int(x, y));
            }
        }
    }

    private void SpawnDoor(Vector3Int cell)
    {
        if (doorPrefab == null) return;

        floorTilemap.SetTile(cell, null);
        wallTilemap.SetTile(cell, wallTile);

        Vector3 worldPos = floorTilemap.GetCellCenterWorld(cell);
        DoorInteractable door = Instantiate(doorPrefab, worldPos, Quaternion.identity, transform);
        door.floorTilemap = floorTilemap;
        door.wallTilemap = wallTilemap;
        door.floorTile = floorTile;
    }

    private int CountWallNeighbors(int x, int y)
    {
        int count = 0;
        if (IsWall(x + 1, y)) count++;
        if (IsWall(x - 1, y)) count++;
        if (IsWall(x, y + 1)) count++;
        if (IsWall(x, y - 1)) count++;
        return count;
    }

    private bool TouchesRoomFloor(int x, int y)
    {
        return IsRoomFloor(x + 1, y) || IsRoomFloor(x - 1, y)
            || IsRoomFloor(x, y + 1) || IsRoomFloor(x, y - 1);
    }

    private bool IsRoomFloor(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return false;
        return floors[x, y] && !corridors[x, y];
    }

    private bool HasWallNeighbor(int x, int y)
    {
        return IsWall(x + 1, y) || IsWall(x - 1, y) || IsWall(x, y + 1) || IsWall(x, y - 1);
    }

    private bool IsWall(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return false;
        return !floors[x, y] && HasFloorNeighbor(x, y);
    }

    private void PlaceInteractables()
    {
        int offsetX = mapWidth / 2;
        int offsetY = mapHeight / 2;

        List<Interactable> toPlace = new List<Interactable>();
        if (mysteryBoxPrefab != null) toPlace.Add(mysteryBoxPrefab);
        if (packAPunchPrefab != null) toPlace.Add(packAPunchPrefab);
        if (perkPrefabs != null)
            foreach (var p in perkPrefabs)
                if (p != null) toPlace.Add(p);

        List<int> roomOrder = new List<int>();
        for (int i = 0; i < roomFloors.Count; i++) roomOrder.Add(i);
        for (int i = 0; i < roomOrder.Count; i++)
        {
            int j = Random.Range(i, roomOrder.Count);
            (roomOrder[i], roomOrder[j]) = (roomOrder[j], roomOrder[i]);
        }

        int placed = 0;
        for (int r = 0; r < roomOrder.Count && placed < toPlace.Count; r++)
        {
            var room = roomFloors[roomOrder[r]];

            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (var f in room)
                if (HasWallNeighbor(f.x, f.y))
                    candidates.Add(f);

            if (candidates.Count == 0) continue;

            Vector2Int p = candidates[Random.Range(0, candidates.Count)];
            Vector3Int cell = new Vector3Int(p.x - offsetX, p.y - offsetY, 0);
            Vector3 worldPos = floorTilemap.GetCellCenterWorld(cell);

            Instantiate(toPlace[placed], worldPos, Quaternion.identity, transform);
            placed++;
        }
    }
}