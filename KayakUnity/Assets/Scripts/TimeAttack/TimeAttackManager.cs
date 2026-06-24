using System;
using UnityEngine;

public class TimeAttackManager : BaseScoreManager
{
    public static event Action<float, int, float> OnRaceFinished;

    [Header("Penalty Settings")] [SerializeField]
    private float penaltyPerMissedCheckpoint = 5f;

    public override void Finish(float time)
    {
        var missedCheckpoints = CheckpointManager.Instance.GetMissedCheckpointsCount();
        var penaltyTime = missedCheckpoints * penaltyPerMissedCheckpoint;
        var finalTotalTime = penaltyTime + time;
        if (GhostManager.Instance != null && GhostManager.Instance.enabled)
        {
            GhostManager.Instance.StopGhostSystem(finalTotalTime);
        }

        OnRaceFinished?.Invoke(finalTotalTime, missedCheckpoints, penaltyTime);
    }
}