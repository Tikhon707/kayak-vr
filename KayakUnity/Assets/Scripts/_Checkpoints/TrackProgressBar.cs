using UnityEngine;
using UnityEngine.UI;

public class TrackProgressBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider progressSlider;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    private int _totalCheckpoints;
    private Transform[] _checkpointTransforms;
    private Vector3 _startPosition;
    private bool _isInitialized = false;

    private int _currentSegmentIndex = 0;

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform != null)
        {
            _startPosition = playerTransform.position;
        }

        InitializeCheckpoints();
    }

    private void InitializeCheckpoints()
    {
        if (CheckpointManager.Instance == null) return;

        _totalCheckpoints = CheckpointManager.Instance.transform.childCount;
        if (_totalCheckpoints == 0) return;

        // Cache all checkpoint transforms into an array for high-performance distance checks
        _checkpointTransforms = new Transform[_totalCheckpoints];
        for (int i = 0; i < _totalCheckpoints; i++)
        {
            _checkpointTransforms[i] = CheckpointManager.Instance.transform.GetChild(i);
        }

        _isInitialized = true;
    }

    private Vector3 GetWaypointPosition(int index)
    {
        if (index < 0) return _startPosition;
        if (index >= _totalCheckpoints) return _checkpointTransforms[_totalCheckpoints - 1].position;
        return _checkpointTransforms[index].position;
    }

    private void Update()
    {
        if (!_isInitialized || progressSlider == null || playerTransform == null || RaceManager.Instance == null) return;

        if (RaceManager.Instance.currentState != RaceManager.RaceState.Racing) return;

        if (_currentSegmentIndex >= _totalCheckpoints)
        {
            progressSlider.value = 1f;
            return;
        }

        Vector3 startPos = GetWaypointPosition(_currentSegmentIndex - 1);
        Vector3 endPos = GetWaypointPosition(_currentSegmentIndex);

        Vector3 segmentVector = endPos - startPos;
        Vector3 playerVector = playerTransform.position - startPos;

        float sqrLen = segmentVector.sqrMagnitude;
        float t = 0f; //Interpolation paramether

        if (sqrLen > 0.001f)
        {
            // Dot Product Projecting the player onto the track axis.
            // Returns a value from 0 (start of segment) to 1 (end of segment).
            t = Vector3.Dot(playerVector, segmentVector) / sqrLen;
        }

        // Check if the player has physically passed the checkpoint plane (t >= 1)
        if (t >= 1.0f)
        {
            _currentSegmentIndex++;
            t = 0f; // Reset for the next segment calculation
        }

        // Calculate the final smooth progress for the UI slider
        float baseProgress = (float)_currentSegmentIndex / _totalCheckpoints;
        float fractional = Mathf.Clamp01(t) / _totalCheckpoints;

        progressSlider.value = baseProgress + fractional;
    }
}