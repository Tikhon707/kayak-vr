using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Ññûëêè")]
    [SerializeField] private BoatDashboard dashboard;

    [Header("Çâóêè")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip finishClip;

    [Header("Íàñòðîéêè")]
    [SerializeField] private int totalCheckpoints;
    

    private int _passedCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        totalCheckpoints = transform.childCount;
    }

    void Start()
    {
        if (dashboard != null)
        {
            dashboard.UpdateCheckpointsUI(0, totalCheckpoints);
        }

        for (int i = 0; i < totalCheckpoints - 1; i++)
        {
            Checkpoint current = transform.GetChild(i).GetComponent<Checkpoint>();
            Checkpoint next = transform.GetChild(i + 1).GetComponent<Checkpoint>();

            if (current != null && next != null)
            {
                current.PointTo(next.transform);
            }
        }

        if (totalCheckpoints > 0)
        {
            Checkpoint last = transform.GetChild(totalCheckpoints - 1).GetComponent<Checkpoint>();
            if (last != null)
            {
                last.HideArrow();
            }
        }
    }

    public void OnCheckpointPassed()
    {
        _passedCount++;
        //dashboard.AddTime(bonusTime);
        dashboard.UpdateCheckpointsUI(_passedCount, totalCheckpoints);

        if (audioSource && checkpointClip) audioSource.PlayOneShot(checkpointClip);
        Debug.Log($"[Checkpoint] {_passedCount}/{totalCheckpoints}");
    }

    public void OnFinish()
    {
        if (audioSource && finishClip) audioSource.PlayOneShot(finishClip);
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