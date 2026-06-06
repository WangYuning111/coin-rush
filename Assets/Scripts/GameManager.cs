using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [Tooltip("Success scene name")]
    [SerializeField] private string successSceneName = "Success";

    [Tooltip("Game over scene name")]
    [SerializeField] private string gameOverSceneName = "Gameover";

    [Header("Countdown Settings")]
    [Tooltip("Countdown duration (seconds)")]
    [SerializeField] private float countdownDuration = 15f;

    [Tooltip("Minimum score required to pass level")]
    [SerializeField] private int passScoreThreshold = 10;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isGameOver = false;

    private CountdownUI countdownUI;
    private string lastLevelSceneName;

    public float CurrentTime => currentTime;
    public bool IsTimerRunning => isTimerRunning;
    public bool IsGameOver => isGameOver;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 优先使用场景内独立的 LevelPauseMenu（Level1/Level2）
            LevelPauseMenu levelPause = FindObjectOfType<LevelPauseMenu>();
            if (levelPause != null)
            {
                levelPause.TogglePause();
            }
            else if (PauseMenu.Instance != null)
            {
                PauseMenu.Instance.TogglePause();
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        countdownUI = FindObjectOfType<CountdownUI>();

        if (IsLevelScene(scene.name))
        {
            lastLevelSceneName = scene.name;
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ResetScore();
            StartCountdown();
            LayoutTopHUD();
            AudioManager.Instance?.PlayGameplayMusic();
        }
        else if (scene.name == successSceneName || scene.name == gameOverSceneName)
        {
            StopCountdown();
            LayoutResultScene(scene.name);
            AudioManager.Instance?.StopMusic();
        }
        else if (scene.name == "Main Menu")
        {
            StopCountdown();
            AudioManager.Instance?.PlayMenuMusic();
            // 强制恢复光标和 timeScale，防止从 pause 返回后异常
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            StopCountdown();
        }

        // 备用：确保 ScoreManager 的计分文本引用已绑定到当前场景
        ScoreManager.Instance?.BindScoreTextFromScene();
    }

    private bool IsLevelScene(string sceneName)
    {
        return sceneName != "Main Menu" &&
               sceneName != successSceneName &&
               sceneName != gameOverSceneName;
    }

    public void StartCountdown()
    {
        currentTime = countdownDuration;
        isTimerRunning = true;
        isGameOver = false;
        StartCoroutine(CountdownCoroutine());
    }

    public void StopCountdown()
    {
        isTimerRunning = false;
        StopAllCoroutines();
    }

    private IEnumerator CountdownCoroutine()
    {
        while (currentTime > 0 && isTimerRunning)
        {
            currentTime -= Time.deltaTime;

            if (countdownUI != null)
            {
                countdownUI.UpdateTimeDisplay(currentTime);
            }

            yield return null;
        }

        if (isTimerRunning)
        {
            currentTime = 0;
            if (countdownUI != null)
            {
                countdownUI.UpdateTimeDisplay(0);
            }

            OnCountdownFinished();
        }
    }

    private void OnCountdownFinished()
    {
        isTimerRunning = false;
        isGameOver = true;

        int currentScore = 0;
        if (ScoreManager.Instance != null)
        {
            currentScore = ScoreManager.Instance.GetScore();
            ScoreManager.Instance.SaveHighScoreIfNeeded();
        }
        else
        {
            Debug.LogWarning("ScoreManager not found, score set to 0");
        }

        if (currentScore >= passScoreThreshold)
        {
            AudioManager.Instance?.PlayLevelSuccess();
            SceneManager.LoadScene(successSceneName);
        }
        else
        {
            AudioManager.Instance?.PlayLevelFail();
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    public void EndGameEarly()
    {
        if (isTimerRunning)
        {
            StopCountdown();
            OnCountdownFinished();
        }
    }

    public float GetCountdownDuration()
    {
        return countdownDuration;
    }

    public void SetCountdownDuration(float duration)
    {
        countdownDuration = duration;
    }

    #region Level HUD Layout

    private void LayoutTopHUD()
    {
        Canvas canvas = null;
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.name == "PauseCanvas") continue;
            canvas = c;
            break;
        }

        if (canvas == null)
        {
            Debug.LogWarning("[GameManager] Canvas not found for HUD layout");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            Debug.Log("[GameManager] CanvasScaler set to ScaleWithScreenSize 1920x1080");
        }

        Text[] allTexts = canvas.GetComponentsInChildren<Text>(true);
        TextMeshProUGUI[] allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);

        Text scoreText = null;
        TextMeshProUGUI scoreTmp = null;
        Text timerText = null;
        TextMeshProUGUI timerTmp = null;
        Button backButton = null;

        foreach (Text t in allTexts)
        {
            string lower = t.text.ToLower();
            if (lower.Contains("coin") || lower.Contains("score"))
            {
                scoreText = t;
                break;
            }
        }

        if (scoreText == null)
        {
            foreach (TextMeshProUGUI t in allTmps)
            {
                string lower = t.text.ToLower();
                if (lower.Contains("coin") || lower.Contains("score"))
                {
                    scoreTmp = t;
                    break;
                }
            }
        }

        foreach (Text t in allTexts)
        {
            if (t.GetComponent<CountdownUI>() != null)
            {
                timerText = t;
                break;
            }
        }

        if (timerText == null)
        {
            foreach (TextMeshProUGUI t in allTmps)
            {
                if (t.GetComponent<CountdownUI>() != null)
                {
                    timerTmp = t;
                    break;
                }
            }
        }

        foreach (Button b in allButtons)
        {
            Text btnText = b.GetComponentInChildren<Text>(true);
            TextMeshProUGUI btnTmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
            string label = "";
            if (btnText != null) label = btnText.text;
            else if (btnTmp != null) label = btnTmp.text;

            if (label.ToLower().Contains("back") || label.ToLower().Contains("menu"))
            {
                backButton = b;
                break;
            }
        }

        float topOffset = 40f;
        float sideOffset = 40f;

        if (scoreText != null)
        {
            SetTopLeft(scoreText.rectTransform, sideOffset, topOffset, new Vector2(400, 80));
            scoreText.fontSize = 48;
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.UpperLeft;
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.scoreText = scoreText;
        }
        else if (scoreTmp != null)
        {
            SetTopLeft(scoreTmp.rectTransform, sideOffset, topOffset, new Vector2(400, 80));
            scoreTmp.fontSize = 48;
            scoreTmp.color = Color.white;
            scoreTmp.alignment = TextAlignmentOptions.TopLeft;
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.scoreTextTMP = scoreTmp;
        }

        if (timerText != null)
        {
            SetTopCenter(timerText.rectTransform, topOffset, new Vector2(500, 80));
            timerText.fontSize = 48;
            timerText.color = Color.white;
            timerText.alignment = TextAnchor.UpperCenter;
            Debug.Log($"[GameManager] Timer text repositioned: {timerText.name}");
        }
        else if (timerTmp != null)
        {
            SetTopCenter(timerTmp.rectTransform, topOffset, new Vector2(500, 80));
            timerTmp.fontSize = 48;
            timerTmp.color = Color.white;
            timerTmp.alignment = TextAlignmentOptions.Top;
            Debug.Log($"[GameManager] Timer TMP repositioned: {timerTmp.name}");
        }

        if (backButton != null)
        {
            SetTopRight(backButton.GetComponent<RectTransform>(), sideOffset, topOffset, new Vector2(200, 80));
            Text btnText = backButton.GetComponentInChildren<Text>(true);
            TextMeshProUGUI btnTmp = backButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (btnText != null)
            {
                btnText.fontSize = 36;
                btnText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            else if (btnTmp != null)
            {
                btnTmp.fontSize = 36;
                btnTmp.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                Debug.Log("[GameManager] Back button clicked - loading Main Menu");
                SceneManager.LoadScene("Main Menu");
            });

            Debug.Log($"[GameManager] Back button repositioned: {backButton.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Back button not found on Canvas");
        }
    }

    private void SetTopLeft(RectTransform rt, float x, float y, Vector2 size)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = size;
    }

    private void SetTopCenter(RectTransform rt, float y, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -y);
        rt.sizeDelta = size;
    }

    private void SetTopRight(RectTransform rt, float x, float y, Vector2 size)
    {
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-x, -y);
        rt.sizeDelta = size;
    }

    private void SetCenter(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    #endregion

    #region Result Scene Layout

    private void LayoutResultScene(string sceneName)
    {
        // 自动解锁鼠标并显示光标，确保按钮可点击
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[GameManager] Cursor unlocked for result scene");

        Canvas canvas = null;
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.name == "PauseCanvas") continue;
            canvas = c;
            break;
        }

        if (canvas == null)
        {
            Debug.LogWarning("[GameManager] Canvas not found for result scene layout");
            return;
        }

        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);

        Button backButton = null;
        foreach (Button b in allButtons)
        {
            Text btnText = b.GetComponentInChildren<Text>(true);
            TextMeshProUGUI btnTmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
            string label = "";
            if (btnText != null) label = btnText.text;
            else if (btnTmp != null) label = btnTmp.text;

            if (label.ToLower().Contains("back") || label.ToLower().Contains("menu"))
            {
                backButton = b;
                break;
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                Debug.Log("[GameManager] Back button clicked - loading Main Menu");
                SceneManager.LoadScene("Main Menu");
            });

            Debug.Log($"[GameManager] Back button event bound: {backButton.name}");

            if (sceneName == successSceneName)
            {
                CreateNextLevelButton(backButton);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Back button not found on result Canvas");
        }
    }

    private void CreateNextLevelButton(Button templateButton)
    {
        string nextLevel = GetNextLevel(lastLevelSceneName);
        if (string.IsNullOrEmpty(nextLevel))
        {
            Debug.Log("[GameManager] No next level available");
            return;
        }

        Button nextButton = Instantiate(templateButton, templateButton.transform.parent);
        nextButton.name = "NextLevelButton";
        RectTransform nextRt = nextButton.GetComponent<RectTransform>();
        SetCenter(nextRt, new Vector2(400, -120), new Vector2(160, 30));
        nextRt.localScale = new Vector3(3.1764631f, 3.9684f, 1f);

        Text nextBtnText = nextButton.GetComponentInChildren<Text>(true);
        TextMeshProUGUI nextBtnTmp = nextButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (nextBtnText != null)
        {
            nextBtnText.text = "Next Level";
            nextBtnText.fontSize = 24;
            nextBtnText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }
        else if (nextBtnTmp != null)
        {
            nextBtnTmp.text = "Next Level";
            nextBtnTmp.fontSize = 24;
            nextBtnTmp.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            Debug.Log($"[GameManager] Next Level clicked - loading {nextLevel}");
            SceneManager.LoadScene(nextLevel);
        });

        Debug.Log($"[GameManager] Next Level button created for: {nextLevel}");
    }

    private string GetNextLevel(string currentLevel)
    {
        if (currentLevel == "Level1") return "Level2";
        if (currentLevel == "Level2") return "Level3";
        if (currentLevel == "Level3") return "Level4";
        return null;
    }

    #endregion
}
