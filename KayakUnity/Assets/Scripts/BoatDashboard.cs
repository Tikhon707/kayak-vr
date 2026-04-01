using UnityEngine;
using TMPro;

public class BoatDashboard : MonoBehaviour
{
    [Header("Настройки Гонки")]
    [Tooltip("Начальное время в секундах")]
    [SerializeField] private float timeLimitInSeconds = 30f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI checkpointText;
    [SerializeField] private TextMeshProUGUI speedometerText;

    [Header("Физика")]
    [SerializeField] private Rigidbody kayakRB;

    [Header("Settings")]
    [SerializeField] private float speedUpdateInterval = 1.0f;

    // Приватные поля
    private float _currentTime;
    private bool _isTimerRunning = false;
    private float _currentSpeedTimer;

    // Публичное свойство — текущее оставшееся время (для ShowVictory)
    public float CurrentTime => _currentTime;

    // ─────────────────────────────────────────────
    // Unity Events
    // ─────────────────────────────────────────────

    private void Start()
    {
        _currentTime = timeLimitInSeconds;
        UpdateTimerUI(_currentTime);
        UpdateCheckpointsUI(0, 0);
    }

    private void Update()
    {
        UpdateSpeedMeter();

        if (!_isTimerRunning) return;

        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0)
        {
            _currentTime = 0;
            _isTimerRunning = false;
            UpdateTimerUI(_currentTime);
            MenuManager.Instance.ShowGameOver();
            return;
        }

        UpdateTimerUI(_currentTime);
    }

    // ─────────────────────────────────────────────
    // Публичные методы (для MenuManager и CheckpointManager)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Запустить таймер (сбрасывает на начальное значение)
    /// </summary>
    public void StartTimer()
    {
        _currentTime = timeLimitInSeconds;
        _isTimerRunning = true;
        UpdateTimerUI(_currentTime);
    }

    /// <summary>
    /// Остановить таймер
    /// </summary>
    public void StopTimer()
    {
        _isTimerRunning = false;
    }

    /// <summary>
    /// Добавить секунды к таймеру (при прохождении арки)
    /// </summary>
    public void AddTime(float seconds)
    {
        _currentTime += seconds;
    }

    /// <summary>
    /// Обновить UI чекпоинтов
    /// </summary>
    public void UpdateCheckpointsUI(int current, int total)
    {
        if (checkpointText != null)
            checkpointText.text = $"{current} / {total}";
    }

    // ─────────────────────────────────────────────
    // Приватные методы
    // ─────────────────────────────────────────────

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