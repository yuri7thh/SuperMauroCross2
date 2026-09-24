using UnityEngine;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Build()
    {
        if (Object.FindFirstObjectByType<LevelGenerator>() != null) return;
        if (Object.FindFirstObjectByType<GameManager>() != null) return;

        Time.timeScale = 1f;

        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        gmGo.AddComponent<ObjectPool>();
        gmGo.AddComponent<AudioManager>();

        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
        }
        if (cam.GetComponent<CameraController>() == null)
            cam.gameObject.AddComponent<CameraController>();

        var feedback = gmGo.AddComponent<FeedbackManager>();
        gmGo.AddComponent<UIManager>();
        var difficulty = gmGo.AddComponent<DifficultyDriver>();

        var playerGo = PlaceholderFactory.CreateBox("Player", new Color(0.2f, 0.8f, 0.3f), new Vector2(1f, 1.5f), false);
        playerGo.tag = "Player";
        playerGo.transform.position = new Vector3(0f, 2f, 0f);
        var rb = playerGo.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var pc = playerGo.AddComponent<PlayerController>();
        pc.difficulty = difficulty;
        playerGo.AddComponent<InputReader>();

        var spike = PlaceholderFactory.CreateBox("Spike", new Color(0.9f, 0.25f, 0.25f), new Vector2(1f, 0.5f), true);
        spike.AddComponent<Hazard>();

        var wall = PlaceholderFactory.CreateBox("Wall", new Color(0.95f, 0.6f, 0.2f), new Vector2(1f, 1.8f), true);
        wall.AddComponent<Hazard>();

        var enemy = PlaceholderFactory.CreateBox("GroundEnemy", new Color(0.7f, 0.3f, 0.8f), new Vector2(1f, 1.5f), true);
        enemy.AddComponent<Hazard>();
        enemy.AddComponent<GroundEnemy>();

        var projectile = PlaceholderFactory.CreateBox("Projectile", new Color(0.2f, 0.9f, 0.9f), new Vector2(0.6f, 0.6f), true);
        projectile.AddComponent<Projectile>();

        spike.SetActive(false);
        wall.SetActive(false);
        enemy.SetActive(false);
        projectile.SetActive(false);

        var particleTemplate = new GameObject("ParryParticleTemplate");
        var particleSr = particleTemplate.AddComponent<SpriteRenderer>();
        particleSr.sprite = PlaceholderFactory.WhiteSprite;
        particleSr.color = Color.white;
        particleTemplate.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        particleTemplate.AddComponent<ParticleMover>();
        particleTemplate.SetActive(false);
        feedback.particleTemplate = particleTemplate;

        var genGo = new GameObject("LevelGenerator");
        var gen = genGo.AddComponent<LevelGenerator>();
        gen.player = playerGo.transform;
        gen.difficulty = difficulty;
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.Spike, template = spike });
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.Wall, template = wall });
        gen.templates.Add(new LevelGenerator.TemplateEntry { kind = ObstacleKind.GroundEnemy, template = enemy });

        var projGo = new GameObject("ProjectileSpawner");
        var spawner = projGo.AddComponent<ProjectileSpawner>();
        spawner.projectileTemplate = projectile;
        spawner.difficulty = difficulty;
    }
}
