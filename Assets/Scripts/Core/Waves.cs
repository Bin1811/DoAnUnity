using System.Collections.Generic;

// 20 dot song, so luong va nhip do tung dot.
public static class Waves
{
    public const int Total = 20;

    public static float HpMultiplier(int wave)
    {
        return 1f + 0.14f * (wave - 1);
    }

    public static List<SpawnGroup> Get(int wave)
    {
        var g = new List<SpawnGroup>();
        switch (wave)
        {
            case 1: g.Add(new SpawnGroup(EnemyKind.Mouse, 6, 1.0f)); break;
            case 2: g.Add(new SpawnGroup(EnemyKind.Mouse, 9, 0.85f)); break;
            case 3:
                g.Add(new SpawnGroup(EnemyKind.Mouse, 8, 0.8f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 3, 0.7f, 0.5f));
                break;
            case 4:
                g.Add(new SpawnGroup(EnemyKind.Rat, 2, 1.6f));
                g.Add(new SpawnGroup(EnemyKind.Mouse, 8, 0.7f, 0.5f));
                break;
            case 5:
                g.Add(new SpawnGroup(EnemyKind.Fast, 9, 0.55f));
                g.Add(new SpawnGroup(EnemyKind.Mouse, 6, 0.8f, 1f));
                break;
            case 6:
                g.Add(new SpawnGroup(EnemyKind.Rat, 5, 1.2f));
                g.Add(new SpawnGroup(EnemyKind.Mouse, 10, 0.6f, 0.5f));
                break;
            case 7:
                g.Add(new SpawnGroup(EnemyKind.Rat, 7, 1.1f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 8, 0.5f, 0.8f));
                break;
            case 8:
                g.Add(new SpawnGroup(EnemyKind.Rat, 9, 1.0f));
                g.Add(new SpawnGroup(EnemyKind.Mouse, 12, 0.5f, 0.5f));
                break;
            case 9:
                g.Add(new SpawnGroup(EnemyKind.Rat, 10, 0.9f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 10, 0.45f, 0.8f));
                break;
            case 10:
                g.Add(new SpawnGroup(EnemyKind.Boss, 1, 1f));
                g.Add(new SpawnGroup(EnemyKind.Rat, 6, 0.9f, 1.5f));
                break;
            case 11:
                g.Add(new SpawnGroup(EnemyKind.Rat, 12, 0.8f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 10, 0.4f, 0.6f));
                break;
            case 12:
                g.Add(new SpawnGroup(EnemyKind.Mouse, 20, 0.35f));
                g.Add(new SpawnGroup(EnemyKind.Rat, 10, 0.8f, 1f));
                break;
            case 13:
                g.Add(new SpawnGroup(EnemyKind.Rat, 14, 0.7f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 12, 0.35f, 0.6f));
                break;
            case 14:
                g.Add(new SpawnGroup(EnemyKind.Rat, 16, 0.65f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 14, 0.3f, 0.8f));
                break;
            case 15:
                g.Add(new SpawnGroup(EnemyKind.Boss, 2, 5f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 14, 0.35f, 1.5f));
                break;
            case 16:
                g.Add(new SpawnGroup(EnemyKind.Rat, 18, 0.6f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 16, 0.3f, 0.8f));
                break;
            case 17:
                g.Add(new SpawnGroup(EnemyKind.Rat, 20, 0.55f));
                g.Add(new SpawnGroup(EnemyKind.Mouse, 20, 0.3f, 0.8f));
                break;
            case 18:
                g.Add(new SpawnGroup(EnemyKind.Rat, 22, 0.5f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 18, 0.28f, 0.8f));
                break;
            case 19:
                g.Add(new SpawnGroup(EnemyKind.Rat, 24, 0.45f));
                g.Add(new SpawnGroup(EnemyKind.Boss, 1, 1f, 2f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 16, 0.3f, 1.2f));
                break;
            case 20:
                g.Add(new SpawnGroup(EnemyKind.Boss, 2, 6f));
                g.Add(new SpawnGroup(EnemyKind.Rat, 20, 0.5f, 2f));
                g.Add(new SpawnGroup(EnemyKind.Fast, 20, 0.25f, 2f));
                break;
            default:
                g.Add(new SpawnGroup(EnemyKind.Mouse, 10, 0.5f));
                break;
        }
        return g;
    }
}
