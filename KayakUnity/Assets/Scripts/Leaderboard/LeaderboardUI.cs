using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject emptyLabel;

    [Header("Settings")]
    [SerializeField] private int topN = 10;

    public void Show(string sceneName, string highlightPlayerName)
    {
        gameObject.SetActive(true);
        Debug.Log($"[LeaderboardUI] Show called: scene='{sceneName}', highlight='{highlightPlayerName}', gameObject.activeInHierarchy={gameObject.activeInHierarchy}, rowContainer={(rowContainer == null ? "NULL" : rowContainer.name)}, rowPrefab={(rowPrefab == null ? "NULL" : rowPrefab.name)}, emptyLabel={(emptyLabel == null ? "NULL" : emptyLabel.name)}");
        Clear();

        List<LeaderboardEntry> top = LeaderboardService.GetTop(sceneName, topN);
        Debug.Log($"[LeaderboardUI] GetTop('{sceneName}', {topN}) returned {top.Count} entries");

        if (emptyLabel != null) emptyLabel.SetActive(top.Count == 0);

        if (rowContainer == null || rowPrefab == null)
        {
            Debug.LogWarning("[LeaderboardUI] rowContainer or rowPrefab is null — cannot build rows");
            return;
        }

        for (int i = 0; i < top.Count; i++)
        {
            GameObject go = Instantiate(rowPrefab, rowContainer, false);
            LeaderboardRow row = go.GetComponent<LeaderboardRow>();
            if (row != null)
            {
                bool isHighlighted = !string.IsNullOrEmpty(highlightPlayerName)
                                     && top[i].playerName == highlightPlayerName;
                row.Bind(i + 1, top[i].playerName, top[i].time, isHighlighted);
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        if (rowContainer == null) return;
        for (int i = rowContainer.childCount - 1; i >= 0; i--)
            Destroy(rowContainer.GetChild(i).gameObject);
    }
}