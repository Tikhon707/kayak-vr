using UnityEngine;
using System.IO;
using System.Collections;
using TMPro;

public class RaceManager : MonoBehaviour
{
    [Header("Components")]
    public GhostRecorder recorder;
    public GhostPlayer player;
    public Rigidbody playerKayak;

    [Header("UI Countdown")]
    public GameObject countdownMenu;
    public TextMeshProUGUI countdownText;

    public enum RaceState { Waiting, Countdown, Racing, Finished  };
    public RaceState currentState = RaceState.Waiting;

    private GhostRun bestRun;
    private float bestTime = float.MaxValue;
    private float currentRaceTime = 0f;
    private string SavePath => Application.persistentDataPath + "/best_ghost.json";

    void Start()
    {
        LoadGhost();
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        currentState = RaceState.Countdown;

        if (playerKayak != null)
            playerKayak.isKinematic = true;

        if (countdownMenu != null)
            countdownMenu.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            if (countdownText != null)
                countdownText.text = count.ToString();

            //audioSource.PlayOneShot(beepClip);

            yield return new WaitForSeconds(1f);
            count--;
        }

        // 4. Сигнал к старту
        if (countdownText != null)
            countdownText.text = "GO!";

        //audioSource.PlayOneShot(startClip);

        yield return new WaitForSeconds(0.5f);

        if (countdownMenu != null)
            countdownMenu.SetActive(false);

        if (playerKayak != null)
            playerKayak.isKinematic = false;

        StartRace();
    }

    private void StartRace()
    {
        currentState = RaceState.Racing;
        currentRaceTime = 0f;

        recorder.StartRecording();

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnStartGameButton();
        }

        if (bestRun != null && bestRun.frames.Count > 0)
        {
            player.PlayRun(bestRun);
        }
    }

    public void OnTriggerFinishLine()
    {
        if (currentState != RaceState.Racing) return;

        currentState = RaceState.Finished;
        recorder.StopRecording();

        // Check record
        if (currentRaceTime < bestTime)
        {
            bestTime = currentRaceTime;
            bestRun = recorder.CurrentRun; // rewrite record
            bestRun.raceTime = currentRaceTime; // note our time into struct
            SaveGhost(); // Save new record
            Debug.Log($"New record! Time: {bestTime:F2} sec.");
        }
        else
        {
            Debug.Log($"Finish! Time: {currentRaceTime:F2} sec. Record not broken ({bestTime:F2}).");
        }

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnFinish();
        }
    }

    void Update()
    {
        // Count race time
        if (currentState == RaceState.Racing)
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
        Debug.Log("Ghost saved: " + SavePath);
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
            Debug.Log($"Ghost loaded. Last best: {bestTime:F2} sec.");
        }
    }
}
