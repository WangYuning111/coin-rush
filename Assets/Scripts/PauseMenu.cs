using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    private GameObject pauseCanvasObj;
    private GameObject pausePanel;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CreatePauseUI();
        HidePauseMenu();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f;

        // 显式清理旧的 Pause UI，防止跨场景引用失效或遮挡
        if (pauseCanvasObj != null)
        {
            Destroy(pauseCanvasObj);
            pauseCanvasObj = null;
        }
        pausePanel = null;

        // 只在关卡场景创建 Pause UI，避免在 Success/Gameover/Main Menu 中创建多余的 Canvas
        string sceneName = scene.name;
        if (sceneName != "Main Menu" && sceneName != "Success" && sceneName != "Gameover")
        {
            CreatePauseUI();
            HidePauseMenu();
        }
    }

    private void CreatePauseUI()
    {
        if (pausePanel != null || pauseCanvasObj != null) return;

        // 创建独立的 PauseCanvas，确保 SortingOrder 最高，不会被其他 UI 遮挡或拦截点击
        pauseCanvasObj = new GameObject("PauseCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        Canvas pauseCanvas = pauseCanvasObj.GetComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 100;
        pauseCanvasObj.GetComponent<GraphicRaycaster>().blockingObjects = GraphicRaycaster.BlockingObjects.None;

        // 添加 CanvasScaler，与关卡 UI 保持一致的缩放模式
        CanvasScaler scaler = pauseCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 确保场景中存在 EventSystem
        EnsureEventSystem();

        pausePanel = new GameObject("PausePanel", typeof(RectTransform));
        pausePanel.transform.SetParent(pauseCanvasObj.transform, false);
        RectTransform panelRt = pausePanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // 背景半透明遮罩，关闭 raycastTarget 避免阻挡子按钮的射线检测
        Image bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);
        bg.raycastTarget = false;

        GameObject titleObj = new GameObject("PauseTitle", typeof(RectTransform));
        titleObj.transform.SetParent(pausePanel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSED";
        titleText.fontSize = 72;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0, 120);
        titleRt.sizeDelta = new Vector2(600, 100);

        CreateButton("ResumeButton", "Resume", new Vector2(0, 20), () => TogglePause());
        CreateButton("RestartButton", "Restart", new Vector2(0, -60), () => RestartLevel());
        CreateButton("MainMenuButton", "Main Menu", new Vector2(0, -140), () => LoadMainMenu());
    }

    private void EnsureEventSystem()
    {
        var existing = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (existing == null)
        {
            GameObject eventSystem = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            eventSystem.SetActive(true);
        }
        else if (!existing.gameObject.activeInHierarchy)
        {
            existing.gameObject.SetActive(true);
        }
    }

    private void CreateButton(string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(pausePanel.transform, false);

        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        btn.targetGraphic = btnImg;
        btn.interactable = true;

        // 设置明显的颜色过渡，让用户有视觉反馈
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.selectedColor = new Color(0.7f, 0.9f, 1f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        // 禁用键盘导航，防止导航干扰鼠标点击
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = 36;
        btnText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.raycastTarget = false; // 让按钮的 Image 负责接收射线，避免文本干扰
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = pos;
        btnRt.sizeDelta = new Vector2(300, 60);

        btn.onClick.AddListener(onClick);
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[PauseMenu] Button '{name}' clicked");
            AudioManager.Instance?.PlayButtonClick();
        });
    }

    public void TogglePause()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Main Menu" || currentScene == "Success" || currentScene == "Gameover")
            return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }

    private void HidePauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}
