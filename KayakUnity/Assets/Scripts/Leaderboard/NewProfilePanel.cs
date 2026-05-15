using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewProfilePanel : MonoBehaviour
{
    private const int LetterCount = 26;

    [Header("Refs")]
    [SerializeField] private ProfileSelectionUI parentUI;

    [Header("Letter slots (size 3)")]
    [SerializeField] private TMP_Text[] letterLabels = new TMP_Text[3];

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Collision warning")]
    [SerializeField] private GameObject collisionRoot;
    [SerializeField] private TMP_Text collisionText;

    private readonly int[] _indices = new int[3];

    private void OnEnable()
    {
        for (int i = 0; i < _indices.Length; i++) _indices[i] = 0;
        RefreshLabels();
        ShowCollision(false);

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirm);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(PlayerProfile.KnownNames.Count > 0);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBack);
        }
    }

    public void OnUpSlot0() { Bump(0, +1); }
    public void OnUpSlot1() { Bump(1, +1); }
    public void OnUpSlot2() { Bump(2, +1); }
    public void OnDownSlot0() { Bump(0, -1); }
    public void OnDownSlot1() { Bump(1, -1); }
    public void OnDownSlot2() { Bump(2, -1); }

    private void Bump(int slot, int delta)
    {
        _indices[slot] = (_indices[slot] + delta + LetterCount) % LetterCount;
        if (IsCollisionShown()) ShowCollision(false);
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        for (int i = 0; i < _indices.Length; i++)
        {
            if (letterLabels != null && i < letterLabels.Length && letterLabels[i] != null)
                letterLabels[i].text = ((char)('A' + _indices[i])).ToString();
        }
    }

    private string CurrentName()
    {
        char[] chars = new char[3];
        for (int i = 0; i < 3; i++) chars[i] = (char)('A' + _indices[i]);
        return new string(chars);
    }

    private void OnConfirm()
    {
        string name = CurrentName();

        if (IsCollisionShown())
        {
            PlayerProfile.Select(name);
            parentUI.OnProfileChosen();
            return;
        }

        if (PlayerProfile.Exists(name))
        {
            if (collisionText != null) collisionText.text = $"Профиль {name} уже существует. Confirm — играть как он, Back — выбрать другое имя.";
            ShowCollision(true);
            return;
        }

        if (PlayerProfile.KnownNames.Count >= PlayerProfile.MaxProfiles)
        {
            if (collisionText != null) collisionText.text = $"Достигнут лимит профилей ({PlayerProfile.MaxProfiles}). Удалите старый, чтобы создать новый.";
            ShowCollision(true);
            return;
        }

        PlayerProfile.Select(name);
        parentUI.OnProfileChosen();
    }

    private void OnBack()
    {
        if (IsCollisionShown())
        {
            ShowCollision(false);
            return;
        }

        parentUI.OnRequestSelector();
    }

    private void ShowCollision(bool on)
    {
        if (collisionRoot != null) collisionRoot.SetActive(on);
    }

    private bool IsCollisionShown()
    {
        return collisionRoot != null && collisionRoot.activeSelf;
    }
}