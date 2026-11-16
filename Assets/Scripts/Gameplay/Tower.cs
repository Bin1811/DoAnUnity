using UnityEngine;

public static class Sorting
{
    public static int Order(float y) => Mathf.RoundToInt((12f - y) * 10f);
    public const int Projectile = 620;
    public const int Fx = 820;
    public const int FxTop = 900;
}

public class Tower : MonoBehaviour
{
    public TowerKind Kind;
    public int Level = 1;
    public int Invested;
    public Vector2Int Cell;

    TowerDef def;
    SpriteRenderer catRenderer;
    Transform cat;
    SpriteRenderer[] pips;
    float cooldown;
    float punch;
    float scaleSmooth = 0.35f;
    float phase;

    public TowerDef Def => def;
    public TowerLevel Stats => def.Stats(Level);
    public float Range => Stats.Range;
    public bool MaxLevel => Level >= 3;
    public int UpgradePrice => MaxLevel ? 0 : def.UpgradeCost[Level - 1];
    public int SellValue => Mathf.FloorToInt(Invested * 0.6f);
    public string TypeName => def.Name;

    public void Init(TowerKind kind, Vector2Int cell)
    {
        Kind = kind;
        Cell = cell;
        def = Catalog.Get(kind);
        Invested = def.Cost;
        phase = Random.Range(0f, 10f);
        transform.position = MapLayout.CellCenter(cell);
        int order = Sorting.Order(transform.position.y);

        var shadow = new GameObject("Shadow").AddComponent<SpriteRenderer>();
        shadow.transform.SetParent(transform, false);
        shadow.sprite = ArtUi.Shadow;
        shadow.color = new Color(0f, 0f, 0f, 0.32f);
        shadow.transform.localPosition = new Vector3(0f, -0.34f, 0f);
        shadow.transform.localScale = new Vector3(0.9f, 0.62f, 1f);
        shadow.sortingOrder = order - 1;

        cat = new GameObject("Cat").transform;
        cat.SetParent(transform, false);
        cat.localPosition = new Vector3(0f, 0.05f, 0f);
        cat.localScale = Vector3.one * 0.35f;
        catRenderer = cat.gameObject.AddComponent<SpriteRenderer>();
        catRenderer.sprite = ArtChars.Cat(kind);
        catRenderer.sortingOrder = order;

        pips = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            var pip = new GameObject("Pip" + i).AddComponent<SpriteRenderer>();
            pip.transform.SetParent(transform, false);
            pip.transform.localPosition = new Vector3((i - 1) * 0.16f, -0.43f, 0f);
            pip.transform.localScale = Vector3.one * 0.135f;
            pip.sprite = ArtUi.Star;
            pip.sortingOrder = order + 4;
            pips[i] = pip;
        }
        RefreshPips();
    }

    void RefreshPips()
    {
        if (pips == null) return;
        for (int i = 0; i < pips.Length; i++)
        {
            bool on = i < Level;
            pips[i].color = on ? Color.white : new Color(0.25f, 0.2f, 0.28f, 0.75f);
        }
    }

    void Update()
    {
        if (def == null) return;
        float dt = Time.deltaTime;
        punch = Mathf.Max(0f, punch - dt * 5f);
        scaleSmooth = Mathf.Lerp(scaleSmooth, 1f, dt * 10f);
        float breath = 1f + Mathf.Sin(Time.time * 2.2f + phase) * 0.012f;
        float s = scaleSmooth * breath * (1f + punch * 0.16f);
        if (cat != null)
        {
            cat.localScale = new Vector3(s, s, 1f);
            cat.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 1.6f + phase) * 1.6f);
        }

        if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

        cooldown -= dt;
        if (cooldown > 0f) return;

        Enemy target = FindTarget();
        if (target == null) return;

        cooldown = Stats.Cooldown;
        Fire(target);
    }

    Enemy FindTarget()
    {
        Enemy best = null;
        float bestProgress = -1f;
        float range = Stats.Range;
        var list = Enemy.All;
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (e == null || e.IsDead) continue;
            Vector2 d = e.transform.position - transform.position;
            if (d.sqrMagnitude > range * range) continue;
            if (e.Distance > bestProgress)
            {
                bestProgress = e.Distance;
                best = e;
            }
        }
        return best;
    }

    void Fire(Enemy target)
    {
        punch = 1f;
        Vector3 from = transform.position + new Vector3(0f, 0.12f, 0f);
        Color tint = def.Accent;

        if (def.Projectile == "sniper")
        {
            Vector3 to = target.AimPoint;
            Fx.Beam(from, to, new Color(1f, 0.95f, 0.65f, 0.95f));
            Fx.Burst(to, new Color(1f, 0.9f, 0.55f, 1f), 4, 0.25f);
            AudioKit.I.Play("sniper", 1f, Random.Range(0.95f, 1.05f));
            target.TakeDamage(Stats.Damage);
            return;
        }

        Projectile.Spawn(from, target, Stats, def.Projectile, tint);
        switch (def.Projectile)
        {
            case "yarn": AudioKit.I.Play("shoot", 0.75f, Random.Range(0.95f, 1.12f)); break;
            case "ice": AudioKit.I.Play("ice", 0.8f, Random.Range(0.95f, 1.1f)); break;
            case "bubble": AudioKit.I.Play("bubble", 0.85f, Random.Range(0.9f, 1.1f)); break;
        }
    }

    public bool Upgrade()
    {
        if (MaxLevel)
        {
            UIManager.I?.Toast("Đã đạt cấp tối đa!", Pal.UiTextDim);
            return false;
        }
        int price = UpgradePrice;
        if (!GameManager.I.TrySpend(price))
        {
            AudioKit.I.Play("error");
            UIManager.I?.Toast("Không đủ cá để nâng cấp!", Pal.Danger);
            return false;
        }
        Level++;
        Invested += price;
        RefreshPips();
        scaleSmooth = 1.25f;
        Fx.Ring(transform.position, 0.9f, new Color(1f, 0.85f, 0.4f, 0.9f));
        Fx.Burst(transform.position + Vector3.up * 0.2f, Pal.Gold, 10, 0.35f);
        AudioKit.I.Play("upgrade");
        UIManager.I?.Toast(TypeName + " lên cấp " + Level + "!", Pal.Gold);
        return true;
    }

    public void Sell()
    {
        int refund = SellValue;
        GameManager.I.AddFish(refund);
        UIManager.I?.Float(transform.position + Vector3.up * 0.3f, "+" + refund, Pal.Gold, 28);
        Fx.Burst(transform.position + Vector3.up * 0.1f, Pal.Cream, 8, 0.32f);
        AudioKit.I.Play("sell");
        BuildManager.I?.ForgetTower(this);
        Destroy(gameObject);
    }
}
