using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSelectorPanel : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ProfileSelectionUI parentUI;
    [SerializeField] private Transform listContainer;
    [SerializeField] private GameObject nameButtonPrefab;
    [SerializeField] private Button newPlayerButton;

    private void OnEnable()
    {
        if (newPlayerButton != null)
        {
            newPlayerButton.onClick.RemoveAllListeners();
            newPlayerButton.onClick.AddListener(() => parentUI.OnRequestNewProfile());
            newPlayerButton.gameObject.SetActive(PlayerProfile.KnownNames.Count < PlayerProfile.MaxProfiles);
        }

        Rebuild();
    }

    private void Rebuild()
    {
        Debug.Log($"[Selector] Rebuild called, KnownNames count = {PlayerProfile.KnownNames.Count}, container = {(listContainer == null ? "null" : listContainer.name)}");
        if (listContainer == null || nameButtonPrefab == null) return;

        for (int i = listContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(listContainer.GetChild(i).gameObject);
        }

        foreach (string name in PlayerProfile.KnownNames)
        {
            string captured = name;
            GameObject go = Instantiate(nameButtonPrefab, listContainer, false);

            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = captured;

            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PlayerProfile.Select(captured);
                    parentUI.OnProfileChosen();
                });
            }
        }
    }
}