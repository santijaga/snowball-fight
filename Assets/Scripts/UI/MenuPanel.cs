using System;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-10)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
[RequireComponent(typeof(StatusPanelView))]
[RequireComponent(typeof(SettingsPanelView))]
[RequireComponent(typeof(InventoryPanelView))]
[RequireComponent(typeof(ControlsPanelView))]
[RequireComponent(typeof(EquipmentPanelView))]
public sealed class MenuPanel : MonoBehaviour
{
    private enum Screen { Main, Inventory, Equipment, Status, Settings, Controls }

    private PanelRenderer panelRenderer;
    private VisualElement menuBox;
    private VisualElement mainMenu;
    private readonly VisualElement[] menuItems = new VisualElement[5];
    private static bool startupControlsDismissed;
    private bool startupControls;
    private ControlsPanelView controlsView;
    private Label controlsLabel;
    private Label inventoryLabel;
    private InventoryPanelView inventoryView;
    private EquipmentPanelView equipmentView;
    private Label equipmentLabel;
    private Label statusLabel;
    private Label settingsLabel;
    private StatusPanelView statusView;
    private SettingsPanelView settingsView;
    private IVisualElementScheduledItem pendingFocus;
    private InputContextService inputService;
    private UIService uiService;
    private IDisposable contextHandle;
    private bool changingState;
    private Screen screen;
    private int selectedItem;

    public bool IsOpen { get; private set; }
    public bool IsInventoryOpen => IsOpen && screen == Screen.Inventory;
    public bool IsEquipmentOpen => IsOpen && screen == Screen.Equipment;
    public bool IsStatusOpen => IsOpen && screen == Screen.Status;
    public bool IsSettingsOpen => IsOpen && screen == Screen.Settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStartupControls()
    {
        startupControlsDismissed = false;
    }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        inventoryView = GetComponent<InventoryPanelView>();
        equipmentView = GetComponent<EquipmentPanelView>();
        statusView = GetComponent<StatusPanelView>();
        settingsView = GetComponent<SettingsPanelView>();
        controlsView = GetComponent<ControlsPanelView>();
        GameLocalization.Changed += RefreshLabels;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindUI();
        menuBox = rootElement?.Q<VisualElement>("MenuBox");
        mainMenu = menuBox?.Q<VisualElement>("MainMenu");
        menuItems[0] = mainMenu?.Q<VisualElement>("InventoryMenuItem");
        inventoryLabel = menuItems[0]?.Q<Label>("InventoryMenuLabel");
        menuItems[1] = mainMenu?.Q<VisualElement>("EquipmentMenuItem");
        equipmentLabel = menuItems[1]?.Q<Label>("EquipmentMenuLabel");
        menuItems[2] = mainMenu?.Q<VisualElement>("StatusMenuItem");
        menuItems[3] = mainMenu?.Q<VisualElement>("SettingsMenuItem");
        statusLabel = menuItems[2]?.Q<Label>("StatusMenuLabel");
        settingsLabel = menuItems[3]?.Q<Label>("SettingsMenuLabel");
        menuItems[4] = mainMenu?.Q<VisualElement>("ControlsMenuItem");
        controlsLabel = menuItems[4]?.Q<Label>("ControlsMenuLabel");

        if (menuBox == null || mainMenu == null || inventoryLabel == null || equipmentLabel == null || statusLabel == null || settingsLabel == null || controlsLabel == null)
        {
            Debug.LogError("MenuPanel requires MenuBox, MainMenu and all menu items with labels.", this);
            Close();

            return;
        }

