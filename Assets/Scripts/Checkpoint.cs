using UnityEngine;

/// <summary>
/// Вешается на каждую арку. IsTrigger должен быть включён на коллайдере.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Это финишная арка?")]
    [SerializeField] private bool isFinish = false;

    [Tooltip("Сколько секунд добавляется за прохождение (не для финиша)")]
    [SerializeField] private float bonusTime = 15f;

    private bool _passed = false;

    /// <summary>
    /// Сбросить состояние (вызывается при рестарте)
    /// </summary>
    public void ResetCheckpoint()
    {
        _passed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Срабатывает только один раз и только на каяк (поставь тег "Player" на каяк)
        if (_passed) return;
        if (!other.CompareTag("Player")) return;

        _passed = true;

        if (isFinish)
        {
            CheckpointManager.Instance.OnFinish();
        }
        else
        {
            CheckpointManager.Instance.OnCheckpointPassed(bonusTime);
        }
    }
}