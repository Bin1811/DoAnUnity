using UnityEngine;

// Bang mau dung chung cho ca game.
public static class Pal
{
    public static Color C(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    public static readonly Color Forest = C("#1b2a21");
    public static readonly Color Grass = C("#7cbe66");
    public static readonly Color GrassDark = C("#5fa04d");
    public static readonly Color GrassLight = C("#9ad67c");
    public static readonly Color Dirt = C("#d7a86c");
    public static readonly Color DirtDark = C("#b27b46");
    public static readonly Color DirtLight = C("#efc791");
    public static readonly Color Ink = C("#2b2033");
    public static readonly Color InkSoft = C("#43324d");
    public static readonly Color Cream = C("#fff1d6");
    public static readonly Color Gold = C("#ffc94d");
    public static readonly Color Rose = C("#ff8fa3");
    public static readonly Color Sky = C("#8fd3ff");
    public static readonly Color Mint = C("#8ce0b0");
    public static readonly Color Danger = C("#ff6b6b");

    public static readonly Color UiPanel = new Color(0.15f, 0.11f, 0.17f, 0.9f);
    public static readonly Color UiCard = new Color(0.26f, 0.2f, 0.29f, 0.95f);
    public static readonly Color UiCardSel = new Color(0.42f, 0.3f, 0.16f, 0.98f);
    public static readonly Color UiText = new Color(1f, 0.95f, 0.86f, 1f);
    public static readonly Color UiTextDim = new Color(0.82f, 0.76f, 0.72f, 1f);
}

// Bo ve pixel don gian, co khu rang cang (supersampling) de vien muot.
public class Canvas2D
{
    public readonly int W;
    public readonly int H;
    public readonly Color32[] Px;

    public Canvas2D(int w, int h)
    {
        W = w;
        H = h;
        Px = new Color32[w * h];
    }

    public Canvas2D(int w, int h, Color fill) : this(w, h)
    {
        Fill(fill);
    }

    public void Fill(Color c)
    {
        Color32 c32 = c;
        for (int i = 0; i < Px.Length; i++) Px[i] = c32;
    }

    public void Blend(int x, int y, Color c, float cov)
    {
        if (x < 0 || y < 0 || x >= W || y >= H || cov <= 0f) return;
        float sa = c.a * cov;
        if (sa <= 0.002f) return;

        int i = y * W + x;
        Color32 d = Px[i];
        float da = d.a / 255f;
        float oa = sa + da * (1f - sa);
        if (oa <= 0.002f) return;

        float inv = 1f / oa;
        float r = (c.r * sa + d.r / 255f * da * (1f - sa)) * inv;
        float g = (c.g * sa + d.g / 255f * da * (1f - sa)) * inv;
        float b = (c.b * sa + d.b / 255f * da * (1f - sa)) * inv;
        Px[i] = new Color32((byte)(Mathf.Clamp01(r) * 255f), (byte)(Mathf.Clamp01(g) * 255f),
            (byte)(Mathf.Clamp01(b) * 255f), (byte)(Mathf.Clamp01(oa) * 255f));
    }

    // Ve mot hinh bang cach lay mau 3x3 diem trong moi pixel.
    public void Shape(System.Func<float, float, bool> inside, float x0, float y0, float x1, float y1, Color c)
    {
        int ix0 = Mathf.Max(0, Mathf.FloorToInt(x0));
        int iy0 = Mathf.Max(0, Mathf.FloorToInt(y0));
        int ix1 = Mathf.Min(W - 1, Mathf.CeilToInt(x1));
        int iy1 = Mathf.Min(H - 1, Mathf.CeilToInt(y1));
        for (int y = iy0; y <= iy1; y++)
        {
            for (int x = ix0; x <= ix1; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < 3; sy++)
                {
                    for (int sx = 0; sx < 3; sx++)
                    {
                        if (inside(x + (sx + 0.5f) / 3f, y + (sy + 0.5f) / 3f)) hit++;
                    }
                }
                if (hit > 0) Blend(x, y, c, hit / 9f);
            }
        }
    }

    public void Disc(float cx, float cy, float r, Color c)
    {
        float r2 = r * r;
        Shape((x, y) =>
        {
            float dx = x - cx, dy = y - cy;
            return dx * dx + dy * dy <= r2;
        }, cx - r, cy - r, cx + r, cy + r, c);
    }

