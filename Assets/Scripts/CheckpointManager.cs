using UnityEngine;

/// <summary>
/// Менеджер чекпоинтов. Вешается на пустой GameObject на сцене.
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Ссылки")]
    [SerializeField] private BoatDashboard dashboard;

    [Header("Настройки")]
    [Tooltip("Количество арок БЕЗ финишной")]
    [SerializeField] private int totalCheckpoints = 5;

    private int _passedCount = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Вызывается при прохождении обычной арки
    /// </summary>
    public void OnCheckpointPassed(float bonusTime)
    {
        _passedCount++;
        dashboard.AddTime(bonusTime);
        dashboard.UpdateCheckpointsUI(_passedCount, totalCheckpoints);

        Debug.Log($"[Checkpoint] Пройдена арка {_passedCount}/{totalCheckpoints}, +{bonusTime}с");
    }

    /// <summary>
    /// Вызывается при прохождении финишной арки
    /// </summary>
    public void OnFinish()
    {
        Debug.Log("[Checkpoint] Финиш!");
        MenuManager.Instance.ShowVictory(dashboard.CurrentTime);
    }

    /// <summary>
    /// Сброс всех чекпоинтов (при рестарте)
    /// </summary>
    public void ResetAll()
    {
        _passedCount = 0;
        dashboard.UpdateCheckpointsUI(0, totalCheckpoints);

        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (var cp in checkpoints)
            cp.ResetCheckpoint();
    }
}