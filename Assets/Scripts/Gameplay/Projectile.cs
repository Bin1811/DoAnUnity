using UnityEngine;

public class Projectile : MonoBehaviour
{
    Enemy target;
    TowerLevel stats;
    string type;
    Color color;
    Vector3 lastPos;
    float speed;
    float life;
    float spin;
    SpriteRenderer sr;

    public static void Spawn(Vector3 from, Enemy target, TowerLevel stats, string type, Color tint)
    {
        if (GameManager.I == null || target == null) return;
        var go = new GameObject("Projectile");
        go.transform.SetParent(GameManager.I.ProjectilesRoot, false);
        go.transform.position = from;
        var p = go.AddComponent<Projectile>();
        p.Setup(target, stats, type, tint);
    }

    void Setup(Enemy enemy, TowerLevel level, string projectileType, Color tint)
    {
        target = enemy;
        stats = level;
        type = projectileType;
        lastPos = enemy != null ? enemy.AimPoint : transform.position;
        color = tint;
        speed = type == "bubble" ? 8.5f : type == "yarn" ? 11f : 13f;
        spin = type == "yarn" ? 520f : type == "ice" ? 200f : 90f;

        sr = gameObject.AddComponent<SpriteRenderer>();
        switch (type)
        {
            case "yarn": sr.sprite = ArtChars.Yarn; break;
            case "ice": sr.sprite = ArtChars.Ice; break;
            default: sr.sprite = ArtChars.Bubble; break;
        }
        sr.sortingOrder = Sorting.Projectile;
        transform.localScale = Vector3.one * (type == "bubble" ? 0.44f : 0.36f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        life += dt;

        Vector3 tp = (target != null && !target.IsDead) ? target.AimPoint : lastPos;
        lastPos = tp;

        transform.position = Vector3.MoveTowards(transform.position, tp, speed * dt);
        transform.Rotate(0f, 0f, spin * dt);

        if ((transform.position - tp).sqrMagnitude < 0.004f)
        {
            Hit();
            return;
        }
        if (life > 2.5f)
        {
            Destroy(gameObject);
        }
    }

    void Hit()
    {
        Vector3 pos = transform.position;
        if (stats.Splash > 0f)
        {
            Fx.Impact(pos, color, stats.Splash);
            var copy = Enemy.All.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                var e = copy[i];
                if (e == null || e.IsDead) continue;
                float d = Vector2.Distance(e.transform.position, pos);
                if (d > stats.Splash) continue;
                float falloff = Mathf.Lerp(1f, 0.6f, d / Mathf.Max(0.01f, stats.Splash));
                e.TakeDamage(stats.Damage * falloff);
                if (stats.Slow > 0f) e.ApplySlow(stats.Slow, stats.SlowTime);
            }
            AudioKit.I.Play("bubble", 0.7f, 0.85f);
        }
        else
        {
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(stats.Damage);
                if (stats.Slow > 0f) target.ApplySlow(stats.Slow, stats.SlowTime);
            }
            Fx.Burst(pos, color, 4, 0.26f);
            if (stats.Slow > 0f)
                Fx.Ring(pos, 0.55f, new Color(0.65f, 0.9f, 1f, 0.85f));
            AudioKit.I.Play("hit", 0.5f, Random.Range(0.9f, 1.1f));
        }
        Destroy(gameObject);
    }
}
