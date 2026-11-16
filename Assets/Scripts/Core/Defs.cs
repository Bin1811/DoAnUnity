using UnityEngine;

public enum GameState { Playing, Won, Lost }
public enum TowerKind { Tabby = 0, Siamese = 1, Persian = 2, British = 3 }
public enum EnemyKind { Mouse = 0, Fast = 1, Rat = 2, Boss = 3 }

public class TowerLevel
{
    public float Damage;
    public float Range;
    public float Cooldown;
    public float Splash;
    public float Slow;
    public float SlowTime;

    public TowerLevel(float damage, float range, float cooldown, float splash = 0f, float slow = 0f, float slowTime = 0f)
    {
        Damage = damage;
        Range = range;
        Cooldown = cooldown;
        Splash = splash;
        Slow = slow;
        SlowTime = slowTime;
    }
}

public class TowerDef
{
    public TowerKind Kind;
    public string Name;
    public string Blurb;
    public int Cost;
    public int[] UpgradeCost;   // [0] = len cap 2, [1] = len cap 3
    public TowerLevel[] Levels; // 3 cap
    public Color Coat;
    public Color CoatDark;
    public Color Accent;
    public Color Eye;
    public string Projectile;

    public TowerLevel Stats(int level) => Levels[Mathf.Clamp(level - 1, 0, Levels.Length - 1)];
}

public class EnemyDef
{
    public EnemyKind Kind;
    public string Name;
    public float Hp;
    public float Speed;
    public float Reward;
    public int Damage;
    public float SlowResist;
    public float Scale;
    public string Taunt;

    public EnemyDef(EnemyKind kind, string name, float hp, float speed, float reward, int damage,
        float slowResist, float scale, string taunt)
    {
        Kind = kind;
        Name = name;
        Hp = hp;
        Speed = speed;
        Reward = reward;
        Damage = damage;
        SlowResist = slowResist;
        Scale = scale;
        Taunt = taunt;
    }
}

public struct SpawnGroup
{
    public EnemyKind Kind;
    public int Count;
    public float Interval;
    public float Delay;

    public SpawnGroup(EnemyKind kind, int count, float interval, float delay = 0f)
    {
        Kind = kind;
        Count = count;
        Interval = interval;
        Delay = delay;
    }
}

public static class Catalog
{
    public static readonly TowerDef[] Towers =
    {
        new TowerDef
        {
            Kind = TowerKind.Tabby,
            Name = "Mèo Mướp",
            Blurb = "Ném len liên tục, sát thương ổn định.",
            Cost = 60,
            UpgradeCost = new[] { 50, 80 },
            Projectile = "yarn",
            Coat = Pal.C("#f2a33c"), CoatDark = Pal.C("#c9752a"), Accent = Pal.C("#ffdf9e"), Eye = Pal.C("#71d06a"),
            Levels = new[]
            {
                new TowerLevel(9f, 2.7f, 0.85f),
                new TowerLevel(15f, 2.9f, 0.75f),
                new TowerLevel(24f, 3.2f, 0.65f),
            }
        },
        new TowerDef
        {
            Kind = TowerKind.Siamese,
            Name = "Mèo Xiêm",
            Blurb = "Đạn băng làm chậm cả bầy chuột.",
            Cost = 90,
            UpgradeCost = new[] { 75, 110 },
            Projectile = "ice",
            Coat = Pal.C("#f6ead6"), CoatDark = Pal.C("#8a6a52"), Accent = Pal.C("#8fd3ff"), Eye = Pal.C("#4fa8ff"),
            Levels = new[]
            {
                new TowerLevel(6f, 2.9f, 1.0f, 0f, 0.35f, 1.6f),
                new TowerLevel(10f, 3.1f, 0.9f, 0f, 0.45f, 1.9f),
                new TowerLevel(15f, 3.3f, 0.8f, 0f, 0.55f, 2.2f),
            }
        },
        new TowerDef
        {
            Kind = TowerKind.Persian,
            Name = "Mèo Ba Tư",
            Blurb = "Bong bóng nổ, sát thương diện rộng.",
            Cost = 120,
            UpgradeCost = new[] { 100, 150 },
            Projectile = "bubble",
            Coat = Pal.C("#f4f2ec"), CoatDark = Pal.C("#b9aec4"), Accent = Pal.C("#ffa8d2"), Eye = Pal.C("#ff9d6b"),
            Levels = new[]
            {
                new TowerLevel(18f, 3.0f, 1.5f, 1.05f),
                new TowerLevel(28f, 3.2f, 1.35f, 1.2f),
                new TowerLevel(42f, 3.4f, 1.2f, 1.35f),
            }
        },
        new TowerDef
        {
            Kind = TowerKind.British,
            Name = "Mèo Anh",
            Blurb = "Bắn tỉa tầm xa, một phát đau điếng.",
            Cost = 150,
            UpgradeCost = new[] { 130, 190 },
            Projectile = "sniper",
            Coat = Pal.C("#8c9bb5"), CoatDark = Pal.C("#5c6880"), Accent = Pal.C("#c9d6ea"), Eye = Pal.C("#ffd84d"),
            Levels = new[]
            {
                new TowerLevel(55f, 6.5f, 2.2f),
                new TowerLevel(80f, 7.0f, 2.0f),
                new TowerLevel(120f, 7.6f, 1.8f),
            }
        },
    };

    public static TowerDef Get(TowerKind kind) => Towers[(int)kind];

    public static readonly EnemyDef Mouse = new EnemyDef(EnemyKind.Mouse, "Chuột Nhắt", 32f, 1.7f, 6f, 1, 0f, 0.62f, "chít chít!");
    public static readonly EnemyDef Fast = new EnemyDef(EnemyKind.Fast, "Chuột Lướt", 24f, 3.3f, 7f, 1, 0f, 0.55f, "vù vù!");
    public static readonly EnemyDef Rat = new EnemyDef(EnemyKind.Rat, "Chuột Cống", 120f, 1.25f, 13f, 1, 0.3f, 0.82f, "khoè khoè!");
    public static readonly EnemyDef Boss = new EnemyDef(EnemyKind.Boss, "Chuột Đầu Đàn", 900f, 0.95f, 90f, 3, 0.55f, 1.3f, "CHOANG!");

    public static EnemyDef Get(EnemyKind kind)
    {
        switch (kind)
        {
            case EnemyKind.Fast: return Fast;
            case EnemyKind.Rat: return Rat;
            case EnemyKind.Boss: return Boss;
            default: return Mouse;
        }
    }
}
