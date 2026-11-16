using UnityEngine;

// Sprite dung chung: hieu ung, icon, khung UI.
public static class ArtUi
{
    static Sprite solid, solidLeft, shadow, ring, cell, spark, beam;
    static Sprite heart, fish, star, pause, play, speed, sound, soundOff;
    static Sprite rounded, roundedOutline, circle, glow;

    public static Sprite Solid => solid ?? (solid = MakeSolid(new Vector2(0.5f, 0.5f)));
    public static Sprite SolidLeft => solidLeft ?? (solidLeft = MakeSolid(new Vector2(0f, 0.5f)));

    public static Sprite Shadow => shadow ?? (shadow = BuildShadow());
    public static Sprite RangeRing => ring ?? (ring = BuildRing());
    public static Sprite Cell => cell ?? (cell = BuildCell());
    public static Sprite Spark => spark ?? (spark = BuildSpark());
    public static Sprite Beam => beam ?? (beam = BuildBeam());
    public static Sprite Glow => glow ?? (glow = BuildGlow());

    public static Sprite Heart => heart ?? (heart = BuildHeart());
    public static Sprite Fish => fish ?? (fish = BuildFish());
    public static Sprite Star => star ?? (star = BuildStar());
    public static Sprite Pause => pause ?? (pause = BuildPause());
    public static Sprite Play => play ?? (play = BuildPlay());
    public static Sprite Speed => speed ?? (speed = BuildSpeed());
    public static Sprite Sound => sound ?? (sound = BuildSound(false));
    public static Sprite SoundOff => soundOff ?? (soundOff = BuildSound(true));

    public static Sprite Rounded => rounded ?? (rounded = BuildRounded(false));
    public static Sprite RoundedOutline => roundedOutline ?? (roundedOutline = BuildRounded(true));
    public static Sprite Circle => circle ?? (circle = BuildCircle());

