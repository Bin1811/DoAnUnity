using UnityEngine;

// Ve meo, chuot va dan bang code (khong can asset ngoai).
public static class ArtChars
{
    static readonly Sprite[] cats = new Sprite[4];
    static readonly Sprite[] mice = new Sprite[4];
    static Sprite yarn, ice, bubble;

    public static Sprite Cat(TowerKind kind)
    {
        int i = (int)kind;
        if (cats[i] == null) cats[i] = BuildCat(kind);
        return cats[i];
    }

    public static Sprite Mouse(EnemyKind kind)
    {
        int i = (int)kind;
        if (mice[i] == null) mice[i] = BuildMouse(kind);
        return mice[i];
    }

    public static Sprite Yarn => yarn ?? (yarn = BuildYarn());
    public static Sprite Ice => ice ?? (ice = BuildIce());
    public static Sprite Bubble => bubble ?? (bubble = BuildBubble());

    static Color Mul(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);

    static Sprite BuildCat(TowerKind kind)
    {
        const int S = 128;
        var c = new Canvas2D(S, S);
        var def = Catalog.Get(kind);
        Color coat = def.Coat;
        Color dark = def.CoatDark;
        Color accent = def.Accent;
        Color eye = def.Eye;
        Color ink = Color.Lerp(Pal.Ink, dark, 0.22f);
        Color muzzle = Color.Lerp(coat, Pal.Cream, 0.65f);
        Color pink = Pal.C("#ff9db4");

        // Duoi
        Color tailColor = kind == TowerKind.Siamese ? dark : coat;
        if (kind != TowerKind.British)
        {
            c.Segment(88, 100, 106, 88, 9f, ink);
            c.Segment(106, 88, 112, 66, 9f, ink);
            c.Segment(88, 100, 106, 88, 5f, tailColor);
            c.Segment(106, 88, 112, 66, 5f, tailColor);
        }
        else
        {
            c.Segment(92, 104, 110, 100, 8f, ink);
            c.Segment(92, 104, 110, 100, 4f, coat);
        }

        // Dem
        c.Ellipse(64, 121, 48, 12, ink);
        c.Ellipse(64, 118, 45, 10, accent);
        c.Ellipse(64, 116, 38, 7, Color.Lerp(accent, Color.white, 0.25f));
        Color stitch = new Color(ink.r, ink.g, ink.b, 0.45f);
        for (int i = 0; i < 7; i++)
        {
            float x = 30 + i * 11f;
            c.Segment(x, 116, x + 4, 122, 1.5f, stitch);
        }

        // Than
        c.Ellipse(64, 98, 31, 25, ink);
        c.Ellipse(64, 97, 28, 22, coat);
        c.Ellipse(64, 100, 18, 16, Color.Lerp(coat, Pal.Cream, 0.35f));

        // Chan
        Color pawColor = kind == TowerKind.Siamese ? Color.Lerp(dark, coat, 0.25f) : Color.Lerp(coat, Pal.Cream, 0.3f);
        c.Ellipse(47, 114, 11, 8, ink);
        c.Ellipse(81, 114, 11, 8, ink);
        c.Ellipse(47, 112, 9, 6, pawColor);
        c.Ellipse(81, 112, 9, 6, pawColor);

        if (kind == TowerKind.Persian)
        {
            Color fluff = Color.Lerp(coat, Color.white, 0.4f);
            for (int i = 0; i < 16; i++)
            {
                float a = Mathf.PI * 0.55f + i / 15f * Mathf.PI * 0.9f;
                c.Disc(64 + Mathf.Cos(a) * 36f, 62 + Mathf.Sin(a) * 32f, 9f, fluff);
            }
            for (int i = 0; i < 8; i++)
            {
                float a = Mathf.PI * 0.15f + i / 7f * Mathf.PI * 0.7f;
                c.Disc(64 + Mathf.Cos(a) * 26f, 102 + Mathf.Sin(a) * 18f, 7f, fluff);
            }
        }

        // Tai
        Color earFill = kind == TowerKind.Siamese ? dark : coat;
        DrawEar(c, 31, 46, 19, 6, 54, 24, ink, earFill, pink);
        DrawEar(c, 97, 46, 109, 6, 74, 24, ink, earFill, pink);

        // Dau
        c.Ellipse(64, 62, 41, 37, ink);
        c.Ellipse(64, 61, 38, 34, coat);
        if (kind == TowerKind.British)
        {
            c.Ellipse(32, 70, 16, 14, ink);
            c.Ellipse(32, 70, 14, 12, coat);
            c.Ellipse(96, 70, 16, 14, ink);
            c.Ellipse(96, 70, 14, 12, coat);
        }

        // Vet van
        if (kind == TowerKind.Tabby)
        {
            Color stripe = Mul(dark, 0.92f);
            c.Segment(52, 30, 56, 42, 4f, stripe);
            c.Segment(64, 27, 64, 41, 4f, stripe);
            c.Segment(76, 30, 72, 42, 4f, stripe);
            c.Segment(44, 44, 40, 55, 3.5f, stripe);
            c.Segment(84, 44, 88, 55, 3.5f, stripe);
            c.Segment(42, 92, 54, 96, 4f, stripe);
            c.Segment(86, 92, 74, 96, 4f, stripe);
        }
        else if (kind == TowerKind.Siamese)
        {
            Color mask = new Color(dark.r, dark.g, dark.b, 0.8f);
            c.Ellipse(64, 70, 32, 26, new Color(dark.r, dark.g, dark.b, 0.55f));
            c.Ellipse(64, 78, 27, 20, mask);
            c.Ellipse(30, 52, 10, 14, new Color(dark.r, dark.g, dark.b, 0.6f));
            c.Ellipse(98, 52, 10, 14, new Color(dark.r, dark.g, dark.b, 0.6f));
            c.Ellipse(64, 110, 22, 8, new Color(dark.r, dark.g, dark.b, 0.45f));
        }
        else if (kind == TowerKind.Persian)
        {
            Color fluff = Color.Lerp(coat, Color.white, 0.4f);
            c.Ellipse(64, 74, 12, 10, fluff);
        }

        // Mat
        c.Ellipse(48, 60, 9, 10, Pal.Cream);
        c.Ellipse(80, 60, 9, 10, Pal.Cream);
        if (kind == TowerKind.British)
        {
            c.Ellipse(48, 60, 6.5f, 8f, eye);
            c.Ellipse(48, 60, 2.3f, 5.5f, Pal.Ink);
            c.Disc(49.2f, 57.5f, 1.6f, Color.white);
            c.Segment(75, 61, 84, 59, 3f, ink);
        }
        else
        {
            c.Ellipse(48, 60, 7.5f, 8.5f, eye);
            c.Ellipse(80, 60, 7.5f, 8.5f, eye);
            c.Ellipse(48, 60, 2.6f, 6.2f, Pal.Ink);
            c.Ellipse(80, 60, 2.6f, 6.2f, Pal.Ink);
            c.Disc(50, 56.5f, 1.9f, Color.white);
            c.Disc(82, 56.5f, 1.9f, Color.white);
        }
        if (kind == TowerKind.British)
        {
            c.Ring(48, 61, 10.5f, 2f, Pal.Gold);
            c.Segment(57, 68, 64, 80, 1.2f, Pal.Gold);
        }

        // Mum, mui, mieng
        c.Ellipse(64, 80, 17, 12, muzzle);
        c.Triangle(60, 74, 68, 74, 64, 79, pink);
        c.Segment(64, 79, 64, 83, 1.6f, ink);
        c.Segment(64, 83, 58, 87, 1.6f, ink);
        c.Segment(64, 83, 70, 87, 1.6f, ink);
        c.Segment(64, 83, 64, 87, 1.4f, ink);

        // Ria
        Color whisk = new Color(ink.r, ink.g, ink.b, 0.55f);
        c.Segment(36, 78, 14, 73, 1.4f, whisk);
        c.Segment(36, 82, 13, 83, 1.4f, whisk);
        c.Segment(36, 86, 15, 92, 1.4f, whisk);
        c.Segment(92, 78, 114, 73, 1.4f, whisk);
        c.Segment(92, 82, 115, 83, 1.4f, whisk);
        c.Segment(92, 86, 113, 92, 1.4f, whisk);

        // Ma hong
        c.Disc(35, 76, 6f, new Color(1f, 0.55f, 0.65f, 0.3f));
        c.Disc(93, 76, 6f, new Color(1f, 0.55f, 0.65f, 0.3f));

        // Vong co + chuong
        Color collar = kind == TowerKind.Siamese ? Pal.C("#66c2ff") : accent;
        if (kind == TowerKind.Tabby) collar = Pal.C("#e2564b");
        c.Segment(38, 92, 90, 92, 6f, ink);
        c.Segment(38, 92, 90, 92, 3.5f, collar);
        c.Disc(64, 96, 5f, Pal.Gold);
        c.Disc(64, 96, 2f, Pal.C("#b8802c"));

        // Phu kien
        if (kind == TowerKind.Persian)
        {
            c.Triangle(30, 24, 8, 12, 20, 38, Pal.Rose);
            c.Triangle(30, 24, 52, 12, 40, 38, Pal.Rose);
            c.Disc(30, 25, 6f, Pal.C("#ff6f91"));
        }
        else if (kind == TowerKind.British)
        {
            // Mu beret
            c.Ellipse(46, 22, 25, 10, ink);
            c.Ellipse(46, 21, 23, 8, Pal.C("#3f6b4a"));
            c.Disc(60, 26, 5f, Pal.C("#558a5c"));
            c.Segment(45, 13, 50, 3, 2f, Pal.Gold);
            c.Disc(50, 3, 3f, Pal.Gold);
            c.Disc(40, 20, 4f, Pal.C("#ffd84d"));
        }

        return c.ToSprite(100f);
    }

