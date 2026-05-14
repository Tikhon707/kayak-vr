using UnityEngine;
using TMPro;

public class BoatDashboard : MonoBehaviour
{
    [Header("Race settings")]
    [Tooltip("Initial time seconds")]
    [SerializeField] private float timeLimitInSeconds = 30f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI checkpointText;
    [SerializeField] private TextMeshProUGUI speedometerText;

    [Header("Physics")]
    [SerializeField] private Rigidbody kayakRB;

    [Header("Settings")]
    [SerializeField] private float speedUpdateInterval = 1.0f;

    private float _currentTime;
    private bool _isTimerRunning = false;
    private float _currentSpeedTimer;

    public float CurrentTime => _currentTime;

    // ─────────────────────────────────────────────
    // Unity Events
    // ─────────────────────────────────────────────

    private void Start()
    {
        _currentTime = 0;
        UpdateTimerUI(_currentTime);
        //UpdateCheckpointsUI(0, 0);
    }

    private void Update()
    {
        UpdateSpeedMeter();

        if (!_isTimerRunning) return;

        _currentTime += Time.deltaTime;
        UpdateTimerUI(_currentTime);
    }

    // ─────────────────────────────────────────────
    // Public Methods (for MenuManager and CheckpointManager)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Start timer (resets to initial)
    /// </summary>
    public void StartTimer()
    {
        _currentTime = 0;
        _isTimerRunning = true;
        UpdateTimerUI(_currentTime);
    }

    /// <summary>
    /// Update timer
    /// </summary>
    public void StopTimer()
    {
        _isTimerRunning = false;
    }

    /// <summary>
    /// Add time (after checkpoint)
    /// </summary>
    public void AddTime(float seconds)
    {
        _currentTime += seconds;
    }

    /// <summary>
    /// Update checkpoint UI
    /// </summary>
    public void UpdateCheckpointsUI(int current, int total)
    {
        if (checkpointText != null)
            checkpointText.text = $"{current} / {total}";
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText == null) return;
        System.TimeSpan t = System.TimeSpan.FromSeconds(time);
        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
    }

    private void UpdateSpeedMeter()
    {
        if (kayakRB == null || speedometerText == null) return;

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