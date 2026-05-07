using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    public RaceManager raceManager;

    private void OnTriggerEnter(Collider other)
    {
        if (raceManager != null)
        {
            raceManager.OnTriggerFinishLine();
        }
    }
}
