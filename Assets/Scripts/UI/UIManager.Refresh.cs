using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    void Update()
    {
        var gm = GameManager.I;
        if (gm == null) return;
        float dt = Time.unscaledDeltaTime;

        if (hintTime > 0f)
        {
            hintTime -= dt;
            if (hintGroup != null)
            {
                hintGroup.alpha = Mathf.Clamp01(hintTime / 1.5f);
                if (hintTime <= 0f) hintGroup.gameObject.SetActive(false);
            }
        }

        if (toastTime > 0f)
        {
            toastTime -= dt;
            if (toastGroup != null) toastGroup.alpha = Mathf.Clamp01(toastTime / 0.6f);
        }
        else if (toastGroup != null && toastGroup.alpha != 0f)
        {
            toastGroup.alpha = 0f;
        }

        if (bannerTime > 0f)
        {
            bannerTime -= dt;
            if (bannerGroup != null)
                bannerGroup.alpha = bannerTime > 0.9f ? 1f : Mathf.Clamp01(bannerTime / 0.9f);
        }
        else if (bannerGroup != null && bannerGroup.alpha != 0f)
        {
            bannerGroup.alpha = 0f;
        }

        if (damageAlpha > 0f)
        {
            damageAlpha = Mathf.Max(0f, damageAlpha - dt * 1.4f);
            if (damageFlash != null)
                damageFlash.color = new Color(1f, 0.15f, 0.2f, damageAlpha * 0.55f);
        }

        if (muteIcon != null)
        {
            var want = AudioKit.I != null && AudioKit.I.Muted ? ArtUi.SoundOff : ArtUi.Sound;
            if (muteIcon.sprite != want) muteIcon.sprite = want;
        }
        if (pauseIcon != null)
        {
            var want = gm.Paused ? ArtUi.Play : ArtUi.Pause;
            if (pauseIcon.sprite != want) pauseIcon.sprite = want;
        }
        if (speedText != null)
        {
            string want = "x" + Mathf.RoundToInt(gm.GameSpeed);
            if (speedText.text != want) speedText.text = want;
        }
        bool showPause = gm.Paused && gm.State == GameState.Playing;
        if (pauseOverlay != null && pauseOverlay.gameObject.activeSelf != showPause)
            pauseOverlay.gameObject.SetActive(showPause);
        if (pauseHint != null && pauseHint.gameObject.activeSelf != showPause)
            pauseHint.gameObject.SetActive(showPause);
        if (pauseDim != null && pauseDim.gameObject.activeSelf != showPause)
            pauseDim.gameObject.SetActive(showPause);

        if (gm.State != GameState.Playing && Input.GetKeyDown(KeyCode.R))
        {
            gm.Restart();
            return;
        }

        Refresh();
    }

    static void SetText(Text t, string value)
    {
        if (t != null && t.text != value) t.text = value;
    }

    void Refresh()
    {
        var gm = GameManager.I;
        if (gm == null || gm.Waves == null) return;

        SetText(livesText, gm.Lives.ToString());
        SetText(fishText, gm.Fish.ToString());

        int shownWave = Mathf.Clamp(gm.Wave <= 0 ? gm.Waves.Upcoming : gm.Wave, 1, Waves.Total);
        SetText(waveTitle, "ĐỢT " + shownWave + "/" + Waves.Total);

        string sub;
        if (gm.State == GameState.Won) sub = "Thành mèo an toàn!";
        else if (gm.State == GameState.Lost) sub = "Thành đã thủng...";
        else if (gm.Waves.WaveActive) sub = "Còn " + Enemy.AliveCount + " chuột";
        else if (gm.Waves.IsPreparing) sub = "Chuẩn bị: " + Mathf.CeilToInt(gm.Waves.Countdown) + "s";
        else sub = "";
        SetText(waveSub, sub);

        bool prep = gm.State == GameState.Playing && gm.Waves.IsPreparing;
        if (waveButton != null) waveButton.interactable = prep;
        if (prep)
        {
            SetText(waveBtnLabel, "GỌI ĐỢT " + gm.Waves.Upcoming);
            SetText(waveBtnSub, "+" + Mathf.CeilToInt(gm.Waves.Countdown) * 2 + " cá");
            if (waveButtonImg != null) waveButtonImg.color = Pal.C("#3f7d5a");
        }
        else if (gm.Waves.WaveActive)
        {
            SetText(waveBtnLabel, "ĐANG CHIẾN ĐẤU");
            SetText(waveBtnSub, "Còn " + Enemy.AliveCount + " chuột");
            if (waveButtonImg != null) waveButtonImg.color = new Color(0.3f, 0.24f, 0.3f, 0.95f);
        }
        else
        {
            SetText(waveBtnLabel, gm.State == GameState.Won ? "ĐÃ THẮNG" : "KẾT THÚC");
            SetText(waveBtnSub, "");
            if (waveButtonImg != null) waveButtonImg.color = new Color(0.3f, 0.24f, 0.3f, 0.95f);
        }

        for (int i = 0; i < 4; i++)
        {
            if (cardImg[i] == null) continue;
            var def = Catalog.Towers[i];
            bool afford = gm.Fish >= def.Cost;
            bool selected = BuildManager.I != null && BuildManager.I.BuildKind == def.Kind;
            Color bg = selected ? Pal.UiCardSel : Pal.UiCard;
            if (!afford) bg = Color.Lerp(bg, new Color(0.12f, 0.1f, 0.14f, 1f), 0.55f);
            if (cardImg[i].color != bg) cardImg[i].color = bg;
            SetText(cardCost[i], def.Cost.ToString());
            Color costColor = afford ? Pal.Gold : Pal.Danger;
            if (cardCost[i].color != costColor) cardCost[i].color = costColor;
        }

        RefreshInfo();
    }

    public void RefreshImmediate()
    {
        if (canvasRect == null) return;
        Refresh();
    }

    void RefreshInfo()
    {
        if (infoPanel == null) return;
        if (infoTower == null)
        {
            if (infoPanel.activeSelf) infoPanel.SetActive(false);
            return;
        }
        if (!infoPanel.activeSelf) infoPanel.SetActive(true);

        var gm = GameManager.I;
        var def = infoTower.Def;
        var s = infoTower.Stats;
        bool max = infoTower.MaxLevel;
        var next = max ? null : def.Stats(infoTower.Level + 1);

        SetText(infoTitle, def.Name);
        SetText(infoLevel, "Cấp " + infoTower.Level);
        SetText(infoStats,
            "Sát thương: " + Num(s.Damage) + (max ? "" : "  →  " + Num(next.Damage)) + "\n" +
            "Tầm bắn: " + s.Range.ToString("0.0") + "\n" +
            "Tốc bắn: " + (1f / s.Cooldown).ToString("0.0") + " lần/s");

        if (s.Slow > 0f)
        {
            float slow = max ? s.Slow : next.Slow;
            SetText(infoSpecial, "Làm chậm chuột " + Mathf.RoundToInt(slow * 100f) + "%");
            infoSpecial.color = Pal.Sky;
        }
        else if (s.Splash > 0f)
        {
            SetText(infoSpecial, "Nổ lan bán kính " + s.Splash.ToString("0.0") + " ô");
            infoSpecial.color = Pal.Rose;
        }
        else if (def.Projectile == "sniper")
        {
            SetText(infoSpecial, "Bắn tỉa xuyên tầm xa");
            infoSpecial.color = Pal.Mint;
        }
        else
        {
            SetText(infoSpecial, def.Blurb);
            infoSpecial.color = Pal.Mint;
        }

        if (max)
        {
            SetText(upgradeLabel, "ĐÃ TỐI ĐA");
            if (upgradeBtn != null) upgradeBtn.interactable = false;
            if (upgradeImg != null) upgradeImg.color = new Color(0.3f, 0.26f, 0.3f, 0.9f);
        }
        else
        {
            int price = infoTower.UpgradePrice;
            SetText(upgradeLabel, "NÂNG CẤP  " + price);
            bool can = gm != null && gm.Fish >= price;
            if (upgradeBtn != null) upgradeBtn.interactable = can;
            if (upgradeImg != null)
                upgradeImg.color = can ? Pal.C("#c07f2e") : new Color(0.33f, 0.27f, 0.3f, 0.95f);
        }
        SetText(sellLabel, "BÁN +" + infoTower.SellValue);
    }

    static string Num(float v)
    {
        return v >= 10f ? Mathf.RoundToInt(v).ToString() : v.ToString("0.#");
    }

    public void ShowInfo(Tower tower)
    {
        infoTower = tower;
        if (infoPanel != null && tower != null) infoPanel.SetActive(true);
        RefreshInfo();
    }

    public void HideInfo()
    {
        infoTower = null;
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    void OnUpgrade()
    {
        if (infoTower == null) return;
        AudioKit.I.Play("click");
        if (infoTower.Upgrade())
        {
            RefreshInfo();
            RefreshImmediate();
        }
    }

    void OnSell()
    {
        if (infoTower == null) return;
        AudioKit.I.Play("click");
        var t = infoTower;
        HideInfo();
        t.Sell();
        RefreshImmediate();
    }

    public void ShowResult(bool won)
    {
        var gm = GameManager.I;
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        HideInfo();
        if (BuildManager.I != null) BuildManager.I.CancelBuild();

        if (resultTitle != null)
        {
            resultTitle.text = won ? "THÀNH MÈO THẮNG LỢI!" : "THÀNH ĐÃ THỦNG!";
            resultTitle.color = won ? Pal.Gold : Pal.Danger;
        }
        if (resultSub != null)
            resultSub.text = won ? "Bạn đã đẩy lùi cả 20 đợt chuột!" : "Lũ chuột đã tràn vào thành mèo...";
        if (resultStats != null && gm != null)
            resultStats.text = "Đợt đạt được: " + gm.Wave + "/" + Waves.Total + "\n" +
                               "Chuột đã hạ: " + gm.Kills + "\n" +
                               "Cá kiếm được: " + gm.FishEarned;
    }

    public void Toast(string message, Color color)
    {
        if (toastText == null) return;
        toastText.text = message;
        toastText.color = color;
        toastTime = 2.6f;
        if (toastGroup != null) toastGroup.alpha = 1f;
    }

    public void Banner(string title, string subtitle)
    {
        if (bannerTitle == null) return;
        bannerTitle.text = title;
        bannerSub.text = subtitle;
        bannerTime = 2.3f;
        if (bannerGroup != null) bannerGroup.alpha = 1f;
    }

    public void FlashDamage()
    {
        damageAlpha = 0.55f;
    }

    public void Float(Vector3 worldPos, string message, Color color, int size)
    {
        if (floatLayer == null || canvasRect == null) return;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 screen = cam.WorldToScreenPoint(worldPos);
        if (screen.z < 0f) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        var t = Txt(floatLayer, "Float", message, size, color, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), local,
            new Vector2(260f, 60f), true);
        StartCoroutine(FloatRoutine(t));
    }

    IEnumerator FloatRoutine(Text t)
    {
        var rt = t.rectTransform;
        Vector2 start = rt.anchoredPosition;
        Color baseColor = t.color;
        const float lifeMax = 1.1f;
        float life = lifeMax;
        while (life > 0f)
        {
            float dt = Time.unscaledDeltaTime;
            life -= dt;
            float k = 1f - Mathf.Clamp01(life / lifeMax);
            rt.anchoredPosition = start + new Vector2(Mathf.Sin(k * 7f) * 8f, 90f * k);
            Color c = baseColor;
            c.a = Mathf.Clamp01(life / 0.45f);
            t.color = c;
            rt.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.16f);
            yield return null;
        }
        Destroy(t.gameObject);
    }
}