        inventoryLabel.enableRichText = false;
        equipmentLabel.enableRichText = false;
        statusLabel.enableRichText = false;
        settingsLabel.enableRichText = false;
        controlsLabel.enableRichText = false;
        RefreshLabels();
        // Ignore every descendant: ignoring only the parent would still allow child picking.
        menuBox.pickingMode = PickingMode.Ignore;
        menuBox.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);
        menuBox.RegisterCallback<NavigationMoveEvent>(BlockNavigation, TrickleDown.TrickleDown);
        menuBox.RegisterCallback<NavigationSubmitEvent>(BlockSubmit, TrickleDown.TrickleDown);
        menuBox.RegisterCallback<NavigationCancelEvent>(BlockCancel, TrickleDown.TrickleDown);
        menuBox.RegisterCallback<KeyDownEvent>(BlockKeyboardEvent, TrickleDown.TrickleDown);
        menuBox.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
        mainMenu.style.display = screen == Screen.Main ? DisplayStyle.Flex : DisplayStyle.None;

        if (IsOpen)
        {
            RestoreScreen();
        }
    }

    private void RefreshLabels()
    {
        if (isActiveAndEnabled && inventoryLabel != null && equipmentLabel != null && statusLabel != null && settingsLabel != null && controlsLabel != null)
        {
            inventoryLabel.text = GameLocalization.Text("menu.inventory");
            equipmentLabel.text = GameLocalization.Text("menu.equipment");
            statusLabel.text = GameLocalization.Text("menu.status");
            settingsLabel.text = GameLocalization.Text("menu.settings");
            controlsLabel.text = GameLocalization.Text("menu.controls");
        }
    }

    private void RestoreScreen()
    {
        controlsView.Hide();
        equipmentView.Hide();

        if (screen == Screen.Equipment)
        {
            inventoryView.Hide();
            statusView.Hide();
            settingsView.Hide();
            equipmentView.Show(false);
        }
        else if (screen == Screen.Inventory)
        {
            statusView.Hide();
            settingsView.Hide();
            inventoryView.Show(false);
        }
        else if (screen == Screen.Status)
        {
            inventoryView.Hide();
            settingsView.Hide();
            statusView.Show(false);
        }
        else if (screen == Screen.Settings)
        {
            inventoryView.Hide();
            statusView.Hide();
            settingsView.Show();
        }
        else if (screen == Screen.Controls)
        {
            inventoryView.Hide();
            statusView.Hide();
            settingsView.Hide();
            controlsView.Show(false);
        }
        else
        {
            inventoryView.Hide();
            statusView.Hide();
            settingsView.Hide();
            FocusSelectedItem();
        }
    }

    private static void BlockNavigation(NavigationMoveEvent navigation)
    {
        navigation.StopImmediatePropagation();
    }

    private static void BlockKeyboardEvent(KeyDownEvent key)
    {
        key.StopImmediatePropagation();
    }

    private static void BlockSubmit(NavigationSubmitEvent submit)
    {
        submit.StopImmediatePropagation();
        submit.PreventDefault();
    }

    private static void BlockCancel(NavigationCancelEvent cancel)
    {
        cancel.StopImmediatePropagation();
        cancel.PreventDefault();
    }

    private void FocusSelectedItem()
    {
        if (!isActiveAndEnabled || !IsOpen || screen != Screen.Main || menuItems[selectedItem] == null)
        {

            return;
        }

        pendingFocus?.Pause();
        menuItems[selectedItem].Focus();
        pendingFocus = menuBox.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsOpen && screen == Screen.Main
                && menuItems[selectedItem] != null && menuItems[selectedItem].enabledInHierarchy)
            {
                menuItems[selectedItem].Focus();
            }
        });
    }

    private bool ViewsReady => inventoryView != null && inventoryView.isActiveAndEnabled && inventoryView.IsReady
        && equipmentView != null && equipmentView.isActiveAndEnabled && equipmentView.IsReady
        && statusView != null && statusView.isActiveAndEnabled && statusView.IsReady
        && settingsView != null && settingsView.isActiveAndEnabled && settingsView.IsReady
        && controlsView != null && controlsView.isActiveAndEnabled && controlsView.IsReady;

    private void OpenSelectedScreen()
    {
        if (!isActiveAndEnabled || !IsOpen || screen != Screen.Main || !ViewsReady)
        {

            return;
        }

        pendingFocus?.Pause();
        pendingFocus = null;
        menuItems[selectedItem].Blur();
        mainMenu.style.display = DisplayStyle.None;

        if (selectedItem == 0)
        {
            screen = Screen.Inventory;
            inventoryView.Show(true);
        }
        else if (selectedItem == 1)
        {
            screen = Screen.Equipment;
            equipmentView.Show(true);
        }
        else if (selectedItem == 2)
        {
            screen = Screen.Status;
            statusView.Show(true);
        }
        else if (selectedItem == 3)
        {
            screen = Screen.Settings;
            settingsView.Show();
        }
        else
        {
            screen = Screen.Controls;
            controlsView.Show();
        }
    }

    private void Back()
    {
        if (startupControls)
        {
            startupControlsDismissed = true;
            Close();

            return;
        }

        if (screen == Screen.Inventory && inventoryView.BackToTabs())
        {

            return;
        }

        if (screen == Screen.Equipment && equipmentView.BackToTabs())
        {

            return;
        }

        if (screen != Screen.Main)
        {
            inventoryView.Hide();
            statusView.Hide();
            settingsView.Hide();
            controlsView.Hide();
            equipmentView.Hide();
            screen = Screen.Main;
            mainMenu.style.display = DisplayStyle.Flex;
            FocusSelectedItem();
        }
        else
        {
            Close();
        }
    }

    private void UnbindUI()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (menuBox != null)
        {
            menuBox.UnregisterCallback<NavigationMoveEvent>(BlockNavigation, TrickleDown.TrickleDown);
            menuBox.UnregisterCallback<NavigationSubmitEvent>(BlockSubmit, TrickleDown.TrickleDown);
            menuBox.UnregisterCallback<NavigationCancelEvent>(BlockCancel, TrickleDown.TrickleDown);
            menuBox.UnregisterCallback<KeyDownEvent>(BlockKeyboardEvent, TrickleDown.TrickleDown);
        }

        foreach (VisualElement item in menuItems)
        {
            item?.Blur();
        }

        Array.Clear(menuItems, 0, menuItems.Length);
        inventoryLabel = null;
        equipmentLabel = null;
        statusLabel = null;
        settingsLabel = null;
        controlsLabel = null;
        mainMenu = null;
    }

    private void Update()
    {
        if (IsOpen)
        {
            if (panelRenderer == null || !panelRenderer.enabled || inputService == null
                || !inputService.isActiveAndEnabled || uiService == null || !ViewsReady || !GameLocalization.IsReady)
            {
                Close();

                return;
            }

            if (inputService.CurrentContext == InputContext.UI)
            {
                if (inputService.Actions.UI.Back.WasPressedThisFrame())
                {
                    Back();
                }
                else if (screen == Screen.Controls)
                {
                    if (inputService.Actions.UI.Select.WasPressedThisFrame())
                    {
                        if (controlsView.Select())
                        {
                            Back();
                        }
                    }
                    else if (inputService.Actions.UI.Left.WasPressedThisFrame())
                    {
                        controlsView.MoveSelection(-1, 0);
                    }
                    else if (inputService.Actions.UI.Right.WasPressedThisFrame())
                    {
                        controlsView.MoveSelection(1, 0);
                    }
                    else if (inputService.Actions.UI.Up.WasPressedThisFrame())
                    {
                        controlsView.MoveSelection(0, -1);
                    }
                    else if (inputService.Actions.UI.Down.WasPressedThisFrame())
                    {
                        controlsView.MoveSelection(0, 1);
                    }
                }
                else if (screen == Screen.Inventory)
                {
                    if (inputService.Actions.UI.Select.WasPressedThisFrame())
                    {
                        inventoryView.Select();
                    }
                    else if (inputService.Actions.UI.Left.WasPressedThisFrame())
                    {
                        inventoryView.MoveSelection(-1, 0);
                    }
                    else if (inputService.Actions.UI.Right.WasPressedThisFrame())
                    {
                        inventoryView.MoveSelection(1, 0);
                    }
                    else if (inputService.Actions.UI.Up.WasPressedThisFrame())
                    {
                        inventoryView.MoveSelection(0, -1);
                    }
                    else if (inputService.Actions.UI.Down.WasPressedThisFrame())
                    {
                        inventoryView.MoveSelection(0, 1);
                    }
                }
                else if (screen == Screen.Equipment)
                {
                    if (inputService.Actions.UI.Select.WasPressedThisFrame())
                    {
                        equipmentView.Select();
                    }
                    else if (inputService.Actions.UI.Left.WasPressedThisFrame())
                    {
                        equipmentView.MoveSelection(-1, 0);
                    }
                    else if (inputService.Actions.UI.Right.WasPressedThisFrame())
                    {
                        equipmentView.MoveSelection(1, 0);
                    }
                    else if (inputService.Actions.UI.Up.WasPressedThisFrame())
                    {
                        equipmentView.MoveSelection(0, -1);
                    }
                    else if (inputService.Actions.UI.Down.WasPressedThisFrame())
                    {
                        equipmentView.MoveSelection(0, 1);
                    }
                }
                else if (screen == Screen.Status)
                {
                    if (inputService.Actions.UI.Left.WasPressedThisFrame())
                    {
                        statusView.MoveSelection(-1);
                    }
                    else if (inputService.Actions.UI.Right.WasPressedThisFrame())
                    {
                        statusView.MoveSelection(1);
                    }
                }
                else if (screen == Screen.Settings)
                {
                    if (inputService.Actions.UI.Left.WasPressedThisFrame())
                    {
                        settingsView.MoveLanguage(-1);
                    }
                    else if (inputService.Actions.UI.Right.WasPressedThisFrame())
                    {
                        settingsView.MoveLanguage(1);
                    }
                    else if (inputService.Actions.UI.Up.WasPressedThisFrame()
                        || inputService.Actions.UI.Down.WasPressedThisFrame())
                    {
                        settingsView.FocusLanguage();
                    }
                }
                else if (inputService.Actions.UI.Select.WasPressedThisFrame())
                {
                    OpenSelectedScreen();
                }
                else if (inputService.Actions.UI.Up.WasPressedThisFrame()
                    || inputService.Actions.UI.Down.WasPressedThisFrame())
                {
                    int direction = inputService.Actions.UI.Up.WasPressedThisFrame() ? -1 : 1;
                    selectedItem = (selectedItem + direction + menuItems.Length) % menuItems.Length;
                    FocusSelectedItem();
                }
            }

            return;
        }

        if (!startupControlsDismissed)
        {
            Open(true);

            return;
        }

        InputContextService input = InputContextService.Instance;

        if (input != null && input.isActiveAndEnabled
            && input.CurrentContext == InputContext.Player
            && input.Actions.Player.OpenMenu.WasPressedThisFrame())
        {
            Open();
        }
    }

    public void Open()
    {
        Open(false);
    }

    private void Open(bool showStartupControls)
    {
        if (!isActiveAndEnabled || IsOpen || changingState || menuBox == null || mainMenu == null
            || !ViewsReady || !GameLocalization.IsReady || panelRenderer == null || !panelRenderer.enabled
            || InputContextService.Instance == null || !InputContextService.Instance.isActiveAndEnabled
            || InputContextService.Instance.CurrentContext != InputContext.Player
            || UIService.Instance == null || UIService.Instance.HasOpenUI)
        {

            return;
        }

        changingState = true;
        IsOpen = true;
        screen = Screen.Main;
        selectedItem = 0;
        startupControls = showStartupControls;
        controlsView.Hide();
        equipmentView.Hide();
        inventoryView.Hide();
        statusView.Hide();
        settingsView.Hide();
        mainMenu.style.display = DisplayStyle.Flex;
        inputService = InputContextService.Instance;
        uiService = UIService.Instance;
        try
        {
            IDisposable context = inputService.PushContext(InputContext.UI);

            if (!IsOpen || !isActiveAndEnabled)
            {
                context.Dispose();

                return;
            }

            contextHandle = context;
            menuBox.style.display = DisplayStyle.Flex;
            uiService.Register(this);

            if (showStartupControls && IsOpen && isActiveAndEnabled)
            {
                screen = Screen.Controls;
                mainMenu.style.display = DisplayStyle.None;
                controlsView.Show();
            }
            else
            {
                FocusSelectedItem();
            }
        }
        catch
        {
            Close();
            throw;
        }
        finally
        {
            changingState = false;
        }
    }

    public void Close()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        foreach (VisualElement item in menuItems)
        {
            item?.Blur();
        }

        equipmentView?.Hide();
        inventoryView?.Hide();
        statusView?.Hide();
        settingsView?.Hide();
        controlsView?.Hide();
        startupControls = false;
        screen = Screen.Main;

        if (menuBox != null)
        {
            menuBox.style.display = DisplayStyle.None;
        }

        IsOpen = false;
        IDisposable context = contextHandle;
        UIService registeredService = uiService;
        contextHandle = null;
        inputService = null;
        uiService = null;
        bool wasChangingState = changingState;
        changingState = true;
        try
        {
            if (registeredService != null)
            {
                registeredService.Unregister(this);
            }
        }
        finally
        {
            try
            {
                context?.Dispose();
            }
            finally
            {
                changingState = wasChangingState;
            }
        }
    }

    private void OnDisable()
    {
        UnbindUI();
        Close();
        GameLocalization.Changed -= RefreshLabels;

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        menuBox = null;
    }
}
