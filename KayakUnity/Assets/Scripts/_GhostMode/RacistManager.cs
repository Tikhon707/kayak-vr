using UnityEngine;
using System.IO;

public class RacistManager : MonoBehaviour
{
    [Header("Components")]
    public GhostRecorder recorder;
    public GhostPlayer player;

    private GhostRun bestRun;
    private float bestTime = float.MaxValue;

    private bool isRacing = false;
    private float currentRaceTime = 0f;
    private string SavePath => Application.persistentDataPath + "/best_ghost.json";

    void Start()
    {
        LoadGhost();
    }

    public void OnTriggerStartLine()
    {
        if (isRacing) return;

        isRacing = true;
        currentRaceTime = 0f;

        recorder.StartRecording();

        if (bestRun != null && bestRun.frames.Count > 0)
        {
            player.PlayRun(bestRun);
        }
    }

    public void OnTriggerFinishLine()
    {
        if (!isRacing) return;

        isRacing = false;
        recorder.StopRecording();

        // Check record
        if (currentRaceTime < bestTime)
        {
            bestTime = currentRaceTime;
            bestRun = recorder.CurrentRun; // rewrite record
            bestRun.raceTime = currentRaceTime; // note our time into struct
            SaveGhost(); // Save new record
            Debug.Log($"Новый рекорд! Время: {bestTime:F2} сек.");
        }
        else
        {
            Debug.Log($"Финиш! Время: {currentRaceTime:F2} сек. Рекорд не побит ({bestTime:F2}).");
        }
    }

    void Update()
    {
        // Count race time
        if (isRacing)
        {
            currentRaceTime += Time.deltaTime;
        }
    }

    // --- SAVE AND LOAD ---

    private void SaveGhost()
    {
        // Convert calss into JSON-line
        string json = JsonUtility.ToJson(bestRun);
        // save line into JSON
        File.WriteAllText(SavePath, json);
        Debug.Log("Призрак сохранен по пути: " + SavePath);
    }

    private void LoadGhost()
    {
        // Check if save exists
        if (File.Exists(SavePath))
        {
            // Read file
            string json = File.ReadAllText(SavePath);
            // Convert JSON back to GhostFile
            bestRun = JsonUtility.FromJson<GhostRun>(json);
            bestTime = bestRun.raceTime;

            Debug.Log($"Призрак успешно загружен. Прошлый рекорд: {bestTime:F2} сек.");
        }
    }
}
