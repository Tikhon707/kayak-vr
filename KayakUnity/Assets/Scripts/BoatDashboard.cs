using UnityEngine;
using TMPro;
using System.Text;

public class BoatDashboard : MonoBehaviour
{
    public static BoatDashboard Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI ghostDifferTimer;
    [SerializeField] private TextMeshProUGUI checkpointText;
    [SerializeField] private TextMeshProUGUI speedometerText;

    [Header("Physics")]
    [SerializeField] private Rigidbody kayakRB;

    [Header("Settings")]
    [SerializeField] private float speedUpdateInterval = 1.0f;

    private StringBuilder _timerBuilder = new StringBuilder(16);
    private StringBuilder _ghostTimerBuilder = new StringBuilder(16);

    private float _currentSpeedTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateTimerUI(0f);
    }

    private void Update()
    {
        UpdateSpeedMeter();

        // Fetch time directly from the RaceManager
        if (RaceManager.Instance != null && RaceManager.Instance.currentState == RaceManager.RaceState.Racing)
        {
            UpdateTimerUI(RaceManager.Instance.CurrentRaceTime);
        }
    }

    // ─────────────────────────────────────────────
    // UI Update Methods
    // ─────────────────────────────────────────────

    public void UpdateCheckpointsUI(int current, int total)
    {
        if (checkpointText != null)
            checkpointText.text = $"{current} / {total}";
    }

    private void UpdateTimerUI(float time)
    {
        if (!timerText) return;

        System.TimeSpan t = System.TimeSpan.FromSeconds(time);
        _timerBuilder.Clear();
        _timerBuilder.AppendFormat("{0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);

        timerText.SetText(_timerBuilder);
    }

    public void UpdateGhostDifferenceUI(float ghostTimeAtThisPoint)
    {
        if (!ghostDifferTimer || RaceManager.Instance == null) return;

        var resultTime = RaceManager.Instance.CurrentRaceTime - ghostTimeAtThisPoint;
        float absoluteTime = Mathf.Abs(resultTime);
        System.TimeSpan t = System.TimeSpan.FromSeconds(absoluteTime);

        _ghostTimerBuilder.Clear();

        if (resultTime < 0) _ghostTimerBuilder.Append("-");

        _ghostTimerBuilder.AppendFormat("{0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);

        ghostDifferTimer.SetText(_ghostTimerBuilder);

        if (resultTime > 0)
            ghostDifferTimer.color = Color.red;
        else
            ghostDifferTimer.color = Color.green;
    }

    private void UpdateSpeedMeter()
    {
        if (!kayakRB || !speedometerText) return;

        _currentSpeedTimer += Time.deltaTime;
        if (_currentSpeedTimer < speedUpdateInterval) return;

        _currentSpeedTimer = 0f;

        float sqrSpeed = kayakRB.linearVelocity.sqrMagnitude;
        if (sqrSpeed < 0.1f)
        {
            speedometerText.text = "0 km/h";
            return;
        }

        float realSpeed = Mathf.Sqrt(sqrSpeed) * 3.6f;
        speedometerText.text = $"{realSpeed:F0} km/h";
    }
}