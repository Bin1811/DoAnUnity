using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public EnemyDef Def;
    public float Hp;
    public float MaxHp;
    public float Distance;
    public bool IsDead;

    public Vector3 AimPoint => transform.position + Vector3.up * 0.12f;
    public static int AliveCount => All.Count;

    float slowAmount;
    float slowUntil;
    float flash;
    float phase;
    float wobble;
    bool facingLeft;
    SpriteRenderer body;
    Transform visual;
    SpriteRenderer hpBg;
    SpriteRenderer hpFill;
    Color baseColor;

    public void Init(EnemyDef def, int wave, Vector3 pos)
    {
        Def = def;
        MaxHp = def.Hp * Waves.HpMultiplier(wave);
        Hp = MaxHp;
        Distance = 0f;
        IsDead = false;
        transform.position = pos;
        phase = Random.Range(0f, 10f);
        wobble = Random.Range(0.8f, 1.2f);

        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        visual.localScale = Vector3.one * def.Scale;

        body = visual.gameObject.AddComponent<SpriteRenderer>();
        body.sprite = ArtChars.Mouse(def.Kind);
        baseColor = Color.white;
        body.color = baseColor;

        var shadow = new GameObject("Shadow").AddComponent<SpriteRenderer>();
        shadow.transform.SetParent(transform, false);
        shadow.sprite = ArtUi.Shadow;
        shadow.color = new Color(0f, 0f, 0f, 0.35f);
        shadow.transform.localPosition = new Vector3(0, -0.22f * def.Scale, 0);
        shadow.transform.localScale = new Vector3(0.5f * def.Scale, 0.5f * def.Scale, 1f);

        float barW = 0.78f * Mathf.Max(0.7f, def.Scale);
        hpBg = MakeBar("HpBg", new Color(0.08f, 0.05f, 0.1f, 0.8f), new Vector2(barW, 0.11f));
        hpFill = MakeBar("HpFill", new Color(0.45f, 0.9f, 0.5f), new Vector2(barW - 0.04f, 0.07f));
        hpFill.sprite = ArtUi.SolidLeft;
        hpFill.transform.localPosition = new Vector3(-(barW - 0.04f) * 0.5f, 0.02f, 0f);
        hpBg.transform.localPosition = new Vector3(0f, 0.02f, 0f);

        All.Add(this);
        ApplySorting();
    }

    SpriteRenderer MakeBar(string name, Color color, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ArtUi.Solid;
        sr.color = color;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        go.transform.localPosition = new Vector3(0f, 0.58f * Def.Scale + 0.12f, 0f);
        return sr;
    }

    void Update()
    {
        if (IsDead) return;
        if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

        float dt = Time.deltaTime;
        float slow = Time.time < slowUntil ? slowAmount : 0f;
        float speed = Def.Speed * (1f - slow);
        Distance += speed * dt;

        Vector2 pos = MapLayout.PointAtDistance(Distance, out Vector2 dir);
        float t = Time.time * 9f * wobble + phase;
        float bob = Mathf.Abs(Mathf.Sin(t)) * 0.05f;
        transform.position = new Vector3(pos.x, pos.y + bob, 0f);

        if (dir.x < -0.15f) facingLeft = true;
        else if (dir.x > 0.15f) facingLeft = false;
        if (visual != null)
        {
            visual.localScale = new Vector3((facingLeft ? -1f : 1f) * Def.Scale,
                Def.Scale * (1f + Mathf.Sin(t * 2f) * 0.03f), Def.Scale);
            visual.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t) * 2.5f);
        }

        if (flash > 0f) flash -= dt;
        Color tint = Color.white;
        if (slow > 0f) tint = Color.Lerp(Color.white, new Color(0.6f, 0.85f, 1f), 0.55f);
        if (flash > 0f) tint = Color.Lerp(tint, Color.white * 1.6f, 0.85f);
        if (body != null) body.color = tint;

        UpdateHealthBar();
        ApplySorting();

        if (Distance >= MapLayout.TotalLength)
        {
            ReachBase();
        }
    }

    void ApplySorting()
    {
        int order = Sorting.Order(transform.position.y);
        if (body != null) body.sortingOrder = order;
        if (hpBg != null) hpBg.sortingOrder = order + 6;
        if (hpFill != null) hpFill.sortingOrder = order + 7;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != body && sr != hpBg && sr != hpFill) sr.sortingOrder = order - 1;
        }
    }

    void UpdateHealthBar()
    {
        if (hpBg == null) return;
        bool show = Hp < MaxHp - 0.01f;
        hpBg.enabled = show;
        hpFill.enabled = show;
        if (!show) return;
        float frac = Mathf.Clamp01(Hp / MaxHp);
        float w = 0.74f * Mathf.Max(0.7f, Def.Scale);
        hpFill.transform.localScale = new Vector3(w * frac, 0.07f, 1f);
        hpFill.color = frac > 0.5f ? new Color(0.5f, 0.92f, 0.55f)
            : frac > 0.25f ? new Color(1f, 0.82f, 0.35f)
            : new Color(1f, 0.42f, 0.42f);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        Hp -= damage;
        flash = 0.07f;
        if (Hp <= 0f)
        {
            Hp = 0f;
            Die(true);
            return;
        }
        if (Random.value < 0.35f)
            Fx.Burst(AimPoint, new Color(1f, 0.95f, 0.8f, 0.9f), 2, 0.22f);
    }

    public void ApplySlow(float amount, float duration)
    {
        if (IsDead) return;
        amount *= (1f - Def.SlowResist);
        if (amount <= 0.01f) return;
        slowAmount = Mathf.Max(slowAmount, amount);
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
    }

    void ReachBase()
    {
        if (IsDead) return;
        GameManager.I.LoseLife(Def.Damage);
        Fx.BasePuff();
        AudioKit.I.Play("basehit", 1f, Random.Range(0.9f, 1.05f));
        IsDead = true;
        All.Remove(this);
        Destroy(gameObject);
    }

    public void Die(bool rewarded)
    {
        if (IsDead) return;
        IsDead = true;
        All.Remove(this);

        Color col = Def.Kind == EnemyKind.Boss ? Pal.C("#c79cff") : Pal.C("#ffd7a8");
        Fx.Burst(AimPoint, col, Def.Kind == EnemyKind.Boss ? 18 : 7, Def.Kind == EnemyKind.Boss ? 0.5f : 0.3f);
        if (Def.Kind == EnemyKind.Boss)
        {
            Fx.Ring(transform.position, 1.6f, new Color(1f, 0.8f, 0.4f, 0.9f));
            CameraShake.I?.Shake(0.28f);
            UIManager.I?.Toast("Hạ gục Chuột Đầu Đàn! +" + Mathf.RoundToInt(Def.Reward) + " cá", Pal.Gold);
            AudioKit.I.Play("bosskill", 1f, 1f);
        }
        else
        {
            AudioKit.I.Play("kill", 0.6f, Random.Range(0.9f, 1.35f));
        }

        if (rewarded)
        {
            GameManager.I.AddFish(Mathf.RoundToInt(Def.Reward));
            GameManager.I.Kills++;
            UIManager.I?.Float(transform.position + Vector3.up * 0.3f, "+" + Mathf.RoundToInt(Def.Reward),
                Pal.Gold, 26);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        All.Remove(this);
    }
}