    static void DrawEar(Canvas2D c, float ax, float ay, float bx, float by, float cx, float cy,
        Color ink, Color coat, Color inner)
    {
        c.Triangle(ax - 4, ay + 4, bx - 2, by - 3, cx + 3, cy + 3, ink);
        c.Triangle(ax, ay, bx, by, cx, cy, coat);
        c.Triangle(ax + 7, ay - 4, bx + 3, by + 7, cx - 7, cy, inner);
    }

    static Sprite BuildMouse(EnemyKind kind)
    {
        const int S = 100;
        var c = new Canvas2D(S, S);
        Color fur, furDark, earInner, eye;
        string eyeGlow = null;
        bool angry = false, crown = false, bandana = false;

        switch (kind)
        {
            case EnemyKind.Fast:
                fur = Pal.C("#f0e9dd"); furDark = Pal.C("#c9baa6"); earInner = Pal.Rose;
                eye = Pal.Ink; bandana = true; break;
            case EnemyKind.Rat:
                fur = Pal.C("#9c7a58"); furDark = Pal.C("#6d5038"); earInner = Pal.C("#e2a0a0");
                eye = Pal.Ink; angry = true; break;
            case EnemyKind.Boss:
                fur = Pal.C("#9a6bb0"); furDark = Pal.C("#6b447f"); earInner = Pal.Rose;
                eye = Pal.C("#ff5d5d"); crown = true; angry = true; break;
            default:
                fur = Pal.C("#bcb5c6"); furDark = Pal.C("#8d8599"); earInner = Pal.Rose;
                eye = Pal.Ink; break;
        }

        Color ink = Color.Lerp(Pal.Ink, furDark, 0.2f);
        Color belly = Color.Lerp(fur, Color.white, 0.5f);

        // Duoi
        c.Segment(23, 63, 8, 56, 7f, ink);
        c.Segment(8, 56, 12, 36, 7f, ink);
        c.Segment(23, 63, 8, 56, 3.5f, furDark);
        c.Segment(8, 56, 12, 36, 3.5f, furDark);
        c.Disc(12, 34, 2.2f, furDark);

        // Chan
        c.Ellipse(40, 78, 10, 6, ink);
        c.Ellipse(62, 78, 10, 6, ink);
        c.Ellipse(40, 77, 8, 4.4f, furDark);
        c.Ellipse(62, 77, 8, 4.4f, furDark);

        // Than
        c.Ellipse(46, 59, 31, 22, ink);
        c.Ellipse(46, 58, 28, 20, fur);
        c.Ellipse(42, 66, 18, 11, belly);

        if (bandana)
        {
            c.Triangle(52, 66, 30, 82, 34, 54, Pal.C("#e8564d"));
            c.Ellipse(58, 70, 16, 6, Pal.C("#e8564d"));
        }

        // Dau
        c.Disc(74, 49, 22, ink);
        c.Disc(74, 48, 20, fur);
        c.Ellipse(88, 55, 12, 9, fur);
        c.Ellipse(88, 55, 10, 7, belly);

        // Tai
        c.Disc(68, 27, 14, ink);
        c.Disc(68, 27, 12, furDark);
        c.Disc(68, 26, 8, earInner);

        // Mat
        c.Disc(77, 44, 4.2f, eye);
        c.Disc(78, 42.6f, 1.5f, Color.white);
        if (eyeGlow == null && kind == EnemyKind.Boss)
        {
            c.Disc(79, 43, 1.2f, Pal.C("#ffd0d0"));
        }
        if (angry)
        {
            c.Segment(70, 36, 83, 39, 3f, ink);
            if (kind == EnemyKind.Rat) c.Segment(64, 32, 70, 42, 2f, Pal.C("#e0b0a0"));
        }
        if (crown)
        {
            c.RoundRect(55, 17, 34, 8, 2, Pal.Gold);
            c.Triangle(58, 18, 64, 4, 70, 18, Pal.Gold);
            c.Triangle(68, 18, 74, 6, 80, 18, Pal.Gold);
            c.Triangle(78, 18, 84, 8, 88, 18, Pal.Gold);
            c.Disc(64, 8, 2.5f, Pal.C("#ff8fa3"));
            c.Disc(89, 8, 2f, Pal.C("#ffe6a1"));
        }

        // Mui + ria
        c.Disc(94, 54, 3.4f, Pal.Rose);
        Color whisk = new Color(ink.r, ink.g, ink.b, 0.5f);
        c.Segment(96, 57, 99, 62, 1.2f, whisk);
        c.Segment(95, 60, 98, 68, 1.2f, whisk);
        c.Segment(95, 62, 96, 70, 1.2f, whisk);

        // Vet seo cho chuot cong
        if (kind == EnemyKind.Rat)
        {
            c.Segment(60, 62, 66, 68, 1.6f, Pal.C("#f0c0b0"));
            c.Segment(66, 62, 60, 68, 1.6f, Pal.C("#f0c0b0"));
        }

        return c.ToSprite(100f);
    }

