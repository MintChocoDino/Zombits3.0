using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridPathfinder : MonoBehaviour
{
    public static GridPathfinder Instance { get; private set; }

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    void Awake()
    {
        Instance = this;
    }

    public List<Vector3> FindPath(Vector3 worldStart, Vector3 worldEnd)
    {
        Vector3Int start = floorTilemap.WorldToCell(worldStart);
        Vector3Int end = floorTilemap.WorldToCell(worldEnd);

        if (!IsWalkable(end))
            end = FindNearestWalkable(end);

        if (!IsWalkable(start) || !IsWalkable(end))
            return null;

        var open = new PriorityQueue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float> { [start] = 0 };

        open.Enqueue(start, Heuristic(start, end));

        int safety = 5000;
        while (open.Count > 0 && safety-- > 0)
        {
            Vector3Int current = open.Dequeue();
            if (current == end)
                return Reconstruct(cameFrom, current);

            foreach (var neighbor in Neighbors(current))
            {
                if (!IsWalkable(neighbor)) continue;

                int dx = neighbor.x - current.x;
                int dy = neighbor.y - current.y;
                if (dx != 0 && dy != 0)
                {
                    Vector3Int sideA = new Vector3Int(current.x + dx, current.y, 0);
                    Vector3Int sideB = new Vector3Int(current.x, current.y + dy, 0);
                    if (!IsWalkable(sideA) && !IsWalkable(sideB)) continue;
                }

                float tentativeG = gScore[current] + Distance(current, neighbor);
                if (!gScore.TryGetValue(neighbor, out float existingG) || tentativeG < existingG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    open.Enqueue(neighbor, tentativeG + Heuristic(neighbor, end));
                }
            }
        }

        return null;
    }

    public bool IsWalkable(Vector3Int cell)
    {
        if (floorTilemap.GetTile(cell) == null) return false;
        if (wallTilemap.GetTile(cell) != null) return false;
        return true;
    }

    private Vector3Int FindNearestWalkable(Vector3Int origin)
    {
        Queue<Vector3Int> q = new Queue<Vector3Int>();
        HashSet<Vector3Int> seen = new HashSet<Vector3Int>();
        q.Enqueue(origin);
        seen.Add(origin);

        int limit = 400;
        while (q.Count > 0 && limit-- > 0)
        {
            var cell = q.Dequeue();
            if (IsWalkable(cell)) return cell;
            foreach (var n in Neighbors(cell))
                if (seen.Add(n)) q.Enqueue(n);
        }
        return origin;
    }

    private static readonly Vector3Int[] dirs =
    {
        new Vector3Int( 1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 1, 0), new Vector3Int( 0,-1, 0),
        new Vector3Int( 1, 1, 0), new Vector3Int(-1, 1, 0),
        new Vector3Int( 1,-1, 0), new Vector3Int(-1,-1, 0),
    };

    private IEnumerable<Vector3Int> Neighbors(Vector3Int cell)
    {
        foreach (var d in dirs) yield return cell + d;
    }

    private float Distance(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 1) ? 1.4142f : 1f;
    }

    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy);
    }

    private List<Vector3> Reconstruct(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var path = new List<Vector3> { floorTilemap.GetCellCenterWorld(current) };
        while (cameFrom.TryGetValue(current, out Vector3Int prev))
        {
            current = prev;
            path.Add(floorTilemap.GetCellCenterWorld(current));
        }
        path.Reverse();
        return path;
    }
}

public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> heap = new();
    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].priority <= heap[i].priority) break;
            (heap[parent], heap[i]) = (heap[i], heap[parent]);
            i = parent;
        }
    }

    public T Dequeue()
    {
        T result = heap[0].item;
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        int i = 0;
        while (true)
        {
            int l = i * 2 + 1, r = i * 2 + 2, smallest = i;
            if (l < heap.Count && heap[l].priority < heap[smallest].priority) smallest = l;
            if (r < heap.Count && heap[r].priority < heap[smallest].priority) smallest = r;
            if (smallest == i) break;
            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }
        return result;
    }
}