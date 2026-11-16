using System.Collections.Generic;
using UnityEngine;

// Bo cuc ban do dang luoi + duong di cua lu chuot.
// Luoi 16 x 9 o, moi o 1 don vi, tam ban do o (0,0).
public static class MapLayout
{
    public const int Cols = 16;
    public const int Rows = 9;
    public const float CellSize = 1f;

    public static readonly Vector2 Origin = new Vector2(-7.5f, -4f); // tam o (0,0)
    public static readonly Rect MapRect = new Rect(-8f, -4.5f, 16f, 9f);

    // Cac o re cua duong di (theo o luoi).
    public static readonly Vector2Int[] TurnCells =
    {
        new Vector2Int(0, 7), new Vector2Int(9, 7), new Vector2Int(9, 4),
        new Vector2Int(2, 4), new Vector2Int(2, 2), new Vector2Int(12, 2),
        new Vector2Int(12, 5), new Vector2Int(15, 5),
    };

    public static readonly Vector2[] Waypoints;
    public static readonly Vector2 SpawnPoint;
    public static readonly Vector2 BasePoint;
    public static readonly float TotalLength;

    static readonly float[] segLen;
    static readonly float[] segCum;
    static readonly HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();

    static MapLayout()
    {
        var list = new List<Vector2>();
        Vector2 first = CellCenter(TurnCells[0].x, TurnCells[0].y);
        list.Add(first + Vector2.left * 1.9f); // xuat phat ngoai bia
        foreach (var t in TurnCells) list.Add(CellCenter(t.x, t.y));
        Waypoints = list.ToArray();

        SpawnPoint = Waypoints[0];
        BasePoint = Waypoints[Waypoints.Length - 1];

        segLen = new float[Waypoints.Length - 1];
        segCum = new float[segLen.Length + 1];
        float total = 0f;
        for (int i = 0; i < segLen.Length; i++)
        {
            segLen[i] = Vector2.Distance(Waypoints[i], Waypoints[i + 1]);
            segCum[i] = total;
            total += segLen[i];
        }
        segCum[segLen.Length] = total;
        TotalLength = total;

        // Danh dau cac o nam tren duong di (khong cho dat meo).
        for (int i = 0; i < TurnCells.Length - 1; i++)
        {
            Vector2Int a = TurnCells[i], b = TurnCells[i + 1];
            int dx = System.Math.Sign(b.x - a.x);
            int dy = System.Math.Sign(b.y - a.y);
            Vector2Int c = a;
            pathCells.Add(c);
            while (c != b)
            {
                c = new Vector2Int(c.x + dx, c.y + dy);
                pathCells.Add(c);
            }
        }
    }

    public static Vector2 CellCenter(int col, int row)
    {
        return Origin + new Vector2(col * CellSize, row * CellSize);
    }

    public static Vector2 CellCenter(Vector2Int cell) => CellCenter(cell.x, cell.y);

    public static Vector2Int WorldToCell(Vector2 p)
    {
        float fx = (p.x - Origin.x) / CellSize;
        float fy = (p.y - Origin.y) / CellSize;
        return new Vector2Int(Mathf.RoundToInt(fx), Mathf.RoundToInt(fy));
    }

    public static bool InBounds(Vector2Int c)
    {
        return c.x >= 0 && c.x < Cols && c.y >= 0 && c.y < Rows;
    }

    public static bool IsPath(Vector2Int c) => pathCells.Contains(c);

    public static Vector2 PointAtDistance(float d, out Vector2 dir)
    {
        if (d <= 0f)
        {
            dir = (Waypoints[1] - Waypoints[0]).normalized;
            return Waypoints[0];
        }
        for (int i = 0; i < segLen.Length; i++)
        {
            if (d <= segCum[i + 1] || i == segLen.Length - 1)
            {
                float t = Mathf.Clamp01((d - segCum[i]) / Mathf.Max(0.0001f, segLen[i]));
                dir = (Waypoints[i + 1] - Waypoints[i]).normalized;
                return Vector2.Lerp(Waypoints[i], Waypoints[i + 1], t);
            }
        }
        dir = Vector2.right;
        return Waypoints[Waypoints.Length - 1];
    }

    public static float DistanceToPath(Vector2 p)
    {
        float best = float.MaxValue;
        for (int i = 0; i < Waypoints.Length - 1; i++)
        {
            float d = DistanceToSegment(p, Waypoints[i], Waypoints[i + 1]);
            if (d < best) best = d;
        }
        return best;
    }

    public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 0.000001f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
        return Vector2.Distance(p, a + ab * t);
    }
}
