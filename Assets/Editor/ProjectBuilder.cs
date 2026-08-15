using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MallDemo;

public static class ProjectBuilder
{
    private const string RuntimeMaterialPath = "Assets/Resources/RuntimeDefaultMaterial.mat";

    private static void EnsureRuntimeMaterialAsset()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath) != null) return;

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) throw new InvalidOperationException("Unity could not resolve a buildable runtime shader.");

        AssetDatabase.CreateAsset(new Material(shader), RuntimeMaterialPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void ValidateScriptAssets()
    {
        ValidateScript("Assets/Scripts/MallApp.cs");
        ValidateScript("Assets/Scripts/CatalogService.cs");
    }

    private static void ValidateScript(string path)
    {
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        Debug.Log($"MALL_DEMO_SCRIPT:{path}:guid={AssetDatabase.AssetPathToGUID(path)}:class={(script == null ? "null" : script.GetClass()?.FullName ?? "no-class")}");
    }

    public static void BuildStandalone()
    {
        EnsureRuntimeMaterialAsset();
        string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Linux"));
        Directory.CreateDirectory(outputDirectory);
        string outputPath = Path.Combine(outputDirectory, "MallDemo.x86_64");
        BuildReport report = BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/Main.unity" },
            outputPath,
            BuildTarget.StandaloneLinux64,
            BuildOptions.None);
        Debug.Log($"MALL_DEMO_BUILD_RESULT:{report.summary.result}:{report.summary.totalSize}");
    }

    public static void BuildProject()
    {
        EnsureRuntimeMaterialAsset();
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject app = new GameObject("MallDemoApp");
        app.AddComponent<CatalogService>();
        app.AddComponent<MallApp>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("MALL_DEMO_SCENE_CREATED");
    }
}
