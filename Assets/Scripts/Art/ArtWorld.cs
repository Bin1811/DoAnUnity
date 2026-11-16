using UnityEngine;

// Sinh hinh nen (co, duong dat, cay coi) va thanh meo / hang chuot bang code.
public static class ArtWorld
{
    const int PPC = 64;                 // pixel tren 1 don vi
    const float HalfW = 12f;            // texture rong 24 don vi
    const float HalfH = 7.5f;           // texture cao 15 don vi

    static Sprite background;
    static Sprite castle;
    static Sprite hole;
    static Sprite sign;

    public static Sprite Background => background ?? (background = BuildBackground());
    public static Sprite Castle => castle ?? (castle = BuildCastle());
    public static Sprite Hole => hole ?? (hole = BuildHole());
    public static Sprite Sign => sign ?? (sign = BuildSign());

    static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + seed * 1442695041;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0x7fffffff) / 2147483647f;
        }
    }

    static float ValueNoise(float x, float y, int seed)
    {
        int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
        float fx = x - xi, fy = y - yi;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        float a = Hash(xi, yi, seed), b = Hash(xi + 1, yi, seed);
        float c = Hash(xi, yi + 1, seed), d = Hash(xi + 1, yi + 1, seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    static Vector2 PixelToWorld(int px, int py)
    {
        return new Vector2(-HalfW + (px + 0.5f) / PPC, HalfH - (py + 0.5f) / PPC);
    }

    static Vector2 WorldToPixel(Vector2 w)
    {
        return new Vector2((w.x + HalfW) * PPC, (HalfH - w.y) * PPC);
    }

    static Sprite BuildBackground()
    {
        int w = Mathf.RoundToInt(HalfW * 2f * PPC);
        int h = Mathf.RoundToInt(HalfH * 2f * PPC);
        var c = new Canvas2D(w, h);
        var rnd = new System.Random(20260613);

        for (int py = 0; py < h; py++)
        {
            for (int px = 0; px < w; px++)
            {
                Vector2 wpos = PixelToWorld(px, py);
                float d = MapLayout.DistanceToPath(wpos);
                float n = ValueNoise(wpos.x * 1.5f, wpos.y * 1.5f, 7) * 0.65f
                          + ValueNoise(wpos.x * 5.5f, wpos.y * 5.5f, 11) * 0.35f;
                Color col;

                if (d < 0.40f)
                {
                    float grain = ValueNoise(wpos.x * 26f, wpos.y * 26f, 21);
                    col = Color.Lerp(Pal.Dirt, Pal.DirtLight, grain * 0.55f + n * 0.35f);
                    if (d > 0.33f) col = Color.Lerp(col, Pal.DirtDark, Mathf.InverseLerp(0.33f, 0.40f, d) * 0.85f);
                }
                else
                {
                    col = Color.Lerp(Pal.GrassDark, Pal.GrassLight, n);
                    float edge = Mathf.InverseLerp(0.40f, 0.66f, d);
                    col = Color.Lerp(Color.Lerp(col, Pal.DirtDark, 0.16f), col, edge);
                }

                // Vien toi quanh bia texture de hoa vao nen rung.
                float edgeDist = Mathf.Min(Mathf.Min(wpos.x + HalfW, HalfW - wpos.x),
                                           Mathf.Min(wpos.y + HalfH, HalfH - wpos.y));
                if (edgeDist < 2.4f) col = Color.Lerp(Pal.Forest, col, Mathf.SmoothStep(0f, 1f, edgeDist / 2.4f));

                // Luoi o mo (chi trong ban do, khong de len duong dat).
                var inMap = MapLayout.MapRect.Contains(wpos);
                if (inMap && d > 0.46f)
                {
                    float gx = Mathf.Abs(wpos.x + 8f - Mathf.Round(wpos.x + 8f));
                    float gy = Mathf.Abs(wpos.y + 4.5f - Mathf.Round(wpos.y + 4.5f));
                    if (gx < 1.1f / PPC || gy < 1.1f / PPC)
                        col = Color.Lerp(col, Pal.Ink, 0.06f);
                }

                c.Px[py * w + px] = col;
            }
        }

        // Sui da nho tren duong.
        for (int i = 0; i < 900; i++)
        {
            float wx = Mathf.Lerp(-10f, 8.6f, (float)rnd.NextDouble());
            float wy = Mathf.Lerp(-5.5f, 4.3f, (float)rnd.NextDouble());
            float d = MapLayout.DistanceToPath(new Vector2(wx, wy));
            if (d > 0.05f && d < 0.31f)
            {
                Vector2 p = WorldToPixel(new Vector2(wx, wy));
                bool light = rnd.NextDouble() > 0.5;
                Color col = light ? new Color(Pal.DirtLight.r, Pal.DirtLight.g, Pal.DirtLight.b, 0.55f)
                                  : new Color(Pal.DirtDark.r, Pal.DirtDark.g, Pal.DirtDark.b, 0.5f);
                c.Ellipse(p.x, p.y, 1f + (float)rnd.NextDouble() * 1.6f, 0.8f + (float)rnd.NextDouble() * 1.2f, col);
            }
        }

        // Co, hoa.
        for (int i = 0; i < 1500; i++)
        {
            float wx = Mathf.Lerp(-11.4f, 11.4f, (float)rnd.NextDouble());
            float wy = Mathf.Lerp(-7f, 7f, (float)rnd.NextDouble());
            if (MapLayout.DistanceToPath(new Vector2(wx, wy)) < 0.55f) continue;
            if (rnd.NextDouble() < 0.22)
            {
                Vector2 p = WorldToPixel(new Vector2(wx, wy));
                Color petal = rnd.NextDouble() > 0.5 ? Pal.Cream : Pal.Gold;
                c.Disc(p.x, p.y, 1.5f, petal);
                c.Disc(p.x, p.y, 0.7f, Pal.C("#e5a33a"));
            }
            else
            {
                Vector2 p = WorldToPixel(new Vector2(wx, wy));
                Color g = new Color(Pal.GrassDark.r, Pal.GrassDark.g, Pal.GrassDark.b, 0.45f);
                c.Segment(p.x, p.y, p.x - 1.5f, p.y - 3f, 1f, g);
                c.Segment(p.x, p.y, p.x + 1.5f, p.y - 3.2f, 1f, g);
                c.Segment(p.x, p.y, p.x, p.y - 3.6f, 1f, g);
            }
        }

        // Dau chan meo tren duong.
        for (float t = 4f; t < MapLayout.TotalLength - 3f; t += 1.65f)
        {
            Vector2 pos = MapLayout.PointAtDistance(t, out Vector2 dir);
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float side = (Mathf.Round(t * 10f) % 2f == 0f) ? 1f : -1f;
            Vector2 p = WorldToPixel(pos + perp * 0.11f * side);
            Color paw = new Color(Pal.Ink.r, Pal.Ink.g, Pal.Ink.b, 0.14f);
            float s = 0.9f;
            c.Ellipse(p.x, p.y, 2.6f * s, 2.1f * s, paw);
            c.Disc(p.x - 2.4f * s, p.y - 2.6f * s, 0.9f * s, paw);
            c.Disc(p.x, p.y - 3.1f * s, 0.9f * s, paw);
            c.Disc(p.x + 2.4f * s, p.y - 2.6f * s, 0.9f * s, paw);
        }

        // Cay bui quanh ria ban do.
        for (int i = 0; i < 34; i++)
        {
            float wx = Mathf.Lerp(-11.4f, 11.4f, (float)rnd.NextDouble());
            float wy = Mathf.Lerp(-6.8f, 6.8f, (float)rnd.NextDouble());
            if (MapLayout.MapRect.Contains(new Vector2(wx, wy))) continue;
            if (MapLayout.DistanceToPath(new Vector2(wx, wy)) < 1.1f) continue;
            if (wx > 6.4f && wy > -0.4f && wy < 3.4f) continue; // chua cho lau dai
            DrawBush(c, WorldToPixel(new Vector2(wx, wy)), 0.7f + (float)rnd.NextDouble() * 0.9f, rnd);
        }

        // Vai cay thong o goc.
        DrawPine(c, WorldToPixel(new Vector2(-10.6f, 6.3f)), 1.25f);
        DrawPine(c, WorldToPixel(new Vector2(10.4f, 6.4f)), 1.1f);
        DrawPine(c, WorldToPixel(new Vector2(-10.5f, -6.3f)), 1.0f);
        DrawPine(c, WorldToPixel(new Vector2(10.6f, -6.1f)), 1.2f);

        return c.ToSprite(PPC);
    }

    static void DrawBush(Canvas2D c, Vector2 p, float scale, System.Random rnd)
    {
        Color dark = Color.Lerp(Pal.GrassDark, Pal.Forest, 0.35f);
        Color mid = Color.Lerp(Pal.Grass, Pal.Forest, 0.15f);
        Color light = Color.Lerp(Pal.GrassLight, Pal.Grass, 0.4f);
        c.Ellipse(p.x, p.y + 3f * scale, 22f * scale, 8f * scale, new Color(0f, 0f, 0f, 0.18f));
        int blobs = 3 + rnd.Next(0, 3);
        for (int i = 0; i < blobs; i++)
        {
            float ox = (float)(rnd.NextDouble() * 2 - 1) * 15f * scale;
            float oy = (float)(rnd.NextDouble() * 2 - 1) * 6f * scale;
            c.Disc(p.x + ox, p.y + oy, (10f + (float)rnd.NextDouble() * 6f) * scale, dark);
        }
        for (int i = 0; i < blobs; i++)
        {
            float ox = (float)(rnd.NextDouble() * 2 - 1) * 13f * scale;
            float oy = (float)(rnd.NextDouble() * 2 - 1) * 5f * scale - 4f * scale;
            c.Disc(p.x + ox, p.y + oy, (7f + (float)rnd.NextDouble() * 5f) * scale, mid);
        }
        c.Disc(p.x - 4f * scale, p.y - 8f * scale, 5f * scale, light);
        c.Disc(p.x + 7f * scale, p.y - 6f * scale, 3.4f * scale, light);
    }

    static void DrawPine(Canvas2D c, Vector2 p, float s)
    {
        c.Ellipse(p.x, p.y + 4f * s, 20f * s, 7f * s, new Color(0f, 0f, 0f, 0.2f));
        c.RoundRect(p.x - 4f * s, p.y + 2f * s, 8f * s, 20f * s, 2f * s, Pal.C("#6b4a2f"));
        Color dark = Pal.C("#2f5b3a"), mid = Pal.C("#3d7447"), light = Pal.C("#4f8f54");
        c.Triangle(p.x, p.y - 46f * s, p.x - 26f * s, p.y - 4f * s, p.x + 26f * s, p.y - 4f * s, dark);
        c.Triangle(p.x, p.y - 46f * s, p.x - 21f * s, p.y - 4f * s, p.x + 21f * s, p.y - 4f * s, mid);
        c.Triangle(p.x, p.y - 30f * s, p.x - 18f * s, p.y + 8f * s, p.x + 18f * s, p.y + 8f * s, mid);
        c.Triangle(p.x, p.y - 14f * s, p.x - 13f * s, p.y + 18f * s, p.x + 13f * s, p.y + 18f * s, light);
    }

    static Sprite BuildCastle()
    {
        const int S = 300;
        var c = new Canvas2D(S, S);
        Color stone = Pal.C("#ecdfc4");
        Color stoneDark = Pal.C("#c9b993");
        Color stoneLight = Pal.C("#f8efd9");
        Color ink = Pal.C("#3a2c3f");
        Color gate = Pal.C("#4a3350");
        Color gateDark = Pal.C("#33223a");
        Color flag = Pal.C("#ff8fa3");

        c.Ellipse(150, 268, 120, 24, new Color(0f, 0f, 0f, 0.2f));
        c.Ellipse(150, 252, 112, 30, Pal.DirtDark);
        c.Ellipse(150, 246, 102, 24, Pal.Dirt);

        // Thanh chinh
        c.RoundRect(60, 136, 180, 116, 12, ink);
        c.RoundRect(64, 140, 172, 112, 10, stone);

        // Lo chau
        for (int i = 0; i < 5; i++)
        {
            float x = 68 + i * 34;
            c.RoundRect(x, 120, 22, 28, 4, ink);
            c.RoundRect(x + 3, 123, 16, 22, 3, stoneLight);
        }

        // Cua cong
        c.Disc(150, 196, 29, ink);
        c.RoundRect(121, 196, 58, 56, 6, ink);
        c.Disc(150, 199, 24, gate);
        c.RoundRect(126, 199, 48, 53, 4, gate);
        c.RoundRect(132, 210, 36, 42, 3, gateDark);
        for (int i = 0; i < 4; i++)
            c.Segment(136 + i * 9, 212, 136 + i * 9, 250, 2f, new Color(0.75f, 0.6f, 0.45f, 0.55f));
        c.Disc(150, 205, 5f, Pal.Gold);
        c.Disc(150, 205, 2.2f, Pal.C("#8a5a20"));

        // Thap hai ben voi tai meo
        DrawTower(c, 62, 150, stone, stoneLight, ink, gate, true);
        DrawTower(c, 238, 150, stone, stoneLight, ink, gate, false);

        // Cua so sang den
        foreach (var wp in new[] { new Vector2(104, 178), new Vector2(196, 178) })
        {
            c.RoundRect(wp.x - 8, wp.y - 12, 16, 24, 6, ink);
            c.RoundRect(wp.x - 5, wp.y - 9, 10, 18, 4, Pal.C("#ffd67a"));
            c.Disc(wp.x, wp.y + 72, 1f, ink);
        }

        // Huy hieu chan meo tren tuong
        c.Disc(150, 160, 15, Pal.Gold);
        c.Disc(150, 160, 11.5f, Pal.C("#ffe6a1"));
        c.Ellipse(150, 164, 5.5f, 4.5f, Pal.C("#b8802c"));
        c.Disc(145, 156, 2.2f, Pal.C("#b8802c"));
        c.Disc(150, 154, 2.2f, Pal.C("#b8802c"));
        c.Disc(155, 156, 2.2f, Pal.C("#b8802c"));

        // Rom co o chan thanh
        var rnd = new System.Random(5);
        for (int i = 0; i < 40; i++)
        {
            float wx = 70 + (float)rnd.NextDouble() * 160;
            c.Disc(wx, 248 + (float)rnd.NextDouble() * 8, 2 + (float)rnd.NextDouble() * 4, Pal.GrassDark);
        }

        return c.ToSprite(100f);
    }

    static void DrawTower(Canvas2D c, float cx, float baseY, Color stone, Color light, Color ink, Color gate, bool left)
    {
        // Than thap
        c.RoundRect(cx - 25, baseY + 40, 50, 62, 7, ink);
        c.RoundRect(cx - 21, baseY + 43, 42, 59, 5, stone);
        c.RoundRect(cx - 16, baseY + 58, 32, 40, 4, light);

        // Mui tai meo
        float earL = cx - 27, earR = cx + 27, tipY = baseY + 4;
        c.Triangle(earL - 3, baseY + 44, cx - 8, tipY - 4, cx + 3, baseY + 44, ink);
        c.Triangle(earR + 3, baseY + 44, cx + 8, tipY - 4, cx - 3, baseY + 44, ink);
        c.Triangle(earL + 2, baseY + 42, cx - 7, tipY + 1, cx + 1, baseY + 42, stone);
        c.Triangle(earR - 2, baseY + 42, cx + 7, tipY + 1, cx - 1, baseY + 42, stone);
        c.Triangle(earL + 6, baseY + 40, cx - 6, tipY + 8, cx - 1, baseY + 40, gate);
        c.Triangle(earR - 6, baseY + 40, cx + 6, tipY + 8, cx + 1, baseY + 40, gate);

        // Cua so
        c.RoundRect(cx - 7, baseY + 68, 14, 22, 5, ink);
        c.RoundRect(cx - 4, baseY + 71, 8, 16, 3, Pal.C("#ffd67a"));

        // Cot + co
        c.Segment(cx, tipY + 6, cx, tipY - 22, 2.5f, ink);
        float dir = left ? -1f : 1f;
        c.Triangle(cx, tipY - 21, cx + dir * 30, tipY - 14, cx, tipY - 5, Pal.Rose);
        c.Ellipse(cx + dir * 14, tipY - 14, 4f, 2.6f, Pal.Gold);
    }

    static Sprite BuildHole()
    {        const int S = 140;
        var c = new Canvas2D(S, S);
        Color dirt = Pal.C("#b27b46"), dirtLight = Pal.C("#d7a86c"), ink = Pal.C("#2a1e2c");

        c.Ellipse(70, 118, 52, 12, new Color(0f, 0f, 0f, 0.2f));
        c.Ellipse(70, 96, 48, 26, dirt);
        c.Ellipse(70, 92, 44, 22, dirtLight);
        c.Disc(70, 72, 34, ink);
        c.Ellipse(70, 84, 34, 22, ink);

        // Vien da quanh hang
        var rnd = new System.Random(9);
        for (int i = 0; i < 18; i++)
        {
            float a = -Mathf.PI * 0.1f - (float)rnd.NextDouble() * Mathf.PI * 0.8f;
            float x = 70 + Mathf.Cos(a) * (36 + (float)rnd.NextDouble() * 10);
            float y = 78 + Mathf.Sin(a) * (26 + (float)rnd.NextDouble() * 8);
            c.Ellipse(x, y, 4 + (float)rnd.NextDouble() * 4, 3 + (float)rnd.NextDouble() * 3, Pal.C("#9c6a3e"));
        }

        // Mat chuot loi ra
        c.Disc(84, 74, 3.2f, Pal.Gold);
        c.Disc(84.8f, 73.2f, 1.2f, Pal.Ink);

        return c.ToSprite(100f, new Vector2(0.5f, 0.4f));
    }

    static Sprite BuildSign()
    {
        const int W = 160, H = 130;
        var c = new Canvas2D(W, H);
        Color wood = Pal.C("#c08c52");
        Color woodDark = Pal.C("#8f6338");
        Color ink = Pal.C("#43302a");
        Color cream = Pal.C("#fff1d6");

        // Cot
        c.RoundRect(74, 52, 12, 78, 3, ink);
        c.RoundRect(77, 54, 7, 74, 2, woodDark);

        // Bien
        c.RoundRect(8, 10, 144, 58, 10, ink);
        c.RoundRect(11, 13, 138, 52, 8, wood);
        c.RoundRect(15, 17, 130, 44, 6, Color.Lerp(wood, Pal.Cream, 0.18f));

        // Dau chan meo
        Color paw = new Color(cream.r, cream.g, cream.b, 0.95f);
        c.Ellipse(52, 44, 13, 10, paw);
        c.Disc(38, 29, 5f, paw);
        c.Disc(50, 25, 5.2f, paw);
        c.Disc(63, 28, 5f, paw);
        c.Disc(71, 38, 3.2f, paw);

        // Mui ten chi huong
        c.Segment(88, 39, 126, 39, 6f, paw);
        c.Triangle(120, 25, 146, 39, 120, 53, paw);

        // Dinh
        c.Disc(17, 17, 2.2f, woodDark);
        c.Disc(143, 17, 2.2f, woodDark);

        return c.ToSprite(100f, new Vector2(0.5f, 0.1f));
    }
}
