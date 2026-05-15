using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private bool isFinish = false;
    [SerializeField] private float bonusTime = 15f;
    public float fadeDuration = 0.5f;
    public Transform arrow;
    private bool isTriggered = false;

    [Header("Рендереры для растворения")]
    public MeshRenderer ringRenderer;
    public MeshRenderer arrowRenderer;

    public void ResetCheckpoint() => isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;

            if (!isFinish) CheckpointManager.Instance.OnCheckpointPassed();
            else CheckpointManager.Instance.OnFinish();
            StartCoroutine(FadeOutAndDisable());
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

        // Получаем уникальные копии материалов
        Material ringMat = ringRenderer != null ? ringRenderer.material : null;
        Material arrowMat = arrowRenderer != null ? arrowRenderer.material : null;

        // Запоминаем их стартовые цвета (нам важен Альфа-канал)
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