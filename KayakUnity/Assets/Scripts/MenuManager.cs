using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Ссылки")]
    [SerializeField] private BoatDashboard dashboard;

    [Header("Панели")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Текст итогов")]
    [SerializeField] private TextMeshProUGUI victoryTimeText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => ShowMainMenu();

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowVictory(float finalTime)
    {
        HideAllPanels();
        if (dashboard != null) dashboard.StopTimer();
        victoryPanel.SetActive(true);

        if (victoryTimeText != null)
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(finalTime);
            victoryTimeText.text = string.Format("TIME: {0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }
    }

    public void ShowGameOver()
    {
        HideAllPanels();
        if (dashboard) dashboard.StopTimer();
        gameOverPanel.SetActive(true);
    }

    public void OnStartGameButton()
    {
        HideAllPanels();
        if (dashboard != null) dashboard.StartTimer();
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