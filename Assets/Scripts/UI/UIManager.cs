using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIManager : MonoBehaviour
{
    public static UIManager I;

    Canvas canvas;
    RectTransform canvasRect;
    Font font;

    Text livesText, fishText, waveTitle, waveSub;
    Text speedText, hintText;
    Image pauseIcon, muteIcon;
    Button waveButton;
    Image waveButtonImg;
    Text waveBtnLabel, waveBtnSub;

    Image[] cardImg = new Image[4];
    Text[] cardName = new Text[4];
    Text[] cardCost = new Text[4];

    GameObject infoPanel;
    Text infoTitle, infoLevel, infoStats, infoSpecial, upgradeLabel, sellLabel;
    Image upgradeImg;
    Button upgradeBtn, sellBtn;
    Tower infoTower;

    Text bannerTitle, bannerSub;
    CanvasGroup bannerGroup;
    Image pauseDim;
    Text pauseOverlay, pauseHint;
    Text toastText;
    CanvasGroup toastGroup;
    CanvasGroup hintGroup;
    RectTransform floatLayer;
    Image damageFlash;

    GameObject resultPanel;
    Text resultTitle, resultSub, resultStats;

    float toastTime, bannerTime, hintTime, damageAlpha;
    int cachedFish = -1;
    int cachedLives = -1;
    int cachedWave = -1;
    TowerKind? cachedBuild;

    void Awake()
    {
        I = this;
        BuildCanvas();
        BuildHud();
        BuildShop();
        BuildInfoPanel();
        BuildWaveButton();
        BuildBannerAndToast();
        BuildResult();
        BuildFloatingLayer();
        RefreshImmediate();
    }

    // ---------------- dung cu ----------------

    RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Place(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    Image Img(Transform parent, string name, Sprite sprite, Color color, bool raycast = false, bool sliced = false)
    {
        var rt = NewRect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        if (sliced && sprite != null && sprite.border.sqrMagnitude > 0f) img.type = Image.Type.Sliced;
        return img;
    }

    Text Txt(Transform parent, string name, string text, int fontSize, Color color, TextAnchor align,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 rect, bool bold = false)
    {
        var rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.supportRichText = true;
        t.lineSpacing = 1.1f;
        var sh = rt.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
        sh.effectDistance = new Vector2(1.5f, -1.5f);
        Place(rt, aMin, aMax, pivot, pos, rect);
        return t;
    }

    Text TxtCenter(Transform parent, string name, string text, int fontSize, Color color, Vector2 pos, Vector2 rect, bool bold = false)
    {
        return Txt(parent, name, text, fontSize, color, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, rect, bold);
    }

    Button MakeButton(Transform parent, string name, Color bg, Vector2 aMin, Vector2 aMax, Vector2 pivot,
        Vector2 pos, Vector2 size, UnityAction onClick)
    {
        var img = Img(parent, name, ArtUi.Rounded, bg, true, true);
        var rt = img.rectTransform;
        Place(rt, aMin, aMax, pivot, pos, size);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        cb.selectedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);
        var gloss = Img(rt, "Gloss", ArtUi.RoundedOutline, new Color(1f, 1f, 1f, 0.12f));
        Place(gloss.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        gloss.rectTransform.offsetMin = new Vector2(2f, 2f);
        gloss.rectTransform.offsetMax = new Vector2(-2f, -2f);
        return btn;
    }

    void BuildCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasRect = go.GetComponent<RectTransform>();

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(transform, false);
        }

        font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Tahoma", "Liberation Sans" }, 32);
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        damageFlash = Img(go.transform, "DamageFlash", null, new Color(1f, 0.15f, 0.2f, 0f));
        Place(damageFlash.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    // ---------------- HUD tren cung ----------------

    void BuildHud()
    {
        var root = canvas.transform;

        // MUA (mang song)
        var livesPill = Img(root, "LivesPill", ArtUi.Rounded, Pal.UiPanel, true, true);
        Place(livesPill.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(24f, -18f), new Vector2(224f, 74f));
        var heart = Img(livesPill.transform, "Heart", ArtUi.Heart, Color.white);
        Place(heart.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(16f, 0f), new Vector2(46f, 46f));
        livesText = Txt(livesPill.transform, "Lives", "12", 38, Pal.UiText, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 0f),
            new Vector2(130f, 54f), true);

        // DOT SONG
        var wavePill = Img(root, "WavePill", ArtUi.Rounded, Pal.UiPanel, true, true);
        Place(wavePill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -14f), new Vector2(380f, 98f));
        waveTitle = Txt(wavePill.transform, "WaveTitle", "ĐỢT 0/20", 32, Pal.UiText, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f),
            new Vector2(350f, 46f), true);
        waveSub = Txt(wavePill.transform, "WaveSub", "Chuẩn bị...", 19, Pal.UiTextDim, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f),
            new Vector2(350f, 30f));

        // CA
        var fishPill = Img(root, "FishPill", ArtUi.Rounded, Pal.UiPanel, true, true);
        Place(fishPill.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-238f, -18f), new Vector2(196f, 74f));
        var fishIcon = Img(fishPill.transform, "FishIcon", ArtUi.Fish, Color.white);
        Place(fishIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(14f, 0f), new Vector2(48f, 48f));
        fishIcon.preserveAspect = true;
        fishText = Txt(fishPill.transform, "Fish", "240", 34, Pal.Gold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(68f, 0f),
            new Vector2(118f, 50f), true);

        // NUT DIEU KHIEN
        var muteBtn = MakeButton(root, "MuteBtn", new Color(0.22f, 0.17f, 0.25f, 0.92f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -18f),
            new Vector2(64f, 64f), () => AudioKit.I.ToggleMute());
        muteIcon = Img(muteBtn.transform, "Icon", ArtUi.Sound, Color.white);
        Place(muteIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(40f, 40f));

        var pauseBtn = MakeButton(root, "PauseBtn", new Color(0.22f, 0.17f, 0.25f, 0.92f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-96f, -18f),
            new Vector2(64f, 64f), () => GameManager.I.TogglePause());
        pauseIcon = Img(pauseBtn.transform, "Icon", ArtUi.Pause, Color.white);
        Place(pauseIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(36f, 36f));

        var speedBtn = MakeButton(root, "SpeedBtn", new Color(0.22f, 0.17f, 0.25f, 0.92f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-168f, -18f),
            new Vector2(64f, 64f), () => GameManager.I.ToggleSpeed());
        speedText = TxtCenter(speedBtn.transform, "SpeedText", "x1", 25, Pal.UiText, Vector2.zero,
            new Vector2(60f, 50f), true);

        // GOI Y
        var hintRoot = NewRect("HintRoot", root);
        Place(hintRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 238f), new Vector2(920f, 44f));
        hintGroup = hintRoot.gameObject.AddComponent<CanvasGroup>();
        hintGroup.blocksRaycasts = false;
        var hintBg = Img(hintRoot, "Bg", ArtUi.Rounded, new Color(0.09f, 0.07f, 0.11f, 0.6f), false, true);
        Place(hintBg.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        hintText = Txt(hintRoot, "Hint", "Chọn mèo ở dưới rồi click lên bãi cỏ để đặt. Phím 1-4 chọn nhanh, Space gọi đợt.",
            19, Pal.UiText, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 40f));
        hintTime = 14f;
    }

    // ---------------- cua hang ----------------

    void BuildShop()
    {
        var root = canvas.transform;
        var shop = Img(root, "Shop", ArtUi.Rounded, new Color(0.13f, 0.1f, 0.16f, 0.92f), true, true);
        Place(shop.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 16f), new Vector2(1400f, 176f));

        for (int i = 0; i < 4; i++)
        {
            var def = Catalog.Towers[i];
            var kind = def.Kind;
            var card = Img(shop.transform, "Card" + i, ArtUi.Rounded, Pal.UiCard, true, true);
            Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-548f + i * 262f, 2f), new Vector2(254f, 140f));
            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card;
            var cb = btn.colors;
            cb.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            cb.highlightedColor = Color.white;
            cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(() =>
            {
                AudioKit.I.Play("click");
                BuildManager.I.BeginBuild(kind);
            });

            var icon = Img(card.transform, "Cat", ArtChars.Cat(kind), Color.white);
            Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-86f, 2f), new Vector2(92f, 92f));
            icon.preserveAspect = true;

            cardName[i] = Txt(card.transform, "Name", def.Name, 22, Pal.UiText, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-34f, 30f), new Vector2(170f, 32f), true);

            var fishIcon = Img(card.transform, "CostIcon", ArtUi.Fish, Color.white);
            Place(fishIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-8f, -20f), new Vector2(28f, 28f));
            fishIcon.preserveAspect = true;

            cardCost[i] = Txt(card.transform, "Cost", def.Cost.ToString(), 21, Pal.Gold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(8f, -20f), new Vector2(100f, 30f), true);

            var badge = Img(card.transform, "Hotkey", ArtUi.Circle, new Color(0f, 0f, 0f, 0.35f));
            Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-10f, -10f), new Vector2(30f, 30f));
            TxtCenter(badge.transform, "Key", (i + 1).ToString(), 17, Pal.UiTextDim, Vector2.zero,
                new Vector2(28f, 28f), true);

            cardImg[i] = card;
        }

        // Nut goi dot nam luon trong thanh cua hang cho gon.
        waveButton = MakeButton(shop.transform, "WaveButton", Pal.C("#3f7d5a"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(512f, 2f), new Vector2(268f, 140f), OnWaveButton);
        waveButtonImg = waveButton.GetComponent<Image>();
        waveBtnLabel = Txt(waveButton.transform, "Label", "GỌI ĐỢT TIẾP", 22, Pal.UiText,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(244f, 34f), true);
        waveBtnSub = Txt(waveButton.transform, "Sub", "Đợt 1 • +36 cá", 16, new Color(0.85f, 0.95f, 0.85f, 1f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(244f, 26f));
        Txt(waveButton.transform, "Hint", "Space", 14, new Color(0.8f, 0.9f, 0.8f, 0.8f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -46f), new Vector2(244f, 22f));
    }

    // ---------------- bang thong tin meo ----------------

    void BuildInfoPanel()
    {
        var root = canvas.transform;
        var panel = Img(root, "InfoPanel", ArtUi.Rounded, Pal.UiPanel, true, true);
        Place(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(24f, 204f), new Vector2(362f, 248f));
        infoPanel = panel.gameObject;

        infoTitle = Txt(panel.transform, "Title", "Mèo Mướp", 26, Pal.UiText, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f),
            new Vector2(240f, 36f), true);
        infoLevel = Txt(panel.transform, "Level", "Cấp 1", 20, Pal.Gold, TextAnchor.MiddleRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -18f),
            new Vector2(90f, 30f), true);

        infoStats = Txt(panel.transform, "Stats", "", 18, Pal.UiText, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -58f),
            new Vector2(326f, 96f));
        infoSpecial = Txt(panel.transform, "Special", "", 17, Pal.Mint, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -156f),
            new Vector2(326f, 28f));

        upgradeBtn = MakeButton(panel.transform, "Upgrade", Pal.C("#c07f2e"),
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 14f),
            new Vector2(216f, 56f), OnUpgrade);
        upgradeImg = upgradeBtn.GetComponent<Image>();
        upgradeLabel = Txt(upgradeBtn.transform, "Label", "NÂNG CẤP  50", 19, Pal.UiText,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(206f, 40f), true);

        sellBtn = MakeButton(panel.transform, "Sell", new Color(0.34f, 0.26f, 0.31f, 1f),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 14f),
            new Vector2(116f, 56f), OnSell);
        sellLabel = Txt(sellBtn.transform, "Label", "BÁN +36", 18, Pal.UiText, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(110f, 40f), true);

        infoPanel.SetActive(false);
    }

    void BuildWaveButton()
    {
    }

    void BuildBannerAndToast()
    {
        var root = canvas.transform;

        var banner = NewRect("Banner", root);
        Place(banner, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 150f), new Vector2(760f, 150f));
        bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;
        var bannerBg = Img(banner, "Bg", ArtUi.Rounded, new Color(0.09f, 0.07f, 0.11f, 0.65f), false, true);
        Place(bannerBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 145f));
        bannerTitle = TxtCenter(banner, "Title", "ĐỢT 1", 64, Pal.Gold, new Vector2(0f, 18f),
            new Vector2(760f, 82f), true);
        bannerSub = TxtCenter(banner, "Sub", "", 24, Pal.UiText, new Vector2(0f, -34f),
            new Vector2(760f, 40f), true);

        var toast = NewRect("Toast", root);
        Place(toast, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -126f), new Vector2(900f, 46f));
        toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
        var toastBg = Img(toast, "Bg", ArtUi.Rounded, new Color(0.09f, 0.07f, 0.11f, 0.65f), false, true);
        Place(toastBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 44f));
        toastText = TxtCenter(toast, "Text", "", 22, Pal.UiText, Vector2.zero,
            new Vector2(900f, 46f), true);

        pauseDim = Img(root, "PauseDim", null, new Color(0.04f, 0.03f, 0.06f, 0.55f), false);
        Place(pauseDim.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        pauseDim.gameObject.SetActive(false);

        pauseOverlay = TxtCenter(root, "PauseOverlay", "TẠM DỪNG", 56, Pal.Cream, new Vector2(0f, 20f),
            new Vector2(900f, 90f), true);
        pauseOverlay.gameObject.SetActive(false);

        pauseHint = TxtCenter(root, "PauseHint", "Nhấn P hoặc nút tạm dừng để tiếp tục", 22, Pal.UiTextDim,
            new Vector2(0f, -48f), new Vector2(900f, 40f));
        pauseHint.gameObject.SetActive(false);
    }

    void BuildResult()
    {
        var root = canvas.transform;
        resultPanel = NewRect("Result", root).gameObject;
        var rt = (RectTransform)resultPanel.transform;
        Place(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var dim = Img(resultPanel.transform, "Dim", null, new Color(0.04f, 0.03f, 0.06f, 0.78f), true);
        Place(dim.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var panel = Img(resultPanel.transform, "Panel", ArtUi.Rounded, new Color(0.16f, 0.12f, 0.19f, 0.98f), true, true);
        Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(640f, 440f));

        resultTitle = TxtCenter(panel.transform, "Title", "THẮNG LỚN!", 54, Pal.Gold, new Vector2(0f, 150f),
            new Vector2(600f, 70f), true);
        resultSub = TxtCenter(panel.transform, "Sub", "", 22, Pal.UiTextDim, new Vector2(0f, 94f),
            new Vector2(600f, 34f));
        resultStats = TxtCenter(panel.transform, "Stats", "", 23, Pal.UiText, new Vector2(0f, 18f),
            new Vector2(560f, 120f));
        resultStats.lineSpacing = 1.35f;

        var restart = MakeButton(panel.transform, "Restart", Pal.C("#3f7d5a"), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(300f, 72f),
            () => GameManager.I.Restart());
        TxtCenter(restart.transform, "Label", "CHƠI LẠI", 26, Pal.UiText, Vector2.zero,
            new Vector2(290f, 50f), true);

        resultPanel.SetActive(false);
    }

    void BuildFloatingLayer()
    {
        floatLayer = NewRect("Floating", canvas.transform);
        Place(floatLayer, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        floatLayer.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void OnWaveButton()
    {
        AudioKit.I.Play("click");
        if (GameManager.I.Waves.IsPreparing) GameManager.I.Waves.StartWave(true);
    }
}
