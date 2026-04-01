using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private bool isFinish = false;
    [SerializeField] private float bonusTime = 15f;

    private bool _passed = false;

    public void ResetCheckpoint() => _passed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_passed) return;
        if (!other.CompareTag("Player")) return;

        _passed = true;
        if (isFinish) CheckpointManager.Instance.OnFinish();
        else CheckpointManager.Instance.OnCheckpointPassed(bonusTime);
    }
}