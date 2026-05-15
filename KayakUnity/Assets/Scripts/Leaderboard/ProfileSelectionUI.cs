using UnityEngine;

public class ProfileSelectionUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootToHide;

    [Header("Panels")]
    [SerializeField] private ProfileSelectorPanel selectorPanel;
    [SerializeField] private NewProfilePanel newProfilePanel;

    private void Start()
    {
        if (PlayerProfile.KnownNames.Count == 0) ShowNewProfile();
        else ShowSelector();
    }

    public void OnRequestNewProfile()
    {
        ShowNewProfile();
    }

    public void OnRequestSelector()
    {
        ShowSelector();
    }

    public void OnProfileChosen()
    {
        if (rootToHide != null) rootToHide.SetActive(false);
        else gameObject.SetActive(false);
    }

    private void ShowSelector()
    {
        if (newProfilePanel != null) newProfilePanel.gameObject.SetActive(false);
        if (selectorPanel != null) selectorPanel.gameObject.SetActive(true);
    }

    private void ShowNewProfile()
    {
        if (selectorPanel != null) selectorPanel.gameObject.SetActive(false);
        if (newProfilePanel != null) newProfilePanel.gameObject.SetActive(true);
    }
}