    static Sprite BuildYarn()
    {
        const int S = 48;
        var c = new Canvas2D(S, S);
        c.Disc(24, 24, 17, Pal.C("#7a2f45"));
        c.Disc(24, 24, 15, Pal.Rose);
        c.Ring(24, 24, 10, 1.6f, new Color(1f, 1f, 1f, 0.55f));
        c.Segment(12, 15, 36, 33, 1.6f, new Color(1f, 1f, 1f, 0.4f));
        c.Segment(14, 33, 34, 15, 1.6f, new Color(0.5f, 0.1f, 0.2f, 0.4f));
        c.Disc(19, 19, 4f, new Color(1f, 1f, 1f, 0.35f));
        c.Segment(4, 38, 12, 32, 1.4f, new Color(1f, 0.75f, 0.8f, 0.7f));
        return c.ToSprite(100f);
    }

    static Sprite BuildIce()
    {
        const int S = 48;
        var c = new Canvas2D(S, S);
        c.Triangle(24, 2, 43, 24, 24, 46, new Color(0.55f, 0.85f, 1f, 0.35f));
        c.Triangle(24, 2, 5, 24, 24, 46, new Color(0.55f, 0.85f, 1f, 0.35f));
        c.Triangle(24, 7, 38, 24, 24, 41, Pal.C("#a9e6ff"));
        c.Triangle(24, 7, 10, 24, 24, 41, Pal.C("#79cdf7"));
        c.Triangle(24, 14, 31, 24, 24, 34, new Color(1f, 1f, 1f, 0.75f));
        c.Disc(19, 19, 2.5f, Color.white);
        return c.ToSprite(100f);
    }

    static Sprite BuildBubble()
    {
        const int S = 48;
        var c = new Canvas2D(S, S);
        c.Disc(24, 24, 18, new Color(1f, 0.65f, 0.85f, 0.22f));
        c.Ring(24, 24, 17, 2.4f, new Color(1f, 0.8f, 0.92f, 0.85f));
        c.Disc(17, 17, 5f, new Color(1f, 1f, 1f, 0.55f));
        c.Disc(33, 31, 2.5f, new Color(1f, 1f, 1f, 0.4f));
        c.Disc(30, 13, 1.8f, new Color(1f, 1f, 1f, 0.35f));
        return c.ToSprite(100f);
    }
}
