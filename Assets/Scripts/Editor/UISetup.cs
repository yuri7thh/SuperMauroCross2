using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class UISetup
{
    private const string WhiteSpritePath = "Assets/Art/White.png";

    [MenuItem("SMC/Ensure UI")]
    public static void EnsureUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        GameSetup.EnsureAssets();

        var ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null)
        {
            Debug.LogWarning("[SMC] UIManager nao encontrado. Rode 'SMC/Ensure Game Scene' primeiro.");
            return;
        }

        if (ui.menuPanel != null)
        {
            Debug.Log("[SMC] UI ja existe; nada a fazer.");
            return;
        }

        BuildUI(ui);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SMC] Canvas de UI criado na cena e ligado ao UIManager.");
    }

    private static void BuildUI(UIManager ui)
    {
        EnsureEventSystem();

        var canvasGo = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvasT = canvasGo.transform;

        BuildMenu(ui, canvasT);
        BuildHUD(ui, canvasT);
        BuildGameOver(ui, canvasT);
        BuildOptions(ui, canvasT);
    }

    private static void BuildMenu(UIManager ui, Transform canvas)
    {
        var menu = MakePanel("MenuPanel", new Color(0f, 0f, 0f, 0.55f), canvas);
        menu.gameObject.AddComponent<CanvasGroup>();

        CenterText(menu, "SUPER MAURO CROSS 2", 72, Color.white, new Vector2(0, 220), new Vector2(1000, 120));
        CenterText(menu, "Infinite Runner + Parry", 30, new Color(0.8f, 0.8f, 0.8f, 1f), new Vector2(0, 150), new Vector2(600, 40));

        ui.playButton = MakeButton(menu, "JOGAR", 40, new Vector2(0, 0), new Vector2(360, 90), new Color(0.2f, 0.7f, 0.3f, 1f));
        ui.optionsButton = MakeButton(menu, "OPCOES", 34, new Vector2(0, -120), new Vector2(300, 70), new Color(0.25f, 0.3f, 0.4f, 1f));

        CenterText(menu, "Espaco = pular   |   K = parry (no ar)", 24, new Color(0.7f, 0.7f, 0.7f, 1f), new Vector2(0, -260), new Vector2(700, 30));

        ui.menuPanel = menu.gameObject;
    }

    private static void BuildHUD(UIManager ui, Transform canvas)
    {
        var hud = MakePanel("HUDPanel", new Color(0f, 0f, 0f, 0f), canvas);

        ui.distanceText = AnchorText(hud, "", 30, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(450, 40));
        ui.scoreText = AnchorText(hud, "", 30, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -70), new Vector2(450, 40));
        ui.comboText = AnchorText(hud, "", 36, new Color(1f, 0.85f, 0.3f, 1f), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(520, 40));

        ui.hudPanel = hud.gameObject;
    }

    private static void BuildGameOver(UIManager ui, Transform canvas)
    {
        var go = MakePanel("GameOverPanel", new Color(0f, 0f, 0f, 0.85f), canvas);

        CenterText(go, "GAME OVER", 72, new Color(0.9f, 0.2f, 0.2f, 1f), new Vector2(0, 140), new Vector2(800, 110));
        ui.finalScoreText = CenterText(go, "", 36, Color.white, new Vector2(0, 20), new Vector2(600, 40));
        ui.bestScoreText = CenterText(go, "", 30, new Color(0.8f, 0.8f, 0.8f, 1f), new Vector2(0, -40), new Vector2(600, 40));

        ui.restartButton = MakeButton(go, "REINICIAR", 36, new Vector2(0, -160), new Vector2(340, 80), new Color(0.2f, 0.7f, 0.3f, 1f));
        ui.menuButton = MakeButton(go, "MENU", 30, new Vector2(0, -260), new Vector2(240, 60), new Color(0.25f, 0.3f, 0.4f, 1f));

        ui.gameOverPanel = go.gameObject;
    }

    private static void BuildOptions(UIManager ui, Transform canvas)
    {
        var opts = MakePanel("OptionsPanel", new Color(0f, 0f, 0f, 0.9f), canvas);

        CenterText(opts, "OPCOES", 56, Color.white, new Vector2(0, 250), new Vector2(600, 80));

        float y = 150;

        ui.parryToggle = MakeToggle(opts, "Parry", new Vector2(0, y)); y -= 70;
        ui.comboToggle = MakeToggle(opts, "Combo", new Vector2(0, y)); y -= 70;
        ui.projectilesToggle = MakeToggle(opts, "Projeteis", new Vector2(0, y)); y -= 70;
        ui.slowMoToggle = MakeToggle(opts, "Slow motion", new Vector2(0, y)); y -= 70;
        ui.shakeToggle = MakeToggle(opts, "Screen shake", new Vector2(0, y)); y -= 70;
        ui.particlesToggle = MakeToggle(opts, "Particulas", new Vector2(0, y)); y -= 70;
        ui.soundToggle = MakeToggle(opts, "Som", new Vector2(0, y)); y -= 70;

        ui.volumeSlider = MakeSlider(opts, "Volume", new Vector2(0, y));

        ui.backButton = MakeButton(opts, "VOLTAR", 32, new Vector2(0, -330), new Vector2(240, 60), new Color(0.25f, 0.3f, 0.4f, 1f));

        ui.optionsPanel = opts.gameObject;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var esGo = new GameObject("EventSystem", typeof(EventSystem));
        var uiModule = esGo.AddComponent<InputSystemUIInputModule>();
        var actions = InputSystem.actions;
        if (actions != null) uiModule.actionsAsset = actions;
    }

    private static RectTransform MakePanel(string name, Color bg, Transform canvas)
    {
        var rt = CreateRect(name, canvas);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = bg;
        return rt;
    }

    private static Text CenterText(Transform parent, string text, int size, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        return AnchorText(parent, text, size, color,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            anchoredPos, sizeDelta);
    }

    private static Text AnchorText(Transform parent, string text, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var t = rt.gameObject.AddComponent<Text>();
        t.font = GetFont();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static Button MakeButton(Transform parent, string label, int fontSize, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var rt = CreateRect("Button", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = color;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        AnchorText(rt, label, fontSize, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        return btn;
    }

    private static Toggle MakeToggle(Transform parent, string label, Vector2 anchoredPos)
    {
        var rt = CreateRect("Toggle", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(360, 54);

        var bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        var toggle = rt.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = bg;

        var checkRt = CreateRect("Checkmark", rt);
        checkRt.anchorMin = new Vector2(0f, 0.5f);
        checkRt.anchorMax = new Vector2(0f, 0.5f);
        checkRt.pivot = new Vector2(0f, 0.5f);
        checkRt.anchoredPosition = new Vector2(18f, 0f);
        checkRt.sizeDelta = new Vector2(26, 26);
        var check = checkRt.gameObject.AddComponent<Image>();
        check.sprite = GetWhiteSprite();
        check.color = new Color(0.2f, 1f, 0.35f, 1f);
        toggle.graphic = check;

        AnchorText(rt, label, 26, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        return toggle;
    }

    private static Slider MakeSlider(Transform parent, string label, Vector2 anchoredPos)
    {
        AnchorText(parent, label, 26, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos + new Vector2(-210, 0), new Vector2(320, 30));

        var rt = CreateRect("Slider", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos + new Vector2(70, 0);
        rt.sizeDelta = new Vector2(280, 40);

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        var slider = rt.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.targetGraphic = img;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var fill = fillGo.GetComponent<Image>();
        var frt = (RectTransform)fill.transform;
        frt.SetParent(rt, false);
        frt.anchorMin = new Vector2(0f, 0.5f);
        frt.anchorMax = new Vector2(0f, 0.5f);
        frt.pivot = new Vector2(0f, 0.5f);
        frt.anchoredPosition = Vector2.zero;
        frt.sizeDelta = new Vector2(280, 10);
        fill.sprite = GetWhiteSprite();
        fill.color = new Color(0.3f, 0.7f, 0.9f, 1f);
        slider.fillRect = frt;

        return slider;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static Sprite GetWhiteSprite()
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
    }

    private static Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}