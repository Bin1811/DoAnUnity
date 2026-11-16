using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Che do chup anh / demo de kiem tra giao dien khi build (chi bon lenh dong lenh).
public class DevCapture : MonoBehaviour
{
    // Che do dev: khoa chuot/phim de anh chup khong bi nguoi dung bam lung tung.
    public static bool InputBlocked;

    string shotDir;
    float[] shotTimes;
    string shotFile;
    float shotDelay = 2.5f;
    bool demo;
    int demoLevel = 2;
    string demoEnd;
    float demoEndAt = 8f;
    string demoSelect;

    void Awake()
    {
        var args = System.Environment.GetCommandLineArgs();
        LogLine("ARGS: " + string.Join(" ", args));
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-shotdir":
                    if (i + 1 < args.Length) shotDir = args[++i];
                    break;
                case "-shots":
                    if (i + 1 < args.Length)
                    {
                        var parts = args[++i].Split(';');
                        var list = new List<float>();
                        foreach (var p in parts)
                            if (float.TryParse(p, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out float v))
                                list.Add(v);
                        shotTimes = list.ToArray();
                    }
                    break;
                case "-shot":
                    if (i + 1 < args.Length) shotFile = args[++i];
                    break;
                case "-shotdelay":
                    if (i + 1 < args.Length) float.TryParse(args[++i], out shotDelay);
                    break;
                case "-demo":
                    demo = true;
                    break;
                case "-demolevel":
                    if (i + 1 < args.Length) int.TryParse(args[++i], out demoLevel);
                    break;
                case "-demoend":
                    if (i + 1 < args.Length) demoEnd = args[++i];
                    break;
                case "-demoendat":
                    if (i + 1 < args.Length) float.TryParse(args[++i], out demoEndAt);
                    break;
                case "-demoselect":
                    if (i + 1 < args.Length) demoSelect = args[++i];
                    break;
            }
        }

        InputBlocked = demo || !string.IsNullOrEmpty(shotDir) || !string.IsNullOrEmpty(shotFile);
        if (InputBlocked && EventSystem.current != null) EventSystem.current.enabled = false;
        LogLine("shots=" + (shotTimes == null ? "null" : shotTimes.Length.ToString()) +
                " dir=" + shotDir + " demo=" + demo);

        if (demo) StartCoroutine(DemoRoutine());
        if (!string.IsNullOrEmpty(shotDir) && shotTimes != null && shotTimes.Length > 0)
            StartCoroutine(Sequence());
        else if (!string.IsNullOrEmpty(shotFile))
            StartCoroutine(SingleShot());
    }

    void LogLine(string msg)
    {
        try
        {
            string dir = shotDir;
            if (string.IsNullOrEmpty(dir)) return;
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.AppendAllText(System.IO.Path.Combine(dir, "dev.log"),
                System.DateTime.Now.ToString("HH:mm:ss.fff") + "  " + msg + "\n");
        }
        catch { }
    }

    IEnumerator SingleShot()
    {
        yield return new WaitForSeconds(shotDelay);
        ScreenCapture.CaptureScreenshot(shotFile);
        yield return new WaitForSeconds(1.2f);
        Application.Quit();
    }

    IEnumerator Sequence()
    {
        float last = 0f;
        for (int i = 0; i < shotTimes.Length; i++)
        {
            float wait = Mathf.Max(0f, shotTimes[i] - last);
            if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
            last = shotTimes[i];
            string path = System.IO.Path.Combine(shotDir, "shot_" + i + ".png");
            LogLine("capture " + i + " t=" + Time.time.ToString("0.00") + " path=" + path);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(1.2f);
            LogLine("captured " + i + " exists=" + System.IO.File.Exists(path));
        }
        LogLine("sequence done, quitting");
        Application.Quit();
    }

    IEnumerator DemoRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        if (GameManager.I == null) yield break;
        GameManager.I.AddFish(6000);

        var setup = new (TowerKind kind, int col, int row, int level)[]
        {
            (TowerKind.Tabby, 1, 6, 3),
            (TowerKind.Tabby, 4, 6, 2),
            (TowerKind.Siamese, 5, 3, 2),
            (TowerKind.Persian, 8, 3, 2),
            (TowerKind.Tabby, 7, 1, 2),
            (TowerKind.Siamese, 11, 3, 1),
            (TowerKind.British, 14, 4, 2),
            (TowerKind.British, 3, 3, 1),
        };

        if (demoLevel > 0)
        {
            int max = demoLevel == 1 ? 3 : setup.Length;
            for (int i = 0; i < max; i++)
            {
                var s = setup[i];
                var tower = BuildManager.I.AutoPlace(s.kind, new Vector2Int(s.col, s.row));
                if (tower == null) continue;
                if (demoLevel == 2)
                {
                    for (int l = 1; l < s.level; l++) tower.Upgrade();
                }
            }
        }
        yield return new WaitForSecondsRealtime(0.4f);

        if (!string.IsNullOrEmpty(demoSelect))
        {
            var parts = demoSelect.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int sc) && int.TryParse(parts[1], out int sr))
            {
                var tower = BuildManager.I.GetTower(new Vector2Int(sc, sr));
                if (tower != null) BuildManager.I.SelectTower(tower);
            }
        }

        GameManager.I.Waves.StartWave(true);

        if (!string.IsNullOrEmpty(demoEnd))
        {
            float elapsed = 0f;
            while (elapsed < demoEndAt)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (demoEnd == "lose") GameManager.I.Lose();
            else if (demoEnd == "win") GameManager.I.Win();
        }
    }
}
