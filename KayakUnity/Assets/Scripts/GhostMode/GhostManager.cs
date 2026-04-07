using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance;

    [Header("Ссылки")]
    [SerializeField] private GhostRecorder recorder;
    [SerializeField] private GhostPlayback playback;

    private GhostData _bestData;
    private string SavePath
    {
        get
        {
            string dir = Path.Combine(Application.dataPath, "GhostData");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"ghost_{SceneManager.GetActiveScene().name}.json");
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        LoadBestRun();
    }

    public void StartRace()
    {
        recorder.StartRecording();
        playback.StartPlayback(_bestData);
    }

    public void FinishRace(float finalTime)
    {
        GhostData newData = recorder.StopRecording(finalTime);
        _bestData = newData;
        SaveBestRun();
        Debug.Log($"[Ghost] Заезд сохранён: {finalTime:F2}s, кадров: {newData.frames.Count}");
    }

    public void ResetRace()
    {
        recorder.ResetRecording();
        playback.StopPlayback();
    }

    private void SaveBestRun()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(_bestData));
        Debug.Log($"[Ghost] Сохранено в {SavePath}");
    }

    private void LoadBestRun()
    {
        if (!File.Exists(SavePath)) return;
        _bestData = JsonUtility.FromJson<GhostData>(File.ReadAllText(SavePath));
        Debug.Log($"[Ghost] Загружен лучший заезд: {_bestData?.totalTime:F2}s, кадров: {_bestData?.frames.Count}");
    }
}
