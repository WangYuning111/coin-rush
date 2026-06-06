using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button levelsButton;
    [SerializeField] private Button tipsButton;
    [SerializeField] private Button quitButton;

    [Header("Level Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [SerializeField] private Button level4Button;

    [Header("Scene Names - Modify here")]
    [SerializeField] private string level1SceneName = "Level1";
    [SerializeField] private string level2SceneName = "Level2";
    [SerializeField] private string level3SceneName = "Level3";
    [SerializeField] private string level4SceneName = "Level4";

    [Header("Tips Text")]
    [SerializeField] private GameObject tipsTextObject;

    [Header("Level Buttons Container")]
    [SerializeField] private GameObject levelButtonsContainer;

    private bool areLevelButtonsVisible = false;
    private bool isTipsVisible = false;

    private void Start()
    {
        // 强制恢复游戏状态，防止从关卡返回后 timeScale 或光标异常
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BindButtonEvents();

        if (levelButtonsContainer != null)
            levelButtonsContainer.SetActive(false);

        if (tipsTextObject != null)
            tipsTextObject.SetActive(false);
    }

    private void BindButtonEvents()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (levelsButton != null)
            levelsButton.onClick.AddListener(OnToggleLevelButtons);

        if (tipsButton != null)
            tipsButton.onClick.AddListener(OnToggleTips);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);

        if (level1Button != null)
            level1Button.onClick.AddListener(() => LoadScene(level1SceneName));

        if (level2Button != null)
            level2Button.onClick.AddListener(() => LoadScene(level2SceneName));

        if (level3Button != null)
            level3Button.onClick.AddListener(() => LoadScene(level3SceneName));

        if (level4Button != null)
            level4Button.onClick.AddListener(() => LoadScene(level4SceneName));
    }

    private void OnStartGame()
    {
        LoadScene(level1SceneName);
    }

    private void OnToggleLevelButtons()
    {
        areLevelButtonsVisible = !areLevelButtonsVisible;
        if (levelButtonsContainer != null)
            levelButtonsContainer.SetActive(areLevelButtonsVisible);
    }

    private void OnToggleTips()
    {
        isTipsVisible = !isTipsVisible;
        if (tipsTextObject != null)
            tipsTextObject.SetActive(isTipsVisible);
    }

    private void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please set it in the Inspector.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    [ContextMenu("Auto-find button references")]
    private void AutoFindReferences()
    {
        Debug.Log("Please manually drag and drop button references in Inspector");
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartGame);

        if (levelsButton != null)
            levelsButton.onClick.RemoveListener(OnToggleLevelButtons);

        if (tipsButton != null)
            tipsButton.onClick.RemoveListener(OnToggleTips);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitGame);

        if (level1Button != null)
            level1Button.onClick.RemoveAllListeners();

        if (level2Button != null)
            level2Button.onClick.RemoveAllListeners();

        if (level3Button != null)
            level3Button.onClick.RemoveAllListeners();

        if (level4Button != null)
            level4Button.onClick.RemoveAllListeners();
    }
}
