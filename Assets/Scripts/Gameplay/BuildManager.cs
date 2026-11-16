using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager I;

    public TowerKind? BuildKind { get; private set; }
    public Tower Selected { get; private set; }

    readonly Dictionary<Vector2Int, Tower> towers = new Dictionary<Vector2Int, Tower>();
    Camera cam;
    SpriteRenderer rangeRing;
    SpriteRenderer cellHighlight;
    SpriteRenderer ghost;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        cam = Camera.main;
        rangeRing = MakeSprite("RangeRing", ArtUi.RangeRing, Sorting.Fx - 30);
        cellHighlight = MakeSprite("CellHighlight", ArtUi.Cell, Sorting.Fx - 20);
        ghost = MakeSprite("GhostCat", ArtChars.Cat(TowerKind.Tabby), Sorting.Fx - 10);
        HidePreviews();
    }

    SpriteRenderer MakeSprite(string name, Sprite sprite, int order)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.enabled = false;
        return sr;
    }

    public Tower GetTower(Vector2Int cell)
    {
        towers.TryGetValue(cell, out var t);
        return t;
    }

    public void BeginBuild(TowerKind kind)
    {
        if (GameManager.I.State != GameState.Playing) return;
        if (BuildKind == kind)
        {
            CancelBuild();
            return;
        }
        var def = Catalog.Get(kind);
        if (GameManager.I.Fish < def.Cost)
        {
            AudioKit.I.Play("error");
            UIManager.I?.Toast("Không đủ cá! Cần " + def.Cost + " cá.", Pal.Danger);
            return;
        }
        BuildKind = kind;
        Deselect();
        ghost.sprite = ArtChars.Cat(kind);
        ghost.enabled = true;
        AudioKit.I.Play("select");
        UIManager.I?.RefreshImmediate();
    }

    public void CancelBuild()
    {
        BuildKind = null;
        HidePreviews();
        UIManager.I?.RefreshImmediate();
    }

    public void SelectTower(Tower tower)
    {
        Selected = tower;
        BuildKind = null;
        if (tower != null)
        {
            AudioKit.I.Play("select", 0.8f);
            UIManager.I?.ShowInfo(tower);
        }
        else
        {
            UIManager.I?.HideInfo();
        }
        UIManager.I?.RefreshImmediate();
    }

    public void Deselect()
    {
        Selected = null;
        UIManager.I?.HideInfo();
    }

    public void ForgetTower(Tower tower)
    {
        if (tower == null) return;
        if (towers.TryGetValue(tower.Cell, out var t) && t == tower) towers.Remove(tower.Cell);
        if (Selected == tower)
        {
            Selected = null;
            UIManager.I?.HideInfo();
        }
    }

    void HidePreviews()
    {
        if (rangeRing != null) rangeRing.enabled = false;
        if (cellHighlight != null) cellHighlight.enabled = false;
        if (ghost != null) ghost.enabled = false;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        var gm = GameManager.I;
        if (gm == null || gm.State != GameState.Playing)
        {
            HidePreviews();
            return;
        }

        bool acceptInput = !DevCapture.InputBlocked;
        if (acceptInput) HandleKeys();

        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = MapLayout.WorldToCell(mouseWorld);
        bool inside = MapLayout.InBounds(cell);

        if (BuildKind.HasValue)
        {
            UpdateBuildPreview(cell, inside);
            if (acceptInput && !overUi && Input.GetMouseButtonDown(0))
            {
                var existing = inside ? GetTower(cell) : null;
                if (existing != null)
                {
                    CancelBuild();
                    SelectTower(existing);
                }
                else
                {
                    TryPlace(cell, inside);
                }
            }
            if (acceptInput && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                CancelBuild();
            }
        }
        else
        {
            if (Selected != null)
            {
                rangeRing.enabled = true;
                rangeRing.transform.position = new Vector3(Selected.transform.position.x,
                    Selected.transform.position.y, 0f);
                rangeRing.transform.localScale = Vector3.one * Selected.Range;
                rangeRing.color = new Color(1f, 1f, 1f, 0.45f);
                cellHighlight.enabled = false;
                ghost.enabled = false;
            }
            else
            {
                HidePreviews();
            }

            if (acceptInput && !overUi && Input.GetMouseButtonDown(0))
            {
                var existing = inside ? GetTower(cell) : null;
                if (existing != null) SelectTower(existing);
                else
                {
                    Deselect();
                    UIManager.I?.RefreshImmediate();
                }
            }
            if (acceptInput && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                Deselect();
                UIManager.I?.RefreshImmediate();
            }
        }
    }

    void UpdateBuildPreview(Vector2Int cell, bool inside)
    {
        if (!inside)
        {
            HidePreviews();
            return;
        }

        bool free = !MapLayout.IsPath(cell) && GetTower(cell) == null;
        var center = new Vector3(MapLayout.CellCenter(cell).x, MapLayout.CellCenter(cell).y, 0f);
        var def = Catalog.Get(BuildKind.Value);

        rangeRing.enabled = true;
        rangeRing.transform.position = center;
        rangeRing.transform.localScale = Vector3.one * def.Stats(1).Range;
        rangeRing.color = free ? new Color(0.55f, 1f, 0.6f, 0.5f) : new Color(1f, 0.45f, 0.45f, 0.5f);

        cellHighlight.enabled = true;
        cellHighlight.transform.position = center;
        cellHighlight.color = free ? new Color(0.6f, 1f, 0.65f, 0.85f) : new Color(1f, 0.45f, 0.45f, 0.85f);

        ghost.enabled = true;
        ghost.transform.position = center + new Vector3(0f, 0.05f, 0f);
        ghost.transform.localScale = Vector3.one;
        ghost.color = free ? new Color(1f, 1f, 1f, 0.6f) : new Color(1f, 0.5f, 0.5f, 0.4f);
        ghost.sortingOrder = Sorting.Order(center.y) + 6;
    }

    void TryPlace(Vector2Int cell, bool inside)
    {
        if (!inside)
        {
            AudioKit.I.Play("error");
            UIManager.I?.Toast("Hãy đặt mèo lên bãi cỏ trong bản đồ!", Pal.Danger);
            return;
        }
        if (MapLayout.IsPath(cell))
        {
            AudioKit.I.Play("error");
            UIManager.I?.Toast("Không được chặn đường của chuột!", Pal.Danger);
            return;
        }
        if (GetTower(cell) != null)
        {
            AudioKit.I.Play("error");
            return;
        }

        var def = Catalog.Get(BuildKind.Value);
        if (!GameManager.I.TrySpend(def.Cost))
        {
            AudioKit.I.Play("error");
            UIManager.I?.Toast("Không đủ cá! Cần " + def.Cost + " cá.", Pal.Danger);
            return;
        }

        var tower = CreateTower(BuildKind.Value, cell);
        towers[cell] = tower;

        AudioKit.I.Play("place");
        Fx.Ring(MapLayout.CellCenter(cell), 0.85f, new Color(0.7f, 1f, 0.75f, 0.85f));
        Fx.Burst(MapLayout.CellCenter(cell) + Vector2.up * 0.2f, Pal.Cream, 8, 0.3f);

        if (GameManager.I.Fish < def.Cost)
        {
            CancelBuild();
            UIManager.I?.Toast("Hết cá rồi, hết đặt mèo!", Pal.UiTextDim);
        }
        UIManager.I?.RefreshImmediate();
    }

    // Dat meo khong can chuot (dung cho che do demo / test).
    public Tower AutoPlace(TowerKind kind, Vector2Int cell)
    {
        if (!MapLayout.InBounds(cell) || MapLayout.IsPath(cell) || GetTower(cell) != null) return null;
        if (!GameManager.I.TrySpend(Catalog.Get(kind).Cost)) return null;
        var tower = CreateTower(kind, cell);
        towers[cell] = tower;
        UIManager.I?.RefreshImmediate();
        return tower;
    }

    Tower CreateTower(TowerKind kind, Vector2Int cell)
    {
        var go = new GameObject("Tower_" + Catalog.Get(kind).Name);
        go.transform.SetParent(GameManager.I.TowersRoot, false);
        var tower = go.AddComponent<Tower>();
        tower.Init(kind, cell);
        return tower;
    }

    void HandleKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) BeginBuild(TowerKind.Tabby);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) BeginBuild(TowerKind.Siamese);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) BeginBuild(TowerKind.Persian);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) BeginBuild(TowerKind.British);
        else if (Input.GetKeyDown(KeyCode.Space)) GameManager.I.Waves.StartWave(true);
        else if (Input.GetKeyDown(KeyCode.P)) GameManager.I.TogglePause();
        else if (Input.GetKeyDown(KeyCode.M)) AudioKit.I.ToggleMute();
    }
}
