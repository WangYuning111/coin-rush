using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Text component for countdown display (choose one)")]
    [SerializeField] private Text uiText;

    [Tooltip("TextMeshPro component for countdown display (choose one)")]
    [SerializeField] private TextMeshProUGUI tmpText;

    [Header("Display Format")]
    [Tooltip("Countdown text format, {0} is remaining seconds")]
    [SerializeField] private string displayFormat = "Time: {0:F1}s";

    [Header("Color Warning")]
    [Tooltip("Color when time is sufficient")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Color when time is running low (less than 5 seconds)")]
    [SerializeField] private Color warningColor = Color.red;

    [Tooltip("Warning threshold (seconds)")]
    [SerializeField] private float warningThreshold = 5f;

    private void Start()
    {
        if (uiText == null && tmpText == null)
        {
            uiText = GetComponent<Text>();
            tmpText = GetComponent<TextMeshProUGUI>();
        }

        if (GameManager.Instance != null)
        {
            UpdateTimeDisplay(GameManager.Instance.GetCountdownDuration());
        }
    }

    public void UpdateTimeDisplay(float time)
    {
        string timeText = string.Format(displayFormat, Mathf.Max(0, time));

        Color displayColor = normalColor;
        if (time <= warningThreshold)
        {
            float flash = Mathf.PingPong(Time.time * 4f, 1f);
            displayColor = Color.Lerp(normalColor, warningColor, flash);
        }

        if (tmpText != null)
        {
            tmpText.text = timeText;
            tmpText.color = displayColor;
        }
        else if (uiText != null)
        {
            uiText.text = timeText;
            uiText.color = displayColor;
        }
    }

    public void ShowGameOverText(string message)
    {
        if (tmpText != null)
        {
            tmpText.text = message;
        }
        else if (uiText != null)
        {
            uiText.text = message;
        }
    }
}
