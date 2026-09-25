using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Paineis")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;
    public GameObject optionsPanel;

    [Header("HUD")]
    public Text distanceText;
    public Text scoreText;

    [Header("Multiplicador - barra")]
    public Color multiplierBarBackground = new Color(0.1f, 0.1f, 0.12f, 0.85f);
    public Color multiplierBarFillColor = new Color(1f, 0.75f, 0.15f, 1f);
    public Color multiplierTextColor = new Color(1f, 0.85f, 0.3f, 1f);
    public Color multiplierTextGlowColor = new Color(1f, 0.95f, 0.6f, 1f);
    public float multiplierPulseSpeed = 4f;
    public float multiplierPulseScale = 1.18f;
    public float multiplierBarWidth = 320f;
    public float multiplierBarHeight = 22f;
    [Tooltip("Deslocamento vertical da barra a partir do topo da tela.")]
    public float multiplierBarYOffset = -60f;
    [Tooltip("Fonte do numero (X). Se vazio, usa a fonte embutida.")]
    public Font multiplierFont;
    [Tooltip("Tamanho da fonte do numero (X).")]
    public int multiplierFontSize = 40;
    [Tooltip("Distancia do numero (xN) abaixo da barra.")]
    public float multiplierLabelYOffset = 8f;
    [Tooltip("Largura/altura da caixa do numero (X).")]
    public Vector2 multiplierLabelSize = new Vector2(200f, 50f);

    [Header("Hover (aumentar texto)")]
    public float hoverScaleAmount = 1.2f;
    public float hoverScaleSpeed = 10f;

    [Header("Game Over")]
    public Text finalScoreText;
    public Text bestScoreText;

    [Header("Menu - botoes")]
    public Button playButton;
    public Button optionsButton;

    [Header("Game Over - botoes")]
    public Button restartButton;
    public Button menuButton;

    [Header("Opcoes")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button backButton;

    private GameManager _game;
    private AudioManager _audio;
    private CanvasGroup _menuCanvasGroup;

    private GameObject _multiplierRoot;
    private Image _multiplierFillLeft;
    private Image _multiplierFillRight;
    private Text _multiplierText;
    private static Sprite _whiteSprite;

    void Start()
    {
        _game = GameManager.Instance;
        _audio = AudioManager.Instance;

        if (menuPanel != null)
        {
            _menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (_menuCanvasGroup == null) _menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
        }

        BuildOptionsPanel();
        WireButtons();
        WireOptions();

        if (_game != null)
        {
            _game.OnStateChanged += OnStateChanged;
            _game.OnScoreChanged += OnScoreChanged;
            _game.OnComboChanged += OnComboChanged;
        }

        LoadSettings();
        RepairVolumeSliders();
        BuildMultiplierBar();
        ApplyHoverEffects();
        OnStateChanged(_game != null ? _game.State : GameState.Menu);
    }

    void Update()
    {
        if (_game == null || _multiplierRoot == null || !_multiplierRoot.activeSelf) return;

        float fill = _game.ComboTimerNormalized;
        if (_multiplierFillLeft != null) _multiplierFillLeft.fillAmount = fill;
        if (_multiplierFillRight != null) _multiplierFillRight.fillAmount = fill;

        if (_multiplierText != null)
        {
            float p = (Mathf.Sin(Time.unscaledTime * multiplierPulseSpeed) + 1f) * 0.5f;
            float s = Mathf.Lerp(1f, multiplierPulseScale, p);
            _multiplierText.rectTransform.localScale = new Vector3(s, s, 1f);
            _multiplierText.color = Color.Lerp(multiplierTextColor, multiplierTextGlowColor, p);
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

    private void WireButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(() => ShowOptions(true));
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (menuButton != null) menuButton.onClick.AddListener(Restart);
        if (backButton != null) backButton.onClick.AddListener(() => ShowOptions(false));
    }

    private void WireOptions()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void StartGame() { if (_game != null) _game.StartGame(); }
    private void Restart() { if (_game != null) _game.Restart(); }

    private IEnumerator FadeOutMenu()
    {
        if (_menuCanvasGroup != null)
        {
            float dur = 0.6f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / dur));
                yield return null;
            }
            _menuCanvasGroup.alpha = 0f;
        }
        SetPanel(menuPanel, false);
    }

    private void SetMusicVolume(float v)
    {
        if (_audio != null) _audio.SetMusicVolume(v);
        PlayerPrefs.SetFloat("SMC_musicVolume", v);
    }

    private void SetSfxVolume(float v)
    {
        if (_audio != null) _audio.volume = v;
        PlayerPrefs.SetFloat("SMC_sfxVolume", v);
    }

    private void LoadSettings()
    {
        if (_audio != null)
        {
            float music = PlayerPrefs.GetFloat("SMC_musicVolume", _audio.musicVolume);
            float sfx = PlayerPrefs.GetFloat("SMC_sfxVolume", _audio.volume);
            SetMusicVolume(music);
            SetSfxVolume(sfx);
            if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(music);
            if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfx);
        }
    }

    public void ShowOptions(bool show)
    {
        SetPanel(optionsPanel, show);
        if (_game != null)
        {
            SetPanel(menuPanel, !show && _game.State == GameState.Menu);
            SetPanel(hudPanel, !show && _game.State == GameState.Playing);
        }
    }

    private void OnStateChanged(GameState state)
    {
        SetPanel(hudPanel, state == GameState.Playing);
        SetPanel(gameOverPanel, state == GameState.GameOver);
        SetPanel(optionsPanel, false);

        if (state == GameState.Menu)
        {
            SetPanel(menuPanel, true);
            if (_menuCanvasGroup != null) _menuCanvasGroup.alpha = 1f;
        }
        else if (state == GameState.Playing || state == GameState.Transition)
        {
            StartCoroutine(FadeOutMenu());
        }

        if (state == GameState.GameOver && _game != null)
        {
            if (finalScoreText != null) finalScoreText.text = "Score: " + _game.Score.ToString("F0");
            if (bestScoreText != null) bestScoreText.text = "Recorde: " + _game.BestScore.ToString("F0");
        }
    }

    private void OnScoreChanged(float score)
    {
        if (_game == null) return;
        if (distanceText != null) distanceText.text = "Distancia: " + _game.Distance.ToString("F0") + " m";
        if (scoreText != null) scoreText.text = "Score: " + score.ToString("F0");
    }

    private void OnComboChanged(int combo)
    {
        if (_game == null) return;
        if (_multiplierRoot != null)
        {
            bool show = combo > 0 && _game.Multiplier > 1f;
            _multiplierRoot.SetActive(show);
            if (show) UpdateMultiplierText();
        }
    }

    private void UpdateMultiplierText()
    {
        if (_multiplierText != null && _game != null)
            _multiplierText.text = "x" + _game.Multiplier.ToString("F0");
    }

    private void BuildMultiplierBar()
    {
        if (hudPanel == null) return;

        var root = CreateRect("MultiplierBar", hudPanel.transform);
        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = new Vector2(0f, multiplierBarYOffset);
        root.sizeDelta = new Vector2(multiplierBarWidth, multiplierBarHeight);

        var bg = root.gameObject.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = multiplierBarBackground;
        bg.raycastTarget = false;

        var fillLeft = CreateRect("FillLeft", root);
        fillLeft.anchorMin = new Vector2(0f, 0.5f);
        fillLeft.anchorMax = new Vector2(0.5f, 0.5f);
        fillLeft.pivot = new Vector2(0.5f, 0.5f);
        fillLeft.anchoredPosition = Vector2.zero;
        fillLeft.sizeDelta = new Vector2(0f, multiplierBarHeight);

        var fillLeftImg = fillLeft.gameObject.AddComponent<Image>();
        fillLeftImg.sprite = GetWhiteSprite();
        fillLeftImg.color = multiplierBarFillColor;
        fillLeftImg.type = Image.Type.Filled;
        fillLeftImg.fillMethod = Image.FillMethod.Horizontal;
        fillLeftImg.fillOrigin = 1;
        fillLeftImg.fillAmount = 1f;
        fillLeftImg.raycastTarget = false;

        var fillRight = CreateRect("FillRight", root);
        fillRight.anchorMin = new Vector2(0.5f, 0.5f);
        fillRight.anchorMax = new Vector2(1f, 0.5f);
        fillRight.pivot = new Vector2(0.5f, 0.5f);
        fillRight.anchoredPosition = Vector2.zero;
        fillRight.sizeDelta = new Vector2(0f, multiplierBarHeight);

        var fillRightImg = fillRight.gameObject.AddComponent<Image>();
        fillRightImg.sprite = GetWhiteSprite();
        fillRightImg.color = multiplierBarFillColor;
        fillRightImg.type = Image.Type.Filled;
        fillRightImg.fillMethod = Image.FillMethod.Horizontal;
        fillRightImg.fillOrigin = 0;
        fillRightImg.fillAmount = 1f;
        fillRightImg.raycastTarget = false;

        var label = CreateRect("MultiplierLabel", root);
        label.anchorMin = new Vector2(0.5f, 0f);
        label.anchorMax = new Vector2(0.5f, 0f);
        label.pivot = new Vector2(0.5f, 1f);
        label.anchoredPosition = new Vector2(0f, -multiplierLabelYOffset);
        label.sizeDelta = multiplierLabelSize;

        var txt = label.gameObject.AddComponent<Text>();
        txt.font = multiplierFont != null ? multiplierFont : GetFont();
        txt.text = "";
        txt.fontSize = multiplierFontSize;
        txt.color = multiplierTextColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        _multiplierRoot = root.gameObject;
        _multiplierFillLeft = fillLeftImg;
        _multiplierFillRight = fillRightImg;
        _multiplierText = txt;
        _multiplierRoot.SetActive(false);
    }

    private void RepairVolumeSliders()
    {
        RepairSliderFill(musicVolumeSlider);
        RepairSliderFill(sfxVolumeSlider);
    }

    private static void RepairSliderFill(Slider slider)
    {
        if (slider == null || slider.fillRect == null) return;

        var fillRect = slider.fillRect;
        float h = fillRect.sizeDelta.y > 1f ? fillRect.sizeDelta.y : 10f;

        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(1f, 0.5f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, h);

        var fillImg = fillRect.GetComponent<Image>();
        if (fillImg != null)
        {
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        }
    }

    private void ApplyHoverEffects()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b == null || b.GetComponent<HoverScale>() != null) continue;

            var text = b.GetComponentInChildren<Text>(true);
            if (text == null) continue;

            var hs = b.gameObject.AddComponent<HoverScale>();
            hs.target = text.rectTransform;
            hs.hoverScale = hoverScaleAmount;
            hs.speed = hoverScaleSpeed;
        }
    }

    private void BuildOptionsPanel()
    {
        if (optionsPanel == null) return;
        // Preserva edicoes manuais no UICanvas: so migra o painel antigo (que tinha Toggles).
        // Se nao ha Toggles, o painel ja foi ajustado pelo usuario; nao mexe em nada.
        if (optionsPanel.GetComponentInChildren<Toggle>(true) == null) return;

        var t = optionsPanel.transform;
        foreach (Transform child in t) Destroy(child.gameObject);

        AddLabel(t, "OPCOES", 56, Color.white, new Vector2(0, 250), new Vector2(600, 80));

        musicVolumeSlider = MakeSlider(t, "Musica", new Vector2(0, 80));
        sfxVolumeSlider = MakeSlider(t, "Efeitos", new Vector2(0, 0));

        backButton = MakeButton(t, "VOLTAR", 32, new Vector2(0, -330), new Vector2(240, 60), new Color(0.25f, 0.3f, 0.4f, 1f));
    }

    private Text AddLabel(Transform parent, string text, int size, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var txt = rt.gameObject.AddComponent<Text>();
        txt.font = GetFont();
        txt.text = text;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;
        return txt;
    }

    private Button MakeButton(Transform parent, string label, int fontSize, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
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

        var labelRt = CreateRect("Label", rt);
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        var txt = labelRt.gameObject.AddComponent<Text>();
        txt.font = GetFont();
        txt.text = label;
        txt.fontSize = fontSize;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        return btn;
    }

    private Slider MakeSlider(Transform parent, string label, Vector2 anchoredPos)
    {
        AddLabel(parent, label, 26, Color.white, anchoredPos + new Vector2(-210, 0), new Vector2(320, 30));

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
        frt.anchorMax = new Vector2(1f, 0.5f);
        frt.pivot = new Vector2(0.5f, 0.5f);
        frt.anchoredPosition = Vector2.zero;
        frt.sizeDelta = new Vector2(0f, 10f);
        fill.sprite = GetWhiteSprite();
        fill.color = new Color(1f, 1f, 1f, 1f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;
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
        if (_whiteSprite == null)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }
        return _whiteSprite;
    }

    private static Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    private static void SetPanel(GameObject panel, bool show)
    {
        if (panel != null) panel.SetActive(show);
    }
}