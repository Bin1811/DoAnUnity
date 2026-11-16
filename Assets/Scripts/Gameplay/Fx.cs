using UnityEngine;

public static class Fx
{
    static Transform Root => GameManager.I != null ? GameManager.I.FxRoot : null;

    public static void Burst(Vector3 pos, Color color, int count, float size)
    {
        if (Root == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("spark");
            go.transform.SetParent(Root, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtUi.Spark;
            sr.color = color;
            sr.sortingOrder = Sorting.Fx;
            var p = go.AddComponent<FxParticle>();
            Vector2 dir = Random.insideUnitCircle.normalized;
            p.Setup(new Vector3(dir.x, dir.y + 0.6f, 0f) * Random.Range(1.2f, 3.4f) * size,
                Random.Range(0.32f, 0.55f), color, Random.Range(0.5f, 1f) * size);
        }
    }

    public static void Impact(Vector3 pos, Color color, float radius)
    {
        Fx.Burst(pos, color, 9, 0.35f);
        Ring(pos, radius, new Color(color.r, color.g, color.b, 0.9f));
    }

    public static void Ring(Vector3 pos, float radius, Color color)
    {
        if (Root == null) return;
        var go = new GameObject("ring");
        go.transform.SetParent(Root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ArtUi.RangeRing;
        sr.color = color;
        sr.sortingOrder = Sorting.Fx - 1;
        go.transform.localScale = Vector3.one * radius * 0.35f;
        var r = go.AddComponent<FxRing>();
        r.Setup(radius, 0.38f);
    }

    public static void Beam(Vector3 from, Vector3 to, Color color)
    {
        if (Root == null) return;
        Vector3 dir = to - from;
        float len = dir.magnitude;
        if (len < 0.01f) return;
        var go = new GameObject("beam");
        go.transform.SetParent(Root, false);
        go.transform.position = (from + to) * 0.5f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        go.transform.localScale = new Vector3(len, 0.75f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ArtUi.Beam;
        sr.color = color;
        sr.sortingOrder = Sorting.FxTop;
        var b = go.AddComponent<FxFade>();
        b.Setup(0.16f);
    }

    public static void BasePuff()
    {
        Vector3 pos = MapLayout.BasePoint + Vector2.left * 0.25f;
        Burst(pos, new Color(1f, 0.65f, 0.55f), 12, 0.55f);
        Ring(pos, 1.1f, new Color(1f, 0.45f, 0.4f, 0.9f));
        if (CameraShake.I != null) CameraShake.I.Shake(0.24f);
    }
}

public class FxParticle : MonoBehaviour
{
    Vector3 velocity;
    float life;
    float maxLife;
    Color color;
    float size;
    SpriteRenderer sr;

    public void Setup(Vector3 vel, float lifetime, Color tint, float scale)
    {
        velocity = vel;
        life = lifetime;
        maxLife = lifetime;
        color = tint;
        size = scale;
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        life -= dt;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += velocity * dt;
        velocity *= Mathf.Pow(0.12f, dt);
        velocity.y -= 3.2f * dt;
        float k = life / maxLife;
        Color c = color;
        c.a *= k;
        if (sr != null) sr.color = c;
        transform.localScale = Vector3.one * size * Mathf.Lerp(0.35f, 1f, k);
    }
}

public class FxRing : MonoBehaviour
{
    float targetRadius;
    float life;
    float maxLife;
    SpriteRenderer sr;

    public void Setup(float radius, float lifetime)
    {
        targetRadius = radius;
        life = lifetime;
        maxLife = lifetime;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        life -= dt;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        float k = 1f - life / maxLife;
        float radius = Mathf.Lerp(targetRadius * 0.35f, targetRadius * 1.15f, k);
        transform.localScale = Vector3.one * radius;
        if (sr != null)
        {
            Color c = sr.color;
            c.a *= (1f - k) * (1f - k);
            sr.color = c;
        }
    }
}

public class FxFade : MonoBehaviour
{
    float life;
    float maxLife;
    SpriteRenderer sr;

    public void Setup(float lifetime)
    {
        maxLife = lifetime;
        life = lifetime;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        if (sr != null)
        {
            Color c = sr.color;
            c.a *= life / maxLife;
            sr.color = c;
        }
    }
}
