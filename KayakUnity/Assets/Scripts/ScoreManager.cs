using UnityEngine;

public interface IScoreManager
{
    int GetMissedScores();
    
    float GetScore(float finishTime);
}
