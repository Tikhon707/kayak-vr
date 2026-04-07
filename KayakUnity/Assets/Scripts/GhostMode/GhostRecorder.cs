using System.Collections.Generic;
using UnityEngine;

public class GhostRecorder : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] public Transform kayakTransform;

    [Header("Настройки")]
    [Tooltip("Записывать каждые N FixedUpdate-кадров (~2 = 25fps записи)")]
    [SerializeField] private int recordEveryNFrames = 2;

    private List<GhostFrame> _frames = new List<GhostFrame>();
    private bool _isRecording;
    private float _raceTime;
    private int _frameCounter;

    public void StartRecording()
    {
        _frames.Clear();
        _raceTime = 0f;
        _frameCounter = 0;
        _isRecording = true;
    }

    public GhostData StopRecording(float finalTime)
    {
        _isRecording = false;
        return new GhostData
        {
            totalTime = finalTime,
            frames = new List<GhostFrame>(_frames)
        };
    }

    public void ResetRecording()
    {
        _isRecording = false;
        _frames.Clear();
        _raceTime = 0f;
        _frameCounter = 0;
    }

    private void FixedUpdate()
    {
        if (!_isRecording || kayakTransform == null) return;

        _raceTime += Time.fixedDeltaTime;
        _frameCounter++;
        if (_frameCounter < recordEveryNFrames) return;
        _frameCounter = 0;

        _frames.Add(new GhostFrame
        {
            time = _raceTime,
            position = kayakTransform.position,
            rotation = kayakTransform.rotation
        });
    }
}
