using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    public static event Action OnRaceStarted;
    public static event Action<float> OnRaceFinished;

    [Header("Components")]
    [SerializeField] private Rigidbody playerKayak;

    [Header("UI Countdown")]
    [SerializeField] private GameObject countdownMenu;
    [SerializeField] private TextMeshProUGUI countdownText;

    public enum RaceState { Waiting, Countdown, Racing, Finished }
    public RaceState currentState = RaceState.Waiting;

    private float currentRaceTime = 0f;
    public float CurrentRaceTime => currentRaceTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Clear countdown UI on start just in case
        if (countdownMenu != null)
            countdownMenu.SetActive(false);

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

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (countdownText != null)
            countdownText.text = "GO!";

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

        OnRaceStarted?.Invoke();
    }

    public void OnTriggerFinishLine()
    {
        if (currentState != RaceState.Racing) return;

        currentState = RaceState.Finished;

        OnRaceFinished?.Invoke(currentRaceTime);
    }

    private void Update()
    {
        if (currentState == RaceState.Racing)
        {
            currentRaceTime += Time.deltaTime;
        }
    }
}
