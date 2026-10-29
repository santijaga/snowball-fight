using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class ControlsPanelView : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private VisualElement screenInstance;
    private VisualElement continueItem;
    private Label continueLabel;
    private IVisualElementScheduledItem pendingFocus;
    private readonly VisualElement[] tabs = new VisualElement[3];
    private readonly VisualElement[] categories = new VisualElement[3];
    private int selectedTab;
    private bool selectingContinue;
    private InputContextService.DeviceScheme displayedScheme;

    public bool IsShown { get; private set; }
    public bool IsReady { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindUI();
        screenInstance = rootElement?.Q<VisualElement>("ControlsScreenInstance");
        continueItem = screenInstance?.Q<VisualElement>("ControlsContinueItem");
        continueLabel = continueItem?.Q<Label>("controls.continue");
        tabs[0] = screenInstance?.Q<VisualElement>("ControlsGameTab");
        tabs[1] = screenInstance?.Q<VisualElement>("ControlsUITab");
        tabs[2] = screenInstance?.Q<VisualElement>("ControlsDialogTab");
        categories[0] = screenInstance?.Q<VisualElement>("ControlsGameMappings");
        categories[1] = screenInstance?.Q<VisualElement>("ControlsUIMappings");
        categories[2] = screenInstance?.Q<VisualElement>("ControlsDialogMappings");

        if (screenInstance == null || continueItem == null || continueLabel == null
            || screenInstance.Q<Label>("controls.keyboard") == null
            || screenInstance.Q<Label>("controls.gamepad") == null
            || tabs[0] == null || tabs[1] == null || tabs[2] == null
            || categories[0] == null || categories[1] == null || categories[2] == null)
        {
            Debug.LogError("ControlsPanelView requires the complete ControlsScreen template.", this);

            return;
        }

        screenInstance.Query<Label>().ForEach(label => label.enableRichText = false);
        IsReady = true;
        screenInstance.style.display = IsShown ? DisplayStyle.Flex : DisplayStyle.None;
        Refresh();
    }

    private void Refresh()
    {
        if (!isActiveAndEnabled || !IsReady)
        {

            return;
        }

        displayedScheme = InputContextService.Instance != null
            ? InputContextService.Instance.ActiveDeviceScheme
            : InputContextService.DeviceScheme.KeyboardAndMouse;
        bool gamepad = displayedScheme == InputContextService.DeviceScheme.Gamepad;
        screenInstance.Query<Label>().ForEach(label => label.text = GameLocalization.Text(label.name));
        screenInstance.Q<Label>("controls.keyboard").style.display = gamepad ? DisplayStyle.None : DisplayStyle.Flex;
        screenInstance.Q<Label>("controls.gamepad").style.display = gamepad ? DisplayStyle.Flex : DisplayStyle.None;
        screenInstance.Query<Label>(className: "controls-keyboard-binding").ForEach(label =>
            label.style.display = gamepad ? DisplayStyle.None : DisplayStyle.Flex);
        screenInstance.Query<Label>(className: "controls-gamepad-binding").ForEach(label =>
            label.style.display = gamepad ? DisplayStyle.Flex : DisplayStyle.None);
        continueLabel.text = GameLocalization.Text("controls.continue");
        FocusSelection();
    }

    private void Update()
    {
        if (IsShown && IsReady && InputContextService.Instance != null
            && displayedScheme != InputContextService.Instance.ActiveDeviceScheme)
        {
            Refresh();
        }
    }

    public void Show(bool resetSelection = true)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (resetSelection)
        {
            selectedTab = 0;
            selectingContinue = false;
        }

        IsShown = true;

        if (IsReady)
        {
            screenInstance.style.display = DisplayStyle.Flex;
            Refresh();
        }
    }

    // True requests the shared exit; selecting a tab only moves focus to Continue.
    public bool Select()
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady)
        {

            return false;
        }

        if (selectingContinue)
        {

            return true;
        }

        selectingContinue = true;
        FocusSelection();

        return false;
    }

    public void MoveSelection(int horizontal, int vertical)
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady)
        {

            return;
        }

        if (vertical != 0)
        {
            selectingContinue = vertical > 0;
        }
        else if (!selectingContinue && horizontal != 0)
        {
            selectedTab = (selectedTab + horizontal + tabs.Length) % tabs.Length;
        }

        FocusSelection();
    }

    private void FocusSelection()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (!IsReady)
        {

            return;
        }

        for (int index = 0; index < tabs.Length; index++)
        {
            tabs[index].EnableInClassList("inventory-tab-selected", index == selectedTab);
            tabs[index].Blur();
            categories[index].style.display = index == selectedTab ? DisplayStyle.Flex : DisplayStyle.None;
        }

        continueItem.Blur();

        if (!isActiveAndEnabled || !IsShown)
        {

            return;
        }

        VisualElement target = selectingContinue ? continueItem : tabs[selectedTab];
        target.Focus();
        pendingFocus = screenInstance.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsShown && IsReady && target.enabledInHierarchy && target.panel != null)
            {
                target.Focus();
            }
        });
    }

    public void Hide()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        IsShown = false;
        continueItem?.Blur();

        foreach (VisualElement tab in tabs)
        {
            tab?.Blur();
        }

        if (screenInstance != null)
        {
            screenInstance.style.display = DisplayStyle.None;
        }
    }

    private void UnbindUI()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        continueItem?.Blur();

        foreach (VisualElement tab in tabs)
        {
            tab?.Blur();
        }

        System.Array.Clear(tabs, 0, tabs.Length);
        System.Array.Clear(categories, 0, categories.Length);
        screenInstance = null;
        continueItem = null;
        continueLabel = null;
        IsReady = false;
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= Refresh;

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        UnbindUI();
    }
}
