using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelPauseMenu : MonoBehaviour
{
    private GameObject pauseCanvasObj;
    private GameObject pausePanel;
    private bool isPaused = false;

    void Start()
    {
        CreatePauseUI();
        HidePauseMenu();
    }

    void Update()
    {
        // Escape 键由 GameManager 统一处理，避免双重触发
        // 如果场景中无 GameManager（如直接从编辑器运行且无 RuntimeInitializer），则自行处理
        if (GameManager.Instance == null && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void CreatePauseUI()
    {
        if (pausePanel != null || pauseCanvasObj != null) return;

        pauseCanvasObj = new GameObject("PauseCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        Canvas pauseCanvas = pauseCanvasObj.GetComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 100;
        pauseCanvasObj.GetComponent<GraphicRaycaster>().blockingObjects = GraphicRaycaster.BlockingObjects.None;

        CanvasScaler scaler = pauseCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        pausePanel = new GameObject("PausePanel", typeof(RectTransform));
        pausePanel.transform.SetParent(pauseCanvasObj.transform, false);
        RectTransform panelRt = pausePanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

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

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.selectedColor = new Color(0.7f, 0.9f, 1f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

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
        btnText.raycastTarget = false;
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
    }

    public void TogglePause()
    {
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
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
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
