using UnityEngine;

public class GhostPlayback : MonoBehaviour
{
    private GhostData _data;
    private float _playbackTime;
    private bool _isPlaying;
    private int _frameIndex;

    public void StartPlayback(GhostData data)
    {
        if (data == null || data.frames == null || data.frames.Count < 2)
        {
            Debug.Log("[GhostPlayback] Нет данных для воспроизведения — объект скрыт");
            gameObject.SetActive(false);
            return;
        }

        _data = data;
        _data = data;
        _playbackTime = 0f;
        _frameIndex = 0;
        _isPlaying = true;
        gameObject.SetActive(true);

        transform.SetPositionAndRotation(data.frames[0].position, data.frames[0].rotation);
        Debug.Log($"[GhostPlayback] Старт воспроизведения: {data.frames.Count} кадров, позиция старта: {data.frames[0].position}");
    }

    public void StopPlayback()
    {
        _isPlaying = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isPlaying || _data == null) return;

        _playbackTime += Time.deltaTime;

        var frames = _data.frames;
        int last = frames.Count - 1;

        while (_frameIndex < last - 1 && frames[_frameIndex + 1].time <= _playbackTime)
            _frameIndex++;

        if (_frameIndex >= last)
        {
            transform.SetPositionAndRotation(frames[last].position, frames[last].rotation);
            _isPlaying = false;
            return;
        }

        var a = frames[_frameIndex];
        var b = frames[_frameIndex + 1];
        float t = Mathf.InverseLerp(a.time, b.time, _playbackTime);

        transform.SetPositionAndRotation(
            Vector3.Lerp(a.position, b.position, t),
            Quaternion.Slerp(a.rotation, b.rotation, t)
        );
    }
}
