using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance;

    [Header("Ghost Components")]
    [SerializeField] private GhostRecorder recorder;
    [SerializeField] private GhostPlayer player;

    private GhostRun bestRun;
    private float bestTime = float.MaxValue;
    
    // Свойство для получения ID текущей сцены
    private string SceneID => SceneManager.GetActiveScene().name;
    private string SavePath => Application.persistentDataPath + $"/best_ghost_{SceneID}.json";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadGhost();
    }

    private void OnEnable()
    {
        RaceManager.OnRaceStarted += StartGhostSystem;
        RaceManager.OnRaceFinished += StopGhostSystem;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        RaceManager.OnRaceStarted -= StartGhostSystem;
        RaceManager.OnRaceFinished -= StopGhostSystem;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bestTime = float.MaxValue;
        bestRun = null;
        LoadGhost();
    }

    public void StartGhostSystem()
    {
        if (recorder != null)
        {
            recorder.StartRecording();
        }

        if (player != null && bestRun != null && bestRun.frames.Count > 0)
        {
            player.PlayRun(bestRun);
        }
    }

    public void StopGhostSystem(float finalRaceTime)
    {
        if (recorder != null)
        {
            recorder.StopRecording();
        }

        if (finalRaceTime < bestTime)
        {
            bestTime = finalRaceTime;
            bestRun = recorder.CurrentRun;
            bestRun.raceTime = finalRaceTime;
            SaveGhost();
            Debug.Log($"New record on {SceneID}! Time: {bestTime:F2} sec.");
        }
        else
        {
            Debug.Log($"Finish! Time: {finalRaceTime:F2} sec. Record not broken ({bestTime:F2}).");
        }
    }

    private void SaveGhost()
    {
        string json = JsonUtility.ToJson(bestRun);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Ghost saved for {SceneID} to: {SavePath}");
    }

    private void LoadGhost()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            bestRun = JsonUtility.FromJson<GhostRun>(json);
            bestTime = bestRun.raceTime;
            Debug.Log($"Ghost loaded for {SceneID}. Best time: {bestTime:F2} sec.");
        }
    }
}