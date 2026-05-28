using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float bonusTime = 15f;
    public float fadeDuration = 0.5f;
    public Transform arrow;
    private bool isTriggered = false;

    [Header("Vanish renders")]
    public MeshRenderer ringRenderer;
    public MeshRenderer arrowRenderer;

    [SerializeField] private GameObject finishFlagVisual;

    public void ResetCheckpoint() => isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;
            // Report that checkpoint is passed. The manager will figure out the rest.
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.OnCheckpointPassed();
            }

            StartCoroutine(FadeOutAndDisable());
        }
    }

    public void ShowFinishFlag()
    {
        if (finishFlagVisual != null)
        {
            finishFlagVisual.SetActive(true);
        }
    }

    public void PointTo(Transform nextCheckpoint)
    {
        if (arrow != null && nextCheckpoint != null)
        {
            arrow.LookAt(nextCheckpoint.position);
        }
    }

    public void HideArrow()
    {
        if (arrow != null)
        {
            arrow.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        float elapsedTime = 0f;

        Material ringMat = ringRenderer != null ? ringRenderer.material : null;
        Material arrowMat = arrowRenderer != null ? arrowRenderer.material : null;

        Color ringColor = ringMat != null ? ringMat.color : Color.white;
        Color arrowColor = arrowMat != null ? arrowMat.color : Color.white;

        float startRingAlpha = ringColor.a;
        float startArrowAlpha = arrowColor.a;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            if (ringMat != null)
            {
                ringColor.a = Mathf.Lerp(startRingAlpha, 0f, t);
                ringMat.color = ringColor;
            }

            if (arrowMat != null)
            {
                arrowColor.a = Mathf.Lerp(startArrowAlpha, 0f, t);
                arrowMat.color = arrowColor;
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }
}