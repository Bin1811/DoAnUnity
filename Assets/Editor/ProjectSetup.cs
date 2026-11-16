using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

// Tao scene, cau hinh build va dong goi game. Chay trong Unity Editor hoac bang batchmode.
public static class ProjectSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string BuildPath = "Builds/Windows/MeoThuThanh.exe";

    [MenuItem("Mèo Thủ Thành/Thiết lập Scene")]
    public static void Setup()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.8f;
        cam.transform.position = new Vector3(0f, 0.65f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Pal.Forest;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFit>();
        camGo.AddComponent<CameraShake>();

        var game = new GameObject("Game");
        game.AddComponent<Bootstrap>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.companyName = "Meo Studio";
        PlayerSettings.productName = "Mèo Thủ Thành";
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.meostudio.meothuthanh");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ProjectSetup] Đã tạo scene: " + ScenePath);
    }

    [MenuItem("Mèo Thủ Thành/Build Windows (.exe)")]
    public static void BuildWindows()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log("[ProjectSetup] BUILD " + summary.result + " | size=" + summary.totalSize +
                  " | errors=" + summary.totalErrors + " | time=" + summary.totalTime);
        if (summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    public static void SetupAndBuild()
    {
        Setup();
        BuildWindows();
    }
}