    static Sprite MakeSolid(Vector2 pivot)
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color32[16];
        for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(px);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), pivot, 4f, 0, SpriteMeshType.FullRect);
    }

    static Sprite BuildShadow()
    {
        const int W = 128, H = 64;
        var c = new Canvas2D(W, H);
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float dx = (x - W * 0.5f) / (W * 0.5f);
                float dy = (y - H * 0.5f) / (H * 0.5f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d) * 0.5f;
                if (a > 0f) c.Blend(x, y, new Color(0f, 0f, 0f, a), 1f);
            }
        }
        return c.ToSprite(100f);
    }

    static Sprite BuildRing()
    {
        const int S = 256;
        var c = new Canvas2D(S, S);
        c.Disc(128, 128, 118, new Color(1f, 1f, 1f, 0.06f));
        c.Ring(128, 128, 118, 3f, new Color(1f, 1f, 1f, 0.55f));
        c.Ring(128, 128, 112, 1f, new Color(1f, 1f, 1f, 0.18f));
        return c.ToSprite(128f);
    }

    static Sprite BuildCell()
    {
        const int S = 100;
        var c = new Canvas2D(S, S);
        c.RoundRect(3, 3, S - 6, S - 6, 16, new Color(1f, 1f, 1f, 0.16f));
        c.RoundRect(2.5f, 2.5f, S - 5, S - 5, 16, new Color(1f, 1f, 1f, 0.85f));
        c.RoundRect(6, 6, S - 12, S - 12, 13, new Color(1f, 1f, 1f, 0.16f));
        return c.ToSprite(100f);
    }

    static Sprite BuildSpark()
    {
        const int S = 64;
        var c = new Canvas2D(S, S);
        c.SoftDisc(32, 32, 30, new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0f));
        return c.ToSprite(64f);
    }

    static Sprite BuildGlow()
    {
        const int S = 128;
        var c = new Canvas2D(S, S);
        c.SoftDisc(64, 64, 62, new Color(1f, 1f, 1f, 0.5f), new Color(1f, 1f, 1f, 0f));
        return c.ToSprite(128f);
    }

    static Sprite BuildBeam()
    {
        const int W = 64, H = 16;
        var c = new Canvas2D(W, H);
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float fy = 1f - Mathf.Abs((y + 0.5f) / H * 2f - 1f);
                float fx = Mathf.Clamp01(Mathf.Min(x, W - 1 - x) / 6f);
                float a = Mathf.Pow(fy, 1.4f) * fx;
                if (a > 0.01f) c.Blend(x, y, new Color(1f, 1f, 1f, a), 1f);
            }
        }
        return c.ToSprite(64f);
    }

    static Sprite BuildHeart()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        Color ink = Pal.C("#7c2a3a");
        Color red = Pal.C("#ff5c72");
        c.Disc(32, 36, 20, ink); c.Disc(64, 36, 20, ink);
        c.Triangle(9, 44, 87, 44, 48, 88, ink);
        c.Disc(32, 35, 16, red); c.Disc(64, 35, 16, red);
        c.Triangle(15, 44, 81, 44, 48, 80, red);
        c.Disc(30, 30, 6, new Color(1f, 1f, 1f, 0.55f));
        c.Disc(40, 44, 3f, new Color(1f, 1f, 1f, 0.3f));
        return c.ToSprite(100f);
    }

    static Sprite BuildFish()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        Color ink = Pal.C("#8a5a20");
        Color body = Pal.C("#7fd4ff");
        Color light = Pal.C("#c9f0ff");
        c.Ellipse(52, 50, 32, 21, ink);
        c.Triangle(24, 50, 3, 27, 3, 73, ink);
        c.Ellipse(52, 49, 29, 18, body);
        c.Triangle(24, 49, 6, 29, 6, 69, body);
        c.Ellipse(52, 53, 22, 11, light);
        c.Triangle(52, 31, 44, 14, 64, 31, body);
        c.Triangle(52, 31, 44, 16, 62, 31, ink);
        c.Disc(72, 44, 4.5f, ink);
        c.Disc(73, 42.7f, 1.8f, Color.white);
        c.Segment(46, 50, 58, 50, 1.5f, new Color(1f, 1f, 1f, 0.4f));
        return c.ToSprite(100f);
    }

    static Sprite BuildStar()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        DrawStar(c, 48, 50, 42, 18, 5, Pal.C("#c08a22"));
        DrawStar(c, 48, 50, 36, 15, 5, Pal.Gold);
        DrawStar(c, 48, 48, 16, 7, 5, new Color(1f, 0.95f, 0.7f, 0.75f));
        return c.ToSprite(100f);
    }

    public static void DrawStar(Canvas2D c, float cx, float cy, float outer, float inner, int points, Color color)
    {
        var verts = new Vector2[points * 2];
        for (int i = 0; i < verts.Length; i++)
        {
            float ang = -Mathf.PI * 0.5f + i * Mathf.PI / points;
            float r = i % 2 == 0 ? outer : inner;
            verts[i] = new Vector2(cx + Mathf.Cos(ang) * r, cy + Mathf.Sin(ang) * r);
        }
        for (int i = 0; i < verts.Length; i++)
        {
            Vector2 a = verts[i];
            Vector2 b = verts[(i + 1) % verts.Length];
            c.Triangle(cx, cy, a.x, a.y, b.x, b.y, color);
        }
    }

    static Sprite BuildPause()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        c.RoundRect(26, 20, 16, 56, 6, Color.white);
        c.RoundRect(54, 20, 16, 56, 6, Color.white);
        return c.ToSprite(100f);
    }

    static Sprite BuildPlay()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        c.Triangle(28, 16, 28, 80, 82, 48, Color.white);
        return c.ToSprite(100f);
    }

    static Sprite BuildSpeed()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        c.Triangle(10, 22, 10, 74, 48, 48, Color.white);
        c.Triangle(44, 22, 44, 74, 82, 48, Color.white);
        return c.ToSprite(100f);
    }

    static Sprite BuildSound(bool off)
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        c.RoundRect(18, 38, 16, 20, 3, Color.white);
        c.Triangle(34, 38, 58, 20, 58, 76, Color.white);
        c.Triangle(34, 58, 58, 58, 58, 76, Color.white);
        if (off)
        {
            c.Segment(66, 34, 84, 62, 5f, Pal.Danger);
            c.Segment(84, 34, 66, 62, 5f, Pal.Danger);
        }
        else
        {
            c.Ring(52, 48, 28, 4f, new Color(1f, 1f, 1f, 0.9f));
            c.Ring(52, 48, 18, 4f, new Color(1f, 1f, 1f, 0.9f));
        }
        return c.ToSprite(100f);
    }

    static Sprite BuildRounded(bool outlineOnly)
    {
        const int S = 64;
        var c = new Canvas2D(S, S);
        const float r = 18f;
        if (outlineOnly)
        {
            c.Shape((x, y) =>
            {
                float qx = Mathf.Abs(x - S * 0.5f) - (S * 0.5f - r - 1f);
                float qy = Mathf.Abs(y - S * 0.5f) - (S * 0.5f - r - 1f);
                float dx = Mathf.Max(qx, 0f), dy = Mathf.Max(qy, 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
                return Mathf.Abs(d) <= 1.3f;
            }, 0, 0, S, S, new Color(1f, 1f, 1f, 0.95f));
        }
        else
        {
            c.RoundRect(1, 1, S - 2, S - 2, r, Color.white);
        }
        return c.ToSprite(100f, new Vector2(0.5f, 0.5f), new Vector4(20, 20, 20, 20));
    }

    static Sprite BuildCircle()
    {
        const int S = 96;
        var c = new Canvas2D(S, S);
        c.Disc(48, 48, 46, Color.white);
        return c.ToSprite(100f);
    }
}
