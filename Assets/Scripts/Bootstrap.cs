using UnityEngine;

// Tao toan bo the gioi va cac he thong khi scene vua chay.
public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        QualitySettings.antiAliasing = 2;

        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Pal.Forest;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        if (cam.GetComponent<CameraFit>() == null) cam.gameObject.AddComponent<CameraFit>();
        if (cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();
        if (FindFirstObjectByType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();

        if (GameManager.I == null) gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<AudioKit>();
        gameObject.AddComponent<BuildManager>();
        gameObject.AddComponent<UIManager>();
        gameObject.AddComponent<DevCapture>();

        WorldBuilder.Build();
    }

    void Start()
    {
        GameManager.I.NewGame();
    }
}

public static class WorldBuilder
{
    public static void Build()
    {
        var bg = new GameObject("MapBackground");
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = ArtWorld.Background;
        bgSr.sortingOrder = -1000;
        bg.transform.position = Vector3.zero;

        var castle = new GameObject("CatCastle");
        var castleSr = castle.AddComponent<SpriteRenderer>();
        castleSr.sprite = ArtWorld.Castle;
        castleSr.sortingOrder = Sorting.Order(1.4f);
        castle.transform.position = new Vector3(8.62f, 1.55f, 0f);
        castle.transform.localScale = Vector3.one * 1.02f;

        var hole = new GameObject("MouseHole");
        var holeSr = hole.AddComponent<SpriteRenderer>();
        holeSr.sprite = ArtWorld.Hole;
        holeSr.sortingOrder = Sorting.Order(3f) - 2;
        hole.transform.position = new Vector3(-9.05f, 3.06f, 0f);

        // Bien bao chi duong vao
        var sign = new GameObject("Sign");
        sign.transform.position = new Vector3(-8.45f, 3.95f, 0f);
        var signSr = sign.AddComponent<SpriteRenderer>();
        signSr.sprite = ArtWorld.Sign;
        signSr.sortingOrder = Sorting.Order(4.2f);
        sign.transform.localScale = Vector3.one * 0.85f;
    }
}