    public void Ellipse(float cx, float cy, float rx, float ry, Color c)
    {
        if (rx <= 0 || ry <= 0) return;
        float rx2 = rx * rx, ry2 = ry * ry;
        Shape((x, y) =>
        {
            float dx = x - cx, dy = y - cy;
            return dx * dx / rx2 + dy * dy / ry2 <= 1f;
        }, cx - rx, cy - ry, cx + rx, cy + ry, c);
    }

    public void Ring(float cx, float cy, float r, float width, Color c)
    {
        float inner = r - width * 0.5f, outer = r + width * 0.5f;
        float in2 = inner * inner, out2 = outer * outer;
        Shape((x, y) =>
        {
            float dx = x - cx, dy = y - cy;
            float d2 = dx * dx + dy * dy;
            return d2 >= in2 && d2 <= out2;
        }, cx - outer, cy - outer, cx + outer, cy + outer, c);
    }

    public void Segment(float ax, float ay, float bx, float by, float width, Color c)
    {
        float r = width * 0.5f;
        Shape((x, y) =>
        {
            float abx = bx - ax, aby = by - ay;
            float len2 = abx * abx + aby * aby;
            float t = 0f;
            if (len2 > 0.00001f) t = Mathf.Clamp01(((x - ax) * abx + (y - ay) * aby) / len2);
            float px = ax + abx * t, py = ay + aby * t;
            float dx = x - px, dy = y - py;
            return dx * dx + dy * dy <= r * r;
        }, Mathf.Min(ax, bx) - r, Mathf.Min(ay, by) - r, Mathf.Max(ax, bx) + r, Mathf.Max(ay, by) + r, c);
    }

    public void Triangle(float ax, float ay, float bx, float by, float cx, float cy, Color c)
    {
        float minX = Mathf.Min(ax, Mathf.Min(bx, cx));
        float minY = Mathf.Min(ay, Mathf.Min(by, cy));
        float maxX = Mathf.Max(ax, Mathf.Max(bx, cx));
        float maxY = Mathf.Max(ay, Mathf.Max(by, cy));
        Shape((x, y) =>
        {
            float d1 = (x - bx) * (ay - by) - (ax - bx) * (y - by);
            float d2 = (x - cx) * (by - cy) - (bx - cx) * (y - cy);
            float d3 = (x - ax) * (cy - ay) - (cx - ax) * (y - ay);
            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }, minX, minY, maxX, maxY, c);
    }

    public void RoundRect(float x, float y, float w, float h, float radius, Color c)
    {
        float x1 = x + w, y1 = y + h;
        float r = Mathf.Min(radius, Mathf.Min(w, h) * 0.5f);
        Shape((px, py) =>
        {
            float qx = Mathf.Max(x + r - px, px - (x1 - r));
            float qy = Mathf.Max(y + r - py, py - (y1 - r));
            float dx = Mathf.Max(qx, 0f), dy = Mathf.Max(qy, 0f);
            return dx * dx + dy * dy <= r * r;
        }, x, y, x1, y1, c);
    }

    public void SoftDisc(float cx, float cy, float r, Color inner, Color outer)
    {
        int ix0 = Mathf.Max(0, Mathf.FloorToInt(cx - r));
        int iy0 = Mathf.Max(0, Mathf.FloorToInt(cy - r));
        int ix1 = Mathf.Min(W - 1, Mathf.CeilToInt(cx + r));
        int iy1 = Mathf.Min(H - 1, Mathf.CeilToInt(cy + r));
        for (int y = iy0; y <= iy1; y++)
        {
            for (int x = ix0; x <= ix1; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / r;
                if (d > 1f) continue;
                float t = Mathf.SmoothStep(0f, 1f, d);
                Color c = Color.Lerp(inner, outer, t);
                c.a = Mathf.Lerp(inner.a, outer.a, t);
                Blend(x, y, c, 1f);
            }
        }
    }

    public Texture2D ToTexture()
    {
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        var flip = new Color32[W * H];
        for (int y = 0; y < H; y++)
        {
            System.Array.Copy(Px, y * W, flip, (H - 1 - y) * W, W);
        }
        tex.SetPixels32(flip);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    public Sprite ToSprite(float ppu, Vector2? pivot = null, Vector4 border = default)
    {
        var tex = ToTexture();
        var sprite = Sprite.Create(tex, new Rect(0, 0, W, H), pivot ?? new Vector2(0.5f, 0.5f), ppu, 0,
            SpriteMeshType.FullRect, border);
        return sprite;
    }
}
