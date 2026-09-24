using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class GameSetup
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string ArtFolder = "Assets/Art";
    private const string ConfigFolder = "Assets/Resources/Configs";

    // ===================== AUTO: garantir assets (idempotente) =====================
    [InitializeOnLoadMethod]
    private static void OnLoad()
    {
        EditorApplication.delayCall += EnsureAssets;
    }

    private static void EnsureAssets()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        Sprite white = EnsureWhiteSprite();
        Sprite stripe = EnsureStripeSprite();

        EnsurePrefab("Player.prefab", () => BuildPlayer(white));
        EnsurePrefab("Spike.prefab", () => BuildObstacle("Spike", white, new Color(0.9f, 0.25f, 0.25f), new Vector2(1f, 0.5f), false));
        EnsurePrefab("Wall.prefab", () => BuildObstacle("Wall", white, new Color(0.95f, 0.6f, 0.2f), new Vector2(1f, 1.8f), false));
        EnsurePrefab("GroundEnemy.prefab", () => BuildObstacle("GroundEnemy", white, new Color(0.7f, 0.3f, 0.8f), new Vector2(1f, 1.5f), true));
        EnsurePrefab("Projectile.prefab", () => BuildProjectile(white));

        EnsureConfig(typeof(GameConfig), "GameConfig");
        EnsureConfig(typeof(PlayerConfig), "PlayerConfig");
        EnsureConfig(typeof(PlayerAnimationSet), "PlayerAnimationSet");
        EnsureConfig(typeof(DifficultyProfile), "DifficultyProfile");
        EnsureConfig(typeof(ProjectileConfig), "ProjectileConfig");
        EnsureConfig(typeof(ChunkLibrary), "ChunkLibrary");

        EnsurePlayerBaseController();

        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
    }

    // ===================== MANUAL: garantir cena (aditivo) =====================
    [MenuItem("SMC/Ensure Game Scene")]
    public static void EnsureScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        EnsureAssets();

        var scene = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("[SMC] A cena ainda nao foi salva. Use File > Save e rode de novo.");
            return;
        }

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Player.prefab");
        GameObject spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Spike.prefab");
        GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Wall.prefab");
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/GroundEnemy.prefab");
        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Projectile.prefab");
        Sprite stripe = EnsureStripeSprite();

        CameraController camCtrl = EnsureCamera();
        EnsureGlobalLight();
        DifficultyDriver difficulty = EnsureGameManager();
        PlayerController player = EnsurePlayer(playerPrefab, difficulty);

        if (camCtrl != null && camCtrl.target == null && player != null) camCtrl.target = player.transform;

        EnsureLevelGenerator(player, difficulty, spikePrefab, wallPrefab, enemyPrefab);
        EnsureProjectileSpawner(difficulty, projectilePrefab);
        EnsureParallax(stripe);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[SMC] Ensure Game Scene concluido (apenas adicionou o que faltava).");
    }

    // ===================== Cena (ensure) =====================
    private static CameraController EnsureCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<CameraController>();
        }

        var ctrl = cam.GetComponent<CameraController>();
        if (ctrl == null) ctrl = cam.gameObject.AddComponent<CameraController>();
        return ctrl;
    }

    private static void EnsureGlobalLight()
    {
        if (Object.FindFirstObjectByType<Light2D>() != null) return;

        var go = new GameObject("Global Light 2D");
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color = Color.white;
    }

    private static DifficultyDriver EnsureGameManager()
    {
        var gm = Object.FindFirstObjectByType<GameManager>();
        GameObject gmGo = gm != null ? gm.gameObject : new GameObject("GameManager");

        if (gm == null) gm = gmGo.AddComponent<GameManager>();
        if (gmGo.GetComponent<ObjectPool>() == null) gmGo.AddComponent<ObjectPool>();
        if (gmGo.GetComponent<AudioManager>() == null) gmGo.AddComponent<AudioManager>();

        var difficulty = gmGo.GetComponent<DifficultyDriver>();
        if (difficulty == null) difficulty = gmGo.AddComponent<DifficultyDriver>();

        if (gmGo.GetComponent<FeedbackManager>() == null) gmGo.AddComponent<FeedbackManager>();
        if (gmGo.GetComponent<UIManager>() == null) gmGo.AddComponent<UIManager>();
        return difficulty;
    }

    private static PlayerController EnsurePlayer(GameObject playerPrefab, DifficultyDriver difficulty)
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            if (player.difficulty == null) player.difficulty = difficulty;
            return player;
        }

        if (playerPrefab == null) return null;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        go.transform.position = new Vector3(0f, 2f, 0f);
        var pc = go.GetComponent<PlayerController>();
        if (pc != null) pc.difficulty = difficulty;
        return pc;
    }

    private static void EnsureLevelGenerator(PlayerController player, DifficultyDriver difficulty, GameObject spike, GameObject wall, GameObject enemy)
    {
        var gen = Object.FindFirstObjectByType<LevelGenerator>();
        if (gen == null)
        {
            var go = new GameObject("LevelGenerator");
            gen = go.AddComponent<LevelGenerator>();
        }

        if (gen.player == null && player != null) gen.player = player.transform;
        if (gen.difficulty == null) gen.difficulty = difficulty;

        EnsureTemplate(gen, ObstacleKind.Spike, spike);
        EnsureTemplate(gen, ObstacleKind.Wall, wall);
        EnsureTemplate(gen, ObstacleKind.GroundEnemy, enemy);
    }

    private static void EnsureTemplate(LevelGenerator gen, ObstacleKind kind, GameObject template)
    {
        if (template == null) return;

        LevelGenerator.TemplateEntry entry = null;
        foreach (var t in gen.templates)
        {
            if (t != null && t.kind == kind) { entry = t; break; }
        }

        if (entry == null)
            gen.templates.Add(new LevelGenerator.TemplateEntry { kind = kind, template = template });
        else if (entry.template == null)
            entry.template = template;
    }

    private static void EnsureProjectileSpawner(DifficultyDriver difficulty, GameObject projectilePrefab)
    {
        var spawner = Object.FindFirstObjectByType<ProjectileSpawner>();
        if (spawner == null)
        {
            var go = new GameObject("ProjectileSpawner");
            spawner = go.AddComponent<ProjectileSpawner>();
        }

        if (spawner.difficulty == null) spawner.difficulty = difficulty;
        if (spawner.projectileTemplate == null) spawner.projectileTemplate = projectilePrefab;
    }

    private static void EnsureParallax(Sprite stripe)
    {
        var bg = Object.FindFirstObjectByType<ParallaxBackground>();
        if (bg != null) return;

        var go = new GameObject("ParallaxBackground");
        bg = go.AddComponent<ParallaxBackground>();

        if (stripe != null)
        {
            bg.layers.Add(new ParallaxLayerConfig { name = "Far", sprite = stripe, tint = new Color(0.5f, 0.55f, 0.65f, 1f), scrollSpeed = 0.15f, sortingOrder = -30, yPosition = 4.5f, scale = 1f });
            bg.layers.Add(new ParallaxLayerConfig { name = "Mid", sprite = stripe, tint = new Color(0.75f, 0.75f, 0.85f, 1f), scrollSpeed = 0.4f, sortingOrder = -20, yPosition = 2.5f, scale = 1f });
        }
    }

    // ===================== Prefabs =====================
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
        if (!File.Exists(path))
        {
            if (!Directory.Exists(PrefabFolder)) Directory.CreateDirectory(PrefabFolder);
            var go = build();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void EnsurePrefabComponent(string fileName, System.Type componentType)
    {
        string path = PrefabFolder + "/" + fileName;
        if (!File.Exists(path)) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null || prefab.GetComponent(componentType) != null) return;

        var contents = PrefabUtility.LoadPrefabContents(path);
        contents.AddComponent(componentType);
        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
        Debug.Log("[SMC] Componente " + componentType.Name + " adicionado ao prefab " + fileName);
    }

    // ===================== Configs =====================
    private static void EnsureConfig(System.Type type, string name)
    {
        string path = ConfigFolder + "/" + name + ".asset";
        if (File.Exists(path)) return;

        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

        var asset = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }

    // ===================== Controller base do jogador =====================
    private const string BaseControllerPath = "Assets/Resources/Configs/PlayerStateController.controller";

    private static void EnsurePlayerBaseController()
    {
        if (File.Exists(BaseControllerPath)) return;
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(BaseControllerPath);
        var sm = controller.layers[0].stateMachine;

        AddState(sm, "Run", LoadClip("Assets/Art/Mauro/Run/Run.anim"));
        AddState(sm, "JumpUp", LoadClip("Assets/Art/Mauro/JumpUp/JumpUp.anim"));
        AddState(sm, "JumpDown", LoadClip("Assets/Art/Mauro/JumpDown/JumpDown.anim"));
        AddState(sm, "Landing", LoadClip("Assets/Art/Mauro/JumpDown/Landing.anim"));
        AddState(sm, "Parry", LoadClip("Assets/Art/Mauro/Parry/Parry.anim"));

        AssetDatabase.SaveAssets();
        Debug.Log("[SMC] Base controller gerado em " + BaseControllerPath);
    }

    private static AnimationClip LoadClip(string path)
    {
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    private static void AddState(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        var state = sm.AddState(name);
        state.motion = clip;
        if (sm.defaultState == null) sm.defaultState = state;
    }

    // ===================== Sprites placeholder =====================
    private static Sprite EnsureWhiteSprite()
    {
        const string path = "Assets/Art/White.png";
        if (!Directory.Exists(ArtFolder)) Directory.CreateDirectory(ArtFolder);

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
        if (imp != null && (imp.textureType != TextureImporterType.Sprite || imp.spritePixelsPerUnit != 4f))
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 4f;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }

        return LoadSprite(path);
    }

    private static Sprite EnsureStripeSprite()
    {
        const string path = "Assets/Art/ParallaxStripe.png";
        if (!Directory.Exists(ArtFolder)) Directory.CreateDirectory(ArtFolder);

        if (!File.Exists(path))
        {
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
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 64f;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }

        return LoadSprite(path);
    }

    private static Sprite LoadSprite(string path)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets) if (a is Sprite) return (Sprite)a;
        return null;
    }
}