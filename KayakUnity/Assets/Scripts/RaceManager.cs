using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    // --- EVENTS ---
    public static event Action OnRaceStarted;
    public static event Action<float> OnRaceFinished;

    [Header("Components")] [SerializeField]
    private Rigidbody playerKayak;

    [Header("UI Countdown")] [SerializeField]
    private GameObject countdownMenu;

    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("ScoreManager")] [SerializeField]
    private GameObject scoreManagerObj;

    private BaseScoreManager _baseScoreManager;

    public enum RaceState
    {
        Waiting,
        Countdown,
        Racing,
        Finished
    }

    public RaceState currentState = RaceState.Waiting;

    private float _currentRaceTime = 0f;
    public float CurrentRaceTime => _currentRaceTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        _baseScoreManager = scoreManagerObj.GetComponent<BaseScoreManager>();
    }

    private void Start()
    {
        if (countdownMenu != null)
            countdownMenu.SetActive(false);

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        currentState = RaceState.Countdown;

        if (playerKayak)
            playerKayak.isKinematic = true;

        if (countdownMenu)
            countdownMenu.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            if (countdownText)
                countdownText.text = count.ToString();

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (countdownText)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(0.5f);

        if (countdownMenu)
            countdownMenu.SetActive(false);

        if (playerKayak)
            playerKayak.isKinematic = false;

        StartRace();
    }

    private void StartRace()
    {
        currentState = RaceState.Racing;
        _currentRaceTime = 0f;

        OnRaceStarted?.Invoke();
    }

    public void OnTriggerFinishLine()
    {
        if (_baseScoreManager == null)
            return;
        if (currentState != RaceState.Racing) return;

        currentState = RaceState.Finished;
        OnRaceFinished?.Invoke(_currentRaceTime);
    }

    private void Update()
    {
        if (currentState == RaceState.Racing)
        {
            _currentRaceTime += Time.deltaTime;
        }
    }
}