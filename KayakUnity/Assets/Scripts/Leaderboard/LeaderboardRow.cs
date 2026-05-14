using TMPro;
using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private GameObject highlight;

    public void Bind(int rank, string playerName, float time, bool isHighlighted)
    {
        if (rankText != null) rankText.text = $"{rank}.";
        if (nameText != null) nameText.text = playerName;
        if (timeText != null) timeText.text = FormatTime(time);
        if (highlight != null) highlight.SetActive(isHighlighted);
    }

    private static string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int m = (int)(seconds / 60f);
        int s = (int)(seconds % 60f);
        int ms = (int)((seconds - Mathf.Floor(seconds)) * 1000f);
        return $"{m:00}:{s:00}.{ms:000}";
    }
}