using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GameSceneBuilder
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string ScenePath = "Assets/Scenes/Game.unity";

    [MenuItem("SMC/Build Game Scene")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        Sprite white = EnsureWhiteSprite();

        GameObject playerPrefab = EnsurePrefab("Player.prefab", () => BuildPlayer(white));
        GameObject spikePrefab = EnsurePrefab("Spike.prefab", () => BuildObstacle("Spike", white, new Color(0.9f, 0.25f, 0.25f), new Vector2(1f, 0.5f), false));
        GameObject wallPrefab = EnsurePrefab("Wall.prefab", () => BuildObstacle("Wall", white, new Color(0.95f, 0.6f, 0.2f), new Vector2(1f, 1.8f), false));
        GameObject enemyPrefab = EnsurePrefab("GroundEnemy.prefab", () => BuildObstacle("GroundEnemy", white, new Color(0.7f, 0.3f, 0.8f), new Vector2(1f, 1.5f), true));
        GameObject projectilePrefab = EnsurePrefab("Projectile.prefab", () => BuildProjectile(white));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CameraController camCtrl = BuildCamera();
        BuildGlobalLight();
        BuildParallax();
        DifficultyDriver difficulty = BuildGameManager();
        GameObject playerGo = BuildPlayerInstance(playerPrefab, difficulty);
        if (camCtrl != null) camCtrl.target = playerGo.transform;
        BuildLevelGenerator(playerGo, difficulty, spikePrefab, wallPrefab, enemyPrefab);
        BuildProjectileSpawner(difficulty, projectilePrefab);

        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        Debug.Log("[SMC] Cena gerada em " + ScenePath + ". Abra-a e de Play.");
    }

    [InitializeOnLoadMethod]
    private static void AutoBuildIfMissing()
    {
        if (File.Exists(ScenePath)) return;
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(ScenePath)) Build();
        };
    }

    private static void BuildParallax()
    {
        Sprite stripe = EnsureStripeSprite();
        if (stripe == null) return;

        CreateParallaxLayer("Parallax_Far", stripe, 0.15f, -30, new Color(0.5f, 0.55f, 0.65f, 1f), 4.5f, 6f, 6f);
        CreateParallaxLayer("Parallax_Mid", stripe, 0.4f, -20, new Color(0.75f, 0.75f, 0.85f, 1f), 2.5f, 8f, 4f);
    }

    private static void CreateParallaxLayer(string name, Sprite sprite, float speed, int order, Color tint, float y, float w, float h)
    {
        var go = new GameObject(name);
        var layer = go.AddComponent<ParallaxLayer>();
        layer.sprite = sprite;
        layer.scrollSpeed = speed;
        layer.sortingOrder = order;
        layer.tint = tint;
        layer.yPosition = y;
        layer.tileWidth = w;
        layer.tileHeight = h;
    }

    private static Sprite EnsureStripeSprite()
    {
        const string path = "Assets/Art/ParallaxStripe.png";
        if (File.Exists(path))
        {
            var existing = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in existing) if (a is Sprite) return (Sprite)a;
            return null;
        }

        if (!Directory.Exists("Assets/Art")) Directory.CreateDirectory("Assets/Art");

        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool dark = (x / 8) % 2 == 0;
                tex.SetPixel(x, y, dark ? new Color(0.25f, 0.25f, 0.3f, 1f) : new Color(0.45f, 0.45f, 0.5f, 1f));
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = size;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }

        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets) if (a is Sprite) return (Sprite)a;
        return null;
    }

    private static CameraController BuildCamera()
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
        camGo.AddComponent<AudioListener>();
        var camCtrl = camGo.AddComponent<CameraController>();
        camGo.AddComponent<UniversalAdditionalCameraData>();
        return camCtrl;
    }

    private static void BuildGlobalLight()
    {
        var go = new GameObject("Global Light 2D");
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color = Color.white;
    }

    private static DifficultyDriver BuildGameManager()
    {
        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();
        gmGo.AddComponent<ObjectPool>();
        gmGo.AddComponent<AudioManager>();
        var difficulty = gmGo.AddComponent<DifficultyDriver>();
        gmGo.AddComponent<FeedbackManager>();
        gmGo.AddComponent<UIManager>();
        return difficulty;
    }

    private static GameObject BuildPlayerInstance(GameObject playerPrefab, DifficultyDriver difficulty)
    {
        var playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerGo.transform.position = new Vector3(0f, 2f, 0f);
        var pc = playerGo.GetComponent<PlayerController>();
        if (pc != null) pc.difficulty = difficulty;
        return playerGo;
    }

    private static void BuildLevelGenerator(GameObject playerGo, DifficultyDriver difficulty, GameObject spike, GameObject wall, GameObject enemy)
    {
        var go = new GameObject("LevelGenerator");
        var gen = go.AddComponent<LevelGenerator>();
        gen.player = playerGo.transform;
        gen.difficulty = difficulty;
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.Spike, template = spike });
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.Wall, template = wall });
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.GroundEnemy, template = enemy });
    }

    private static void BuildProjectileSpawner(DifficultyDriver difficulty, GameObject projectilePrefab)
    {
        var go = new GameObject("ProjectileSpawner");
        var spawner = go.AddComponent<ProjectileSpawner>();
        spawner.projectileTemplate = projectilePrefab;
        spawner.difficulty = difficulty;
    }

    private static GameObject BuildPlayer(Sprite white)
    {
        var go = PlaceholderFactory.CreateBox("Player", new Color(0.2f, 0.8f, 0.3f), new Vector2(1f, 1.5f), false);
        go.tag = "Player";
        go.GetComponent<SpriteRenderer>().sprite = white;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        go.AddComponent<PlayerController>();
        go.AddComponent<InputReader>();
        go.AddComponent<Animator>();
        return go;
    }

    private static GameObject BuildObstacle(string name, Sprite white, Color color, Vector2 size, bool moving)
    {
        var go = PlaceholderFactory.CreateBox(name, color, size, true);
        go.GetComponent<SpriteRenderer>().sprite = white;
        go.AddComponent<Hazard>();
        if (moving) go.AddComponent<GroundEnemy>();
        return go;
    }

    private static GameObject BuildProjectile(Sprite white)
    {
        var go = PlaceholderFactory.CreateBox("Projectile", new Color(0.2f, 0.9f, 0.9f), new Vector2(0.6f, 0.6f), true);
        go.GetComponent<SpriteRenderer>().sprite = white;
        go.AddComponent<Projectile>();
        return go;
    }

    private static GameObject EnsurePrefab(string fileName, System.Func<GameObject> build)
    {
        string path = PrefabFolder + "/" + fileName;
        if (File.Exists(path)) return AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (!Directory.Exists(PrefabFolder)) Directory.CreateDirectory(PrefabFolder);

        var go = build();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static Sprite EnsureWhiteSprite()
    {
        const string path = "Assets/Art/White.png";
        if (!Directory.Exists("Assets/Art")) Directory.CreateDirectory("Assets/Art");

        if (!File.Exists(path))
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null)
        {
            bool changed = imp.textureType != TextureImporterType.Sprite || imp.spritePixelsPerUnit != 4f;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 4f;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            if (changed) imp.SaveAndReimport();
        }

        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets)
            if (a is Sprite) return (Sprite)a;

        return null;
    }
}