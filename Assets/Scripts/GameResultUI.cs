using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameResultUI : MonoBehaviour
{
    [Header("Result Display")]
    [SerializeField] private Text resultText;
    [SerializeField] private TextMeshProUGUI resultTmpText;

    [SerializeField] private Text scoreText;
    [SerializeField] private TextMeshProUGUI scoreTmpText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string level1SceneName = "Level1";

    [Header("Display Text")]
    [SerializeField] private string successTitle = "SUCCESS!";
    [SerializeField] private string gameOverTitle = "GAME OVER";

    private void Start()
    {
        if (resultText == null && resultTmpText == null)
            resultTmpText = GameObject.Find("ResultTitle")?.GetComponent<TextMeshProUGUI>();

        if (scoreText == null && scoreTmpText == null)
            scoreTmpText = GameObject.Find("FinalScore")?.GetComponent<TextMeshProUGUI>();

        DisplayResult();

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnReturnToMainMenu);
    }

    private void DisplayResult()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isSuccess = currentScene.ToLower().Contains("success");

        string title = isSuccess ? successTitle : gameOverTitle;
        SetText(resultText, resultTmpText, title);

        int finalScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.GetScore();
        }
        SetText(scoreText, scoreTmpText, $"Final Score: {finalScore}");
    }

    private void SetText(Text uiText, TextMeshProUGUI tmpText, string content)
    {
        if (tmpText != null)
            tmpText.text = content;
        else if (uiText != null)
            uiText.text = content;
    }

    private void OnRestart()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(level1SceneName);
    }

    private void OnReturnToMainMenu()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnReturnToMainMenu);
    }
}
