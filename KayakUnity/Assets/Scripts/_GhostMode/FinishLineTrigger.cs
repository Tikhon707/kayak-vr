using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnTriggerFinishLine();
            }
        }
    }
}
