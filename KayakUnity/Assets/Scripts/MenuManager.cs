using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private TextMeshProUGUI victoryTimeText;

    [SerializeField] private LeaderboardUI leaderboardUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => ShowMainMenu();

    private void OnEnable()
    {
        RaceManager.OnRaceStarted += OnStartGameButton;
    }

    private void OnDisable()
    {
        RaceManager.OnRaceStarted -= OnStartGameButton;
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowVictory(float finalTime)
    {
        HideAllPanels();
        victoryPanel.SetActive(true);

        if (victoryTimeText != null)
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(finalTime);
            victoryTimeText.text = string.Format("TIME: {0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        Debug.Log($"[MenuManager] ShowVictory finalTime={finalTime}, leaderboardUI={(leaderboardUI == null ? "NULL" : leaderboardUI.name)}, currentName='{PlayerProfile.CurrentName}'");
        if (leaderboardUI != null)
        {
            leaderboardUI.Show(SceneManager.GetActiveScene().name, PlayerProfile.CurrentName);
        }
        else
        {
            Debug.LogWarning("[MenuManager] leaderboardUI is not assigned in inspector");
        }
    }

    public void ShowGameOver()
    {
        HideAllPanels();

        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.currentState = RaceManager.RaceState.Finished;
        }

        gameOverPanel.SetActive(true);
    }

    public void OnStartGameButton()
    {
        //HideAllPanels();
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnPauseButton()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnResumeButton()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnQuitButton() => Application.Quit();

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        victoryPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
    }
}