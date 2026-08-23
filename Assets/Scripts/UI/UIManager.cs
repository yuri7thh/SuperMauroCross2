using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIManager : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _menuPanel;
    private RectTransform _hudPanel;
    private RectTransform _gameOverPanel;
    private RectTransform _optionsPanel;

    private Text _distanceText;
    private Text _scoreText;
    private Text _comboText;
    private Text _finalScoreText;
    private Text _bestScoreText;
    private Slider _volumeSlider;

    private GameManager _game;
    private AudioManager _audio;
    private FeedbackManager _feedback;

    private Font _font;

    void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        BuildCanvas();
        BuildEventSystem();
        BuildMenu();
        BuildHUD();
        BuildGameOver();
        BuildOptions();

        SetPanel(_menuPanel, false);
        SetPanel(_hudPanel, false);
        SetPanel(_gameOverPanel, false);
        SetPanel(_optionsPanel, false);
    }

    void Start()
    {
        _game = GameManager.Instance;
        _audio = AudioManager.Instance;
        _feedback = FindFirstObjectByType<FeedbackManager>();

        LoadSettings();

        if (_volumeSlider != null && _audio != null) _volumeSlider.value = _audio.volume;

        if (_game != null)
        {
            _game.OnStateChanged += OnStateChanged;
            _game.OnScoreChanged += OnScoreChanged;
            _game.OnComboChanged += OnComboChanged;
            OnStateChanged(_game.State);
        }
    }

    private void LoadSettings()
    {
        if (_game != null)
        {
            _game.ParryEnabled = PlayerPrefs.GetInt("SMC_parry", _game.ParryEnabled ? 1 : 0) == 1;
            _game.ComboEnabled = PlayerPrefs.GetInt("SMC_combo", _game.ComboEnabled ? 1 : 0) == 1;
            _game.ProjectilesEnabled = PlayerPrefs.GetInt("SMC_projectiles", _game.ProjectilesEnabled ? 1 : 0) == 1;
        }
        if (_feedback != null)
        {
            _feedback.slowMoEnabled = PlayerPrefs.GetInt("SMC_slowmo", _feedback.slowMoEnabled ? 1 : 0) == 1;
            _feedback.shakeEnabled = PlayerPrefs.GetInt("SMC_shake", _feedback.shakeEnabled ? 1 : 0) == 1;
            _feedback.particlesEnabled = PlayerPrefs.GetInt("SMC_particles", _feedback.particlesEnabled ? 1 : 0) == 1;
        }
        if (_audio != null)
        {
            _audio.audioEnabled = PlayerPrefs.GetInt("SMC_audio", _audio.audioEnabled ? 1 : 0) == 1;
            _audio.volume = PlayerPrefs.GetFloat("SMC_volume", _audio.volume);
        }
    }

    void OnDestroy()
    {
        if (_game != null)
        {
            _game.OnStateChanged -= OnStateChanged;
            _game.OnScoreChanged -= OnScoreChanged;
            _game.OnComboChanged -= OnComboChanged;
        }
    }

    private void OnStateChanged(GameState state)
    {
        SetPanel(_menuPanel, state == GameState.Menu);
        SetPanel(_hudPanel, state == GameState.Playing);
        SetPanel(_gameOverPanel, state == GameState.GameOver);
        SetPanel(_optionsPanel, false);

        if (state == GameState.GameOver && _game != null)
        {
            if (_finalScoreText != null) _finalScoreText.text = "Score: " + _game.Score.ToString("F0");
            if (_bestScoreText != null) _bestScoreText.text = "Recorde: " + _game.BestScore.ToString("F0");
        }
    }

    private void OnScoreChanged(float score)
    {
        if (_game == null) return;
        if (_distanceText != null) _distanceText.text = "Distancia: " + _game.Distance.ToString("F0") + " m";
        if (_scoreText != null) _scoreText.text = "Score: " + score.ToString("F0");
    }

    private void OnComboChanged(int combo)
    {
        if (_game == null) return;
        if (_comboText != null) _comboText.text = "Combo x" + combo + "  (mult x" + _game.Multiplier.ToString("F1") + ")";
    }

    private void ShowOptions(bool show)
    {
        SetPanel(_optionsPanel, show);
        if (_game != null)
        {
            SetPanel(_menuPanel, !show && _game.State == GameState.Menu);
            SetPanel(_hudPanel, !show && _game.State == GameState.Playing);
        }
    }

    private void SetPanel(RectTransform panel, bool show)
    {
        if (panel != null) panel.gameObject.SetActive(show);
    }

    private void BuildCanvas()
    {
        var canvasGo = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = canvasGo.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void BuildEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        var esGo = new GameObject("EventSystem", typeof(EventSystem));
        var uiModule = esGo.AddComponent<InputSystemUIInputModule>();
        var actions = InputSystem.actions;
        if (actions != null) uiModule.actionsAsset = actions;
    }

    private void BuildMenu()
    {
        _menuPanel = MakePanel("MenuPanel", new Color(0f, 0f, 0f, 0.85f));

        MakeText(_menuPanel, "SUPER MAURO CROSS 2", 72, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1000, 120));
        MakeText(_menuPanel, "Infinite Runner + Parry", 30, new Color(0.8f, 0.8f, 0.8f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(600, 40));

        MakeButton(_menuPanel, "JOGAR", 40, new Vector2(0, 0), new Vector2(360, 90), new Color(0.2f, 0.7f, 0.3f, 1f), () => { if (_game != null) _game.StartGame(); });
        MakeButton(_menuPanel, "OPCOES", 34, new Vector2(0, -120), new Vector2(300, 70), new Color(0.25f, 0.3f, 0.4f, 1f), () => ShowOptions(true));

        MakeText(_menuPanel, "Espaco = pular   |   K = parry (no ar)", 24, new Color(0.7f, 0.7f, 0.7f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -260), new Vector2(700, 30));
    }

    private void BuildHUD()
    {
        _hudPanel = MakePanel("HUDPanel", new Color(0f, 0f, 0f, 0f));

        _distanceText = MakeText(_hudPanel, "", 30, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(450, 40));
        _scoreText = MakeText(_hudPanel, "", 30, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -70), new Vector2(450, 40));
        _comboText = MakeText(_hudPanel, "", 36, new Color(1f, 0.85f, 0.3f, 1f), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(520, 40));
    }

    private void BuildGameOver()
    {
        _gameOverPanel = MakePanel("GameOverPanel", new Color(0f, 0f, 0f, 0.85f));

        MakeText(_gameOverPanel, "GAME OVER", 72, new Color(0.9f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(800, 110));
        _finalScoreText = MakeText(_gameOverPanel, "", 36, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(600, 40));
        _bestScoreText = MakeText(_gameOverPanel, "", 30, new Color(0.8f, 0.8f, 0.8f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(600, 40));

        MakeButton(_gameOverPanel, "REINICIAR", 36, new Vector2(0, -160), new Vector2(340, 80), new Color(0.2f, 0.7f, 0.3f, 1f), () => { if (_game != null) _game.Restart(); });
        MakeButton(_gameOverPanel, "MENU", 30, new Vector2(0, -260), new Vector2(240, 60), new Color(0.25f, 0.3f, 0.4f, 1f), () => { if (_game != null) _game.Restart(); });
    }

    private void BuildOptions()
    {
        _optionsPanel = MakePanel("OptionsPanel", new Color(0f, 0f, 0f, 0.9f));

        MakeText(_optionsPanel, "OPCOES", 56, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(600, 80));

        float y = 150;

        MakeOptionToggle(_optionsPanel, "Parry", new Vector2(0, y),
            () => _game != null && _game.ParryEnabled,
            v => { if (_game != null) { _game.ParryEnabled = v; PlayerPrefs.SetInt("SMC_parry", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Combo", new Vector2(0, y),
            () => _game != null && _game.ComboEnabled,
            v => { if (_game != null) { _game.ComboEnabled = v; PlayerPrefs.SetInt("SMC_combo", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Projeteis", new Vector2(0, y),
            () => _game != null && _game.ProjectilesEnabled,
            v => { if (_game != null) { _game.ProjectilesEnabled = v; PlayerPrefs.SetInt("SMC_projectiles", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Slow motion", new Vector2(0, y),
            () => _feedback != null && _feedback.slowMoEnabled,
            v => { if (_feedback != null) { _feedback.slowMoEnabled = v; PlayerPrefs.SetInt("SMC_slowmo", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Screen shake", new Vector2(0, y),
            () => _feedback != null && _feedback.shakeEnabled,
            v => { if (_feedback != null) { _feedback.shakeEnabled = v; PlayerPrefs.SetInt("SMC_shake", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Particulas", new Vector2(0, y),
            () => _feedback != null && _feedback.particlesEnabled,
            v => { if (_feedback != null) { _feedback.particlesEnabled = v; PlayerPrefs.SetInt("SMC_particles", v ? 1 : 0); } });
        y -= 70;

        MakeOptionToggle(_optionsPanel, "Som", new Vector2(0, y),
            () => _audio != null && _audio.audioEnabled,
            v => { if (_audio != null) { _audio.audioEnabled = v; PlayerPrefs.SetInt("SMC_audio", v ? 1 : 0); } });
        y -= 70;

        _volumeSlider = MakeSlider(_optionsPanel, "Volume", new Vector2(0, y), 0f, 1f, 1f,
            v => { if (_audio != null) { _audio.volume = v; PlayerPrefs.SetFloat("SMC_volume", v); } });

        MakeButton(_optionsPanel, "VOLTAR", 32, new Vector2(0, -330), new Vector2(240, 60), new Color(0.25f, 0.3f, 0.4f, 1f), () => ShowOptions(false));
    }

    private RectTransform MakePanel(string name, Color bg)
    {
        var rt = CreateRect(name, _canvas.transform);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = PlaceholderFactory.WhiteSprite;
        img.color = bg;
        return rt;
    }

    private Text MakeText(Transform parent, string text, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var t = rt.gameObject.AddComponent<Text>();
        t.font = _font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private Button MakeButton(Transform parent, string label, int fontSize, Vector2 anchoredPos, Vector2 sizeDelta, Color color, UnityAction onClick)
    {
        var rt = CreateRect("Button", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = PlaceholderFactory.WhiteSprite;
        img.color = color;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        MakeText(rt, label, fontSize, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        return btn;
    }

    private void MakeOptionToggle(Transform parent, string label, Vector2 anchoredPos, Func<bool> getter, Action<bool> setter)
    {
        Button button = null;
        button = MakeButton(parent, label + ": " + (getter() ? "ON" : "OFF"), 26, anchoredPos, new Vector2(360, 54), new Color(0.15f, 0.15f, 0.2f, 1f), () =>
        {
            bool v = !getter();
            setter(v);
            var text = button.GetComponentInChildren<Text>();
            if (text != null) text.text = label + ": " + (v ? "ON" : "OFF");
        });
    }

    private Slider MakeSlider(Transform parent, string label, Vector2 anchoredPos, float min, float max, float initial, Action<float> onChange)
    {
        MakeText(parent, label, 26, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos + new Vector2(-210, 0), new Vector2(320, 30));

        var rt = CreateRect("Slider", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos + new Vector2(70, 0);
        rt.sizeDelta = new Vector2(280, 40);

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = PlaceholderFactory.WhiteSprite;
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        var slider = rt.gameObject.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = initial;
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
        fill.sprite = PlaceholderFactory.WhiteSprite;
        fill.color = new Color(0.3f, 0.7f, 0.9f, 1f);

        slider.fillRect = frt;
        slider.onValueChanged.AddListener(v => onChange(v));

        return slider;
    }

    private RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
}