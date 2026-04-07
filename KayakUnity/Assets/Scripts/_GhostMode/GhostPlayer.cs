using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
    [Header("Ghost Visuals")]
    [Tooltip("Трансформ префаба призрака")]
    public Transform ghostTransform;

    private GhostRun currentRun;
    private bool isPlaying = false;
    private float timeSinceStart = 0f;
    private int currentFrameIndex = 0;

    public void PlayRun(GhostRun runData)
    {
        if (runData == null || runData.frames.Count == 0) return;

        currentRun = runData;
        timeSinceStart = 0f;
        currentFrameIndex = 0;
        isPlaying = true;

        ghostTransform.gameObject.SetActive(true);

        ghostTransform.position = currentRun.frames[0].position;
        ghostTransform.rotation = currentRun.frames[0].rotation;
    }

    public void StopPlayback()
    {
        isPlaying = false;
        if (ghostTransform != null)
        {
            //ghostTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        timeSinceStart += Time.deltaTime;

        if (currentFrameIndex >= currentRun.frames.Count - 1)
        {
            StopPlayback();
            return;
        }

        // Freeze fail-safe code. Searching for actual frame for Ghost if time went ahead
        while (currentFrameIndex < currentRun.frames.Count - 1 &&
               timeSinceStart > currentRun.frames[currentFrameIndex + 1].timestamp)
        {
            currentFrameIndex++;
        }

        if (currentFrameIndex >= currentRun.frames.Count - 1) return;

        // Current and next frames for interpolation
        GhostFrame frameA = currentRun.frames[currentFrameIndex];
        GhostFrame frameB = currentRun.frames[currentFrameIndex + 1];

        // Time progress between frames (from 0.0 up to 1.0)
        float timeBetweenFrames = frameB.timestamp - frameA.timestamp;
        float timePassedSinceFrameA = timeSinceStart - frameA.timestamp;
        float interpolationFactor = timePassedSinceFrameA / timeBetweenFrames;

        // Interpolation for pos and rot
        ghostTransform.position = Vector3.Lerp(frameA.position, frameB.position, interpolationFactor);
        ghostTransform.rotation = Quaternion.Slerp(frameA.rotation, frameB.rotation, interpolationFactor);
    }
}
