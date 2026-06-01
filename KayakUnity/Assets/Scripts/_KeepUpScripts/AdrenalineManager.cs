using System;
using UnityEngine;
using UnityEngine.UI;

public class AdrenalineManager : MonoBehaviour
{
    // Event to be caught by MenuManager to trigger the Game Over panel
    public static event Action OnBombExploded;

    public enum BombState { GracePeriod, Idle, Awakening, Ticking, Detonating, Detonated }

    [Header("State")]
    public BombState currentState = BombState.GracePeriod;

    [Header("References")]
    [SerializeField] private Rigidbody kayakRB;
    [SerializeField] private GameObject bombModel;
    [SerializeField] private GameObject ledIndicator;
    [SerializeField] private AudioSource sfxSource;

    [Header("UI Pace Slider")]
    [SerializeField] private Image paceBarFill;
    [SerializeField] private RectTransform paceMarker;
    [SerializeField] private float maxDisplaySpeed = 20f; // Max speed for the bar (100% fill)
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip armedClip;
    [SerializeField] private AudioClip awakeClip; // 3 rapid beeps
    [SerializeField] private AudioClip tickClip;  // Single short beep
    [SerializeField] private AudioClip boomDelay;
    [SerializeField] private AudioClip boomClip;


    [Header("Pace Settings")]
    [SerializeField] private float minimumPaceSpeed = 10f;
    [SerializeField] private float defusePaceMargin = 2f;
    [SerializeField] private float timeToExplode = 10f;
    [SerializeField] private float gracePeriodDuration = 5f;
    [SerializeField] private float detonationDelay = 2.0f;

    [Header("Feedback Settings")]
    [SerializeField] private float maxTickInterval = 1.0f; // Time between ticks at 10 seconds
    [SerializeField] private float minTickInterval = 0.3f; // Time between ticks right before explosion
    [SerializeField] private ParticleSystem explosionParticles;

    private float _bombTimer;
    private float _nextTickTime;
    private float _graceTimer;
    private float _detonationTimer;

    private void Start()
    {
        InitializePaceSlider();
        ResetBomb();
    }

    private void InitializePaceSlider()
    {
        if (paceMarker)
        {
            float normalizedThreshold = Mathf.Clamp01(minimumPaceSpeed / maxDisplaySpeed);

            // Save the actual visual HEIGHT of the marker before changing anchors
            float markerHeight = paceMarker.rect.height;

            // Anchor to the Y axis (vertical position), stretch across X (horizontal width)
            paceMarker.anchorMin = new Vector2(0f, normalizedThreshold);
            paceMarker.anchorMax = new Vector2(1f, normalizedThreshold);

            // Apply the saved height explicitly so it doesn't vanish, keeping the X offset at 0
            paceMarker.offsetMin = new Vector2(0f, -markerHeight / 2f);
            paceMarker.offsetMax = new Vector2(0f, markerHeight / 2f);
        }
    }

    private void Update()
    {
        // Ensure we only run logic if the race is active
        if (RaceManager.Instance == null || RaceManager.Instance.currentState != RaceManager.RaceState.Racing)
            return;

        float speedKmh = GetKayakSpeed();

        UpdatePaceSliderUI(speedKmh);

        switch (currentState)
        {
            case BombState.GracePeriod: // ADDED: Handle the initial grace period
                HandleGracePeriodState();
                break;
            case BombState.Idle:
                HandleIdleState(speedKmh);
                break;
            case BombState.Awakening:
                // Handled via Coroutine or duration check. For simplicity, assuming awakeClip dictates duration.
                // Once awake sequence is done, move to Ticking.
                break;
            case BombState.Ticking:
                HandleTickingState(speedKmh);
                break;
            case BombState.Detonating: // ADDED: Handle the pre-explosion delay phase
                HandleDetonatingState();
                break;
            case BombState.Detonated:
                // Do nothing, race is over
                break;
        }
    }

    private void UpdatePaceSliderUI(float currentSpeed)
    {
        if (!paceBarFill) return;

        float fillTarget = Mathf.Clamp01(currentSpeed / maxDisplaySpeed);

        // Smoothly interpolate the bar to avoid VR jitter from physics fluctuations
        paceBarFill.fillAmount = Mathf.Lerp(paceBarFill.fillAmount, fillTarget, Time.deltaTime * 5f);

        // Shift color based on our threshold
        if (currentSpeed >= minimumPaceSpeed)
        {
            paceBarFill.color = safeColor;
        }
        else
        {
            paceBarFill.color = dangerColor;
        }
    }

