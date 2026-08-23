using System.IO;
using UnityEditor;
using UnityEngine;

public static class ConfigAssetGenerator
{
    private const string Folder = "Assets/Resources/Configs";

    [InitializeOnLoadMethod]
    private static void EnsureConfigs()
    {
        Ensure(typeof(GameConfig), "GameConfig");
        Ensure(typeof(PlayerConfig), "PlayerConfig");
        Ensure(typeof(DifficultyProfile), "DifficultyProfile");
        Ensure(typeof(ProjectileConfig), "ProjectileConfig");
        Ensure(typeof(ChunkLibrary), "ChunkLibrary");
    }

    private static void Ensure(System.Type type, string name)
    {
        string path = Folder + "/" + name + ".asset";
        if (File.Exists(path)) return;

        if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

        var asset = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }
}