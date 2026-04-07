using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Ссылки")]
    [SerializeField] private BoatDashboard dashboard;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip finishClip;

    [Header("Настройки")]
    [SerializeField] private int totalCheckpoints = 1;

    private int _passedCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnCheckpointPassed(float bonusTime)
    {
        _passedCount++;
        dashboard.AddTime(bonusTime);
        dashboard.UpdateCheckpointsUI(_passedCount, totalCheckpoints);

        if (audioSource && checkpointClip) audioSource.PlayOneShot(checkpointClip);
        Debug.Log($"[Checkpoint] {_passedCount}/{totalCheckpoints}");
    }

    public void OnFinish()
    {
        if (audioSource && finishClip) audioSource.PlayOneShot(finishClip);
        if (GhostManager.Instance != null) GhostManager.Instance.FinishRace(dashboard.CurrentTime);
        MenuManager.Instance.ShowVictory(dashboard.CurrentTime);
    }

    public void ResetAll()
    {
        _passedCount = 0;
        dashboard.UpdateCheckpointsUI(0, totalCheckpoints);
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (var cp in checkpoints) cp.ResetCheckpoint();
    }
}