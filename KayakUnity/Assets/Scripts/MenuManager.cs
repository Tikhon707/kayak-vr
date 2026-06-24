using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")] [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI penaltyInfoText;

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
        //RaceManager.OnRaceFinished += ShowVictory;
        TimeAttackManager.OnRaceFinished += ShowVictory;
        SmoothnessControlManager.OnRaceFinished += ShowVictorySmoothness;
        AdrenalineManager.OnBombExploded += ShowGameOver;
    }

    private void OnDisable()
    {
        RaceManager.OnRaceStarted -= OnStartGameButton;
        //RaceManager.OnRaceFinished -= ShowVictory;
        TimeAttackManager.OnRaceFinished -= ShowVictory;
        SmoothnessControlManager.OnRaceFinished -= ShowVictorySmoothness;
        AdrenalineManager.OnBombExploded -= ShowGameOver;
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowVictory(float finalTime, int missedCount, float penaltyTime)
    {
        HideAllPanels();
        victoryPanel.SetActive(true);

        if (victoryTimeText != null)
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(finalTime);
            victoryTimeText.text = string.Format("TIME: {0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        if (penaltyInfoText != null)
        {
            if (missedCount > 0)
            {
                penaltyInfoText.gameObject.SetActive(true);
                penaltyInfoText.text = $"MISSED CHECKPOINTS: {missedCount}\nPENALTY: +{penaltyTime} SEC";
                penaltyInfoText.color = Color.red;
            }
            else
            {
                penaltyInfoText.gameObject.SetActive(true);
                penaltyInfoText.text = "PERFECT RUN! NO PENALTIES";
                penaltyInfoText.color = Color.green;
            }
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (PlayerProfile.HasName)
            LeaderboardService.AddRecord(sceneName, PlayerProfile.CurrentName, finalTime);

        if (leaderboardUI != null)
            leaderboardUI.Show(sceneName, PlayerProfile.CurrentName);
        else
            Debug.LogWarning("[MenuManager] leaderboardUI is not assigned in inspector");
    }

    public void ShowVictorySmoothness(float scores, int missedCount)
    {
        HideAllPanels();
        victoryPanel.SetActive(true);

        if (victoryTimeText != null)
        {
            victoryTimeText.text = string.Format("SCORE: " + scores);
        }

        if (penaltyInfoText != null)
        {
            if (missedCount > 0)
            {
                penaltyInfoText.gameObject.SetActive(true);
                penaltyInfoText.text = $"MISSED CUBES: {missedCount}";
                penaltyInfoText.color = Color.red;
            }
            else
            {
                penaltyInfoText.gameObject.SetActive(true);
                penaltyInfoText.text = "PERFECT RUN! NO PENALTIES";
                penaltyInfoText.color = Color.green;
            }
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (PlayerProfile.HasName)
            LeaderboardService.AddRecord(sceneName, PlayerProfile.CurrentName, scores);

        if (leaderboardUI != null)
            leaderboardUI.Show(sceneName, PlayerProfile.CurrentName);
        else
            Debug.LogWarning("[MenuManager] leaderboardUI is not assigned in inspector");
    }

    public void ShowGameOver()
    {
        HideAllPanels();

        if (RaceManager.Instance)
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