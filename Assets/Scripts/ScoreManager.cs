using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public Text scoreText;
    public TextMeshProUGUI scoreTextTMP;
    private int score = 0;
    private int highScore = 0;

    [Header("Display Format")]
    [SerializeField] private string scorePrefix = "Coins: ";
    [SerializeField] private bool showHighScore = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHighScore()
    {
        return highScore;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    public void SaveHighScoreIfNeeded()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// 重新从当前场景查找并绑定计分文本（用于跨场景后旧引用失效的情况）
    /// </summary>
    public void BindScoreTextFromScene()
    {
        Canvas canvas = null;
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.name == "PauseCanvas") continue;
            canvas = c;
            break;
        }
        if (canvas == null) return;

        Text[] allTexts = canvas.GetComponentsInChildren<Text>(true);
        TextMeshProUGUI[] allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);

        scoreText = null;
        scoreTextTMP = null;

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
                    scoreTextTMP = t;
                    break;
                }
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        string display = showHighScore && highScore > 0
            ? $"{scorePrefix}{score}  (Best: {highScore})"
            : scorePrefix + score;

        if (scoreText != null)
            scoreText.text = display;
        if (scoreTextTMP != null)
            scoreTextTMP.text = display;
    }
}
