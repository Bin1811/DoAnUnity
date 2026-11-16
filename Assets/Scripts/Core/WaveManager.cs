using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public int CurrentWave;
    public float Countdown;
    public bool WaveActive;
    public bool Started;

    int nextWave = 1;

    public bool IsPreparing => Started && !WaveActive && GameManager.I.State == GameState.Playing
                              && nextWave <= Waves.Total;

    public int Upcoming => Mathf.Clamp(nextWave, 1, Waves.Total);

    public float EarlyBonus => Mathf.CeilToInt(Mathf.Max(0f, Countdown)) * 2;

    public void Begin()
    {
        StopAllCoroutines();
        nextWave = 1;
        CurrentWave = 0;
        WaveActive = false;
        Started = true;
        Countdown = 18f;
        GameManager.I.Wave = 0;
    }

    void Update()
    {
        if (!Started) return;
        if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;
        if (WaveActive || nextWave > Waves.Total) return;

        Countdown -= Time.deltaTime;
        if (Countdown <= 0f)
        {
            StartWave(false);
        }
    }

    public void StartWave(bool early)
    {
        if (!Started || WaveActive || GameManager.I.State != GameState.Playing) return;
        if (nextWave > Waves.Total) return;

        if (early && Countdown > 0.5f)
        {
            int bonus = Mathf.CeilToInt(Countdown) * 2;
            GameManager.I.AddFish(bonus);
            UIManager.I?.Toast("Gọi sớm! +" + bonus + " cá", Pal.Mint);
        }

        CurrentWave = nextWave;
        nextWave++;
        GameManager.I.Wave = CurrentWave;
        WaveActive = true;
        Countdown = 0f;
        AudioKit.I.Play("wave", 0.9f);
        UIManager.I?.Banner("ĐỢT " + CurrentWave, WaveSubtitle(CurrentWave));
        StartCoroutine(RunWave(CurrentWave));
    }

    static string WaveSubtitle(int wave)
    {
        switch (wave)
        {
            case 10: return "Chuột Đầu Đàn xuất hiện!";
            case 15: return "Hai tên đầu đàn kéo tới!";
            case 19: return "Đàn chuột điên cuồng!";
            case 20: return "Trận chiến cuối cùng!";
            default: return wave % 5 == 0 ? "Chuột tràn vào thành!" : "Giữ vững thành mèo!";
        }
    }

    IEnumerator RunWave(int wave)
    {
        yield return new WaitForSeconds(0.7f);
        var groups = Waves.Get(wave);

        foreach (var g in groups)
        {
            if (g.Delay > 0f) yield return new WaitForSeconds(g.Delay);
            for (int i = 0; i < g.Count; i++)
            {
                if (GameManager.I.State != GameState.Playing) yield break;
                Spawn(g.Kind, wave);
                yield return new WaitForSeconds(g.Interval);
            }
        }

        while (Enemy.AliveCount > 0)
        {
            if (GameManager.I.State != GameState.Playing) yield break;
            yield return null;
        }

        WaveActive = false;
        GameManager.I.OnWaveCleared(wave);

        if (wave >= Waves.Total)
        {
            Started = false;
            GameManager.I.Win();
        }
        else
        {
            Countdown = 12f;
        }
    }

    void Spawn(EnemyKind kind, int wave)
    {
        var def = Catalog.Get(kind);
        var go = new GameObject("Enemy_" + def.Name);
        go.transform.SetParent(GameManager.I.EnemiesRoot, false);
        var enemy = go.AddComponent<Enemy>();
        Vector2 jitter = Random.insideUnitCircle * 0.12f;
        enemy.Init(def, wave, new Vector3(MapLayout.SpawnPoint.x + jitter.x, MapLayout.SpawnPoint.y + jitter.y, 0f));
    }
}
