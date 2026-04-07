using UnityEngine;

public class RacistTrigger : MonoBehaviour
{
    public RacistManager raceManager;
    public bool isStartLine = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isStartLine)
                raceManager.OnTriggerStartLine();
            else
                raceManager.OnTriggerFinishLine();
        }
    }
}
