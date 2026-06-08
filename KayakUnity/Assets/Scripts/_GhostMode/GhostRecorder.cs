using UnityEngine;

public class GhostRecorder : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Трансформ каяка, который мы записываем")]
    public Transform kayakTransform;

    [Header("Settings")]
    [Tooltip("Как часто записывать данные (в секундах). 0.1 = 10 раз в секунду.")]
    public float recordInterval = 0.1f;

    public bool IsRecording { get; private set; }

    public GhostRun CurrentRun { get; private set; }

    private float timeSinceStart = 0f;
    private float recordTimer = 0f;

    public void StartRecording()
    {
        CurrentRun = new GhostRun();
        IsRecording = true;
        timeSinceStart = 0f;
        recordTimer = 0f;
        // first frame on start
        RecordFrame();
    }

    public void StopRecording()
    {
        IsRecording = false;
        RecordFrame();
    }

    void FixedUpdate()
    {
        if (!IsRecording) return;

        timeSinceStart += Time.fixedDeltaTime;
        recordTimer += Time.fixedDeltaTime;

        // Save frame if enough time passed
        if (recordTimer >= recordInterval)
        {
            RecordFrame();
            // subtract the interval, not reset it, so that the time error does not accumulate.
            recordTimer -= recordInterval;
        }
    }

    private void RecordFrame()
    {
        if (!kayakTransform) return;

        GhostFrame newFrame = new GhostFrame
        {
            timestamp = timeSinceStart,
            position = kayakTransform.position,
            rotation = kayakTransform.rotation
        };

        CurrentRun.frames.Add(newFrame);
    }
}
