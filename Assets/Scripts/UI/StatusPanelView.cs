using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class StatusPanelView : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private PartyService partyService;
    private VisualElement screenInstance;
    private VisualElement portraitHost;
    private Image portrait;
    private Label level;
    private Label characterClass;
    private Label health;
    private Label mana;
    private Label experience;
    private Label stamina;
    private Label intellect;
    private readonly VisualElement[] tabs = new VisualElement[PartyRules.MaximumMembers];
    private readonly Label[] tabNames = new Label[PartyRules.MaximumMembers];
    private IVisualElementScheduledItem pendingFocus;
    private PartyMember selectedMember;
    private int selectedIndex;

    public bool IsShown { get; private set; }
    public bool IsReady { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        BindService();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        if (!ReferenceEquals(partyService, PartyService.Instance))
        {
            BindService();
        }
    }

    private void BindService()
    {
        if (partyService != null)
        {
            partyService.Changed -= Refresh;
        }

        partyService = PartyService.Instance;
        Refresh();

        if (partyService != null)
        {
            partyService.Changed += Refresh;
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindUI();
        screenInstance = rootElement?.Q<VisualElement>("StatusScreenInstance");
        VisualElement screen = screenInstance?.Q<VisualElement>("StatusScreen");
        portraitHost = screen?.Q<VisualElement>("StatusPortraitHost");
        portrait = screen?.Q<Image>("StatusPortrait");
        level = screen?.Q<Label>("StatusLevel");
        characterClass = screen?.Q<Label>("StatusClass");
        health = screen?.Q<Label>("StatusHealth");
        mana = screen?.Q<Label>("StatusMana");
        experience = screen?.Q<Label>("StatusExperience");
        stamina = screen?.Q<Label>("StatusStamina");
        intellect = screen?.Q<Label>("StatusIntellect");

        if (screen == null || portraitHost == null || portrait == null || level == null
            || characterClass == null || health == null || mana == null || experience == null
            || stamina == null || intellect == null)
        {
            Debug.LogError("StatusPanelView requires the complete StatusScreen template.", this);

            return;
        }

        for (int index = 0; index < tabs.Length; index++)
        {
            tabs[index] = screen.Q<VisualElement>($"StatusTab{index + 1}");
            tabNames[index] = tabs[index]?.Q<Label>("StatusTabName");

            if (tabs[index] == null || tabNames[index] == null)
            {
                Debug.LogError($"StatusPanelView requires character tab {index + 1}.", this);

                return;
            }
        }

        foreach (Label label in new[] { level, characterClass, health, mana, experience, stamina, intellect })
        {
            label.enableRichText = false;
        }

        foreach (Label label in tabNames)
        {
            label.enableRichText = false;
        }

        portrait.scaleMode = ScaleMode.ScaleToFit;
        portraitHost.RegisterCallback<GeometryChangedEvent>(ResizePortrait);
        IsReady = true;
        screenInstance.style.display = IsShown ? DisplayStyle.Flex : DisplayStyle.None;
        Refresh();
        ResizePortrait(null);

        if (IsShown)
        {
            FocusSelectedTab();
        }
    }

    public void Show(bool resetSelection)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (resetSelection)
        {
            selectedIndex = 0;
            selectedMember = null;
        }

        IsShown = true;

        if (IsReady)
        {
            screenInstance.style.display = DisplayStyle.Flex;
            Refresh();
            FocusSelectedTab();
        }
    }

    public void Hide()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        IsShown = false;

        foreach (VisualElement tab in tabs)
        {
            tab?.Blur();
        }

        if (screenInstance != null)
        {
            screenInstance.style.display = DisplayStyle.None;
        }
    }

    public void MoveSelection(int direction)
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || partyService == null
            || partyService.Members == null || partyService.Members.Count == 0 || direction == 0)
        {

            return;
        }

        int count = partyService.Members.Count;
        selectedIndex = (selectedIndex + (direction > 0 ? 1 : count - 1)) % count;
        selectedMember = partyService.Members[selectedIndex];
        Refresh();
        FocusSelectedTab();
    }

    private void Refresh()
    {
        if (!isActiveAndEnabled || !IsReady)
        {

            return;
        }

        int count = partyService != null && partyService.Members != null ? partyService.Members.Count : 0;
        int previousIndex = selectedIndex;
        PartyMember previousMember = selectedMember;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, count - 1));

        // Preserve the selected character if another member is removed before it.
        for (int index = 0; index < count; index++)
        {
            if (ReferenceEquals(partyService.Members[index], selectedMember))
            {
                selectedIndex = index;
                break;
            }
        }

        selectedMember = count > 0 ? partyService.Members[selectedIndex] : null;

        for (int index = 0; index < tabs.Length; index++)
        {
            bool occupied = index < count;
            tabNames[index].text = occupied ? partyService.Members[index].Name : string.Empty;
            tabs[index].SetEnabled(occupied);
            tabs[index].focusable = occupied;
            tabs[index].EnableInClassList("status-tab-selected", occupied && index == selectedIndex);
        }

        portrait.sprite = selectedMember?.Portrait;
        level.text = selectedMember == null ? string.Empty : GameLocalization.Text("status.level", selectedMember.Level);
        characterClass.text = selectedMember == null ? string.Empty : GameLocalization.Text("status.class", selectedMember.ClassName);
        health.text = selectedMember == null ? string.Empty : GameLocalization.Text("status.health", selectedMember.CurrentHealth, selectedMember.MaximumHealth);
        mana.text = selectedMember == null ? string.Empty : GameLocalization.Text("status.mana", selectedMember.CurrentMana, selectedMember.MaximumMana);
        experience.text = selectedMember == null ? string.Empty
            : GameLocalization.Text("status.experience", selectedMember.Experience, selectedMember.ExperienceToNextLevel);
        stamina.text = selectedMember == null ? string.Empty
            : GameLocalization.Text("status.stamina", selectedMember.CurrentStamina, selectedMember.BaseStamina);
        intellect.text = selectedMember == null ? string.Empty
            : GameLocalization.Text("status.intellect", selectedMember.CurrentIntellect, selectedMember.BaseIntellect);

        if (IsShown && (previousIndex != selectedIndex || !ReferenceEquals(previousMember, selectedMember)))
        {
            FocusSelectedTab();
        }
    }

    private void FocusSelectedTab()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (!isActiveAndEnabled || !IsShown || !IsReady || selectedMember == null)
        {

            return;
        }

        tabs[selectedIndex].Focus();
        pendingFocus = screenInstance.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsShown && IsReady && selectedMember != null
                && tabs[selectedIndex].enabledInHierarchy)
            {
                tabs[selectedIndex].Focus();
            }
        });
    }

    private void ResizePortrait(GeometryChangedEvent change)
    {
        float width = portraitHost.contentRect.width - portrait.resolvedStyle.marginLeft - portrait.resolvedStyle.marginRight;
        float height = portraitHost.contentRect.height - portrait.resolvedStyle.marginTop - portrait.resolvedStyle.marginBottom;
        float size = Mathf.Max(0f, Mathf.Min(width, height));
        portrait.style.width = size;
        portrait.style.height = size;
    }

    private void UnbindUI()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (portraitHost != null)
        {
            portraitHost.UnregisterCallback<GeometryChangedEvent>(ResizePortrait);
        }

        foreach (VisualElement tab in tabs)
        {
            tab?.Blur();
        }

        System.Array.Clear(tabs, 0, tabs.Length);
        System.Array.Clear(tabNames, 0, tabNames.Length);
        screenInstance = null;
        portraitHost = null;
        IsReady = false;
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= Refresh;

        if (partyService != null)
        {
            partyService.Changed -= Refresh;
        }

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        partyService = null;
        selectedMember = null;
        UnbindUI();
    }
}
