using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToMenuButton : MonoBehaviour
{
    [Header("Main Menu Scene")]
    [Tooltip("Scene name to return to when button is clicked")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Optional: Specify Button (if not specified, will auto-get Button on current object)")]
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackToMenu);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Button component not found, please attach this script to a Button or assign one in Inspector");
        }
    }

    private void OnBackToMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name is empty! Please set main menu scene name in Inspector.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ReturnToMainMenu()
    {
        OnBackToMenu();
    }

    private void OnDestroy()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackToMenu);
        }
    }
}