    private void HandleGracePeriodState()
    {
        _graceTimer -= Time.deltaTime;

        if (_graceTimer <= 0f)
        {
            currentState = BombState.Idle;
            PlayArmedFeedback();
        }
    }

    private void PlayArmedFeedback()
    {
        if (sfxSource && armedClip)
        {
            sfxSource.PlayOneShot(armedClip);
        }

        if (ledIndicator)
        {
            ledIndicator.SetActive(true);

            // Turn off the LED after 0.5 seconds to create a single blink effect
            Invoke(nameof(TurnOffLed), 0.5f);
        }
    }

    private void TurnOffLed()
    {
        if (ledIndicator)
        {
            ledIndicator.SetActive(false);
        }
    }

    private void HandleIdleState(float speedKmh)
    {
        if (speedKmh < minimumPaceSpeed)
        {
            TriggerAwakening();
        }
    }

    private void TriggerAwakening()
    {
        currentState = BombState.Awakening;

        if (sfxSource && awakeClip)
        {
            sfxSource.PlayOneShot(awakeClip);
        }

        // Delay ticking state until awake clip finishes (or use a fixed 1-second delay)
        float awakeDuration = awakeClip ? awakeClip.length : 1f;
        Invoke(nameof(StartTicking), awakeDuration);
    }

    private void StartTicking()
    {
        currentState = BombState.Ticking;
        _bombTimer = timeToExplode;
        _nextTickTime = timeToExplode;
    }

    private void HandleTickingState(float speedKmh)
    {
        // 1. Check if player sprinted to defuse the bomb
        if (speedKmh > minimumPaceSpeed + defusePaceMargin)
        {
            ResetBombToIdle();
            return;
        }

        // 2. Count down the bomb timer
        _bombTimer -= Time.deltaTime;

        // 3. Handle visual and audio feedback (Linearly decreasing interval)
        float progress = _bombTimer / timeToExplode; // 1 to 0
        float currentTickInterval = Mathf.Lerp(minTickInterval, maxTickInterval, progress);

        if (_bombTimer <= _nextTickTime)
        {
            PlayTickFeedback();
            _nextTickTime = _bombTimer - currentTickInterval;
        }

        // 4. Detonate if time runs out
        if (_bombTimer <= 0f)
        {
            StartDetonationSequence();
        }
    }

    private void PlayTickFeedback()
    {
        if (sfxSource && tickClip)
        {
            sfxSource.PlayOneShot(tickClip);
        }

        if (ledIndicator)
        {
            ledIndicator.SetActive(true);
            // Cancel any pending turn-off commands to prevent overlapping when the timer ticks very fast at the end
            CancelInvoke(nameof(TurnOffLed));
            Invoke(nameof(TurnOffLed), 0.1f);
        }
    }

    private void StartDetonationSequence()
    {
        currentState = BombState.Detonating;
        _detonationTimer = detonationDelay;

        if (ledIndicator)
        {
            CancelInvoke(nameof(TurnOffLed));
            ledIndicator.SetActive(true);
        }

        if (sfxSource && boomDelay) sfxSource.PlayOneShot(boomDelay);
    }

    private void HandleDetonatingState()
    {
        _detonationTimer -= Time.deltaTime;

        if (_detonationTimer <= 0f)
        {
            FinalizeExplosion();
        }
    }

    private void FinalizeExplosion()
    {
        currentState = BombState.Detonated;

        if (sfxSource && boomClip) sfxSource.PlayOneShot(boomClip);
        if (explosionParticles) explosionParticles.Play();
        if (bombModel) bombModel.SetActive(false);
        if (ledIndicator) ledIndicator.SetActive(false);

        OnBombExploded?.Invoke();
    }

    private void ResetBomb()
    {
        currentState = BombState.GracePeriod; // EDITED: Bomb now always resets into GracePeriod
        _graceTimer = gracePeriodDuration;    // ADDED: Initialize grace timer
        _bombTimer = timeToExplode;

        TurnOffLed();
    }

    private void ResetBombToIdle()
    {
        currentState = BombState.Idle;
        _bombTimer = timeToExplode;

        TurnOffLed();
    }

    private float GetKayakSpeed()
    {
        if (!kayakRB) return 0f;

        float sqrSpeed = kayakRB.linearVelocity.sqrMagnitude;
        return sqrSpeed < 0.1f ? 0f : Mathf.Sqrt(sqrSpeed) * 3.6f;
    }
}
