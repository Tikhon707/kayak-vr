using System;
using System.Text;
using TMPro;
using UnityEngine;

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
    [SerializeField] private float speedUpdateInterval = 2.0f;
    // ADDED: Throttle UI updates to save massive CPU cycles at 72/90 FPS
    [SerializeField] private float uiRefreshInterval = 0.05f;

    private StringBuilder _timerBuilder = new StringBuilder(16);
    private StringBuilder _ghostTimerBuilder = new StringBuilder(16);

    private float _currentSpeedTimer;
    private float _currentUiTimer; // ADDED: Timer to control general UI refresh rate
    private float _lastRenderedTime = -1f; // ADDED: Cache to prevent re-rendering identical time values

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
        // EDITED: Combined UI updates into a throttled system to keep stable FPS on Standalone VR
        float deltaTime = Time.deltaTime;
        _currentSpeedTimer += deltaTime;
        _currentUiTimer += deltaTime;

        if (_currentSpeedTimer >= speedUpdateInterval)
        {
            _currentSpeedTimer = 0f;
            UpdateSpeedMeter();
        }

        if (_currentUiTimer >= uiRefreshInterval)
        {
            _currentUiTimer = 0f;

            if (RaceManager.Instance != null && RaceManager.Instance.currentState == RaceManager.RaceState.Racing)
            {
                UpdateTimerUI(RaceManager.Instance.CurrentRaceTime);
            }
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

        // EDITED: Avoid optimization overhead if the time hasn't changed significantly since last frame
        if (Mathf.Abs(time - _lastRenderedTime) < 0.001f) return;
        _lastRenderedTime = time;

        TimeSpan t = TimeSpan.FromSeconds(time);
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

        // EDITED: Only modify color if it actually changes to prevent internal graphic component dirtying
        Color targetColor = resultTime > 0 ? Color.red : Color.green;
        if (ghostDifferTimer.color != targetColor)
        {
            ghostDifferTimer.color = targetColor;
        }
    }

    private void UpdateSpeedMeter()
    {
        if (!kayakRB || !speedometerText) return;

        float sqrSpeed = kayakRB.linearVelocity.sqrMagnitude;
        if (sqrSpeed < 0.1f)
        {
            // EDITED: Avoid string allocation by using a pre-allocated literal or TMP internal buffer
            speedometerText.SetText("0 km/h");
            return;
        }

        float realSpeed = Mathf.Sqrt(sqrSpeed) * 3.6f;
        // EDITED: Exploit TextMeshPro's zero-allocation native formatting method.
        // This completely eliminates GC allocation leaks without needing a custom StringBuilder setup.
        speedometerText.SetText("{0:0} km/h", realSpeed);
    }
}