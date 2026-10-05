using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class InventoryPanelView : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private InventoryService inventory;
    private VisualElement screenInstance;
    private readonly VisualElement[] tabs = new VisualElement[2];
    private readonly Label[] tabLabels = new Label[2];
    private ScrollView list;
    private Label description;
    private readonly List<InventoryService.Stack> visibleStacks = new();
    private readonly List<VisualElement> cells = new();
    private IVisualElementScheduledItem pendingFocus;
    private int selectedTab;
    private int selectedItem;
    private bool selectingItems;

    public bool IsShown { get; private set; }
    public bool IsReady { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        InventoryService current = InventoryService.Instance;

        if (!ReferenceEquals(inventory, current))
        {
            if (inventory != null)
            {
                inventory.Changed -= Refresh;
            }

            inventory = current;

            if (inventory != null)
            {
                inventory.Changed += Refresh;
            }

            Refresh();
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindUI();
        screenInstance = rootElement?.Q<VisualElement>("InventoryScreenInstance");
        tabs[0] = screenInstance?.Q<VisualElement>("InventoryTab");
        tabs[1] = screenInstance?.Q<VisualElement>("KeyItemsTab");
        tabLabels[0] = tabs[0]?.Q<Label>();
        tabLabels[1] = tabs[1]?.Q<Label>();
        list = screenInstance?.Q<ScrollView>("InventoryList");
        description = screenInstance?.Q<Label>("InventoryDescription");

        if (screenInstance == null || tabs[0] == null || tabs[1] == null
            || tabLabels[0] == null || tabLabels[1] == null || list == null || description == null)
        {
            Debug.LogError("InventoryPanelView requires the complete InventoryScreen template.", this);

            return;
        }

        description.enableRichText = false;
        tabLabels[0].enableRichText = false;
        tabLabels[1].enableRichText = false;
        list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        list.verticalScrollerVisibility = ScrollerVisibility.Hidden;
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

        pendingFocus?.Pause();
        pendingFocus = null;
        ItemDefinition previous = selectingItems && selectedItem < visibleStacks.Count
            ? visibleStacks[selectedItem].Item : null;
        tabLabels[0].text = GameLocalization.Text("menu.inventory");
        tabLabels[1].text = GameLocalization.Text("inventory.key_items");
        visibleStacks.Clear();
        cells.Clear();
        list.Clear();

        if (inventory != null)
        {
            foreach (InventoryService.Stack stack in inventory.Stacks)
            {
                if ((int)stack.Item.Category == selectedTab)
                {
                    visibleStacks.Add(stack);
                }
            }
        }

        VisualElement row = null;

        for (int i = 0; i < visibleStacks.Count; i++)
        {
            if (i % 2 == 0)
            {
                row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("inventory-row");
                list.Add(row);
            }

            InventoryService.Stack stack = visibleStacks[i];
            VisualElement cell = new() { focusable = true, tabIndex = -1, pickingMode = PickingMode.Ignore };
            cell.AddToClassList("inventory-item");
            VisualElement cursor = new() { pickingMode = PickingMode.Ignore };
            cursor.AddToClassList("menu-cursor");
            cell.Add(cursor);
            Label name = new(GameLocalization.Text(stack.Item.NameKey))
            {
                enableRichText = false, pickingMode = PickingMode.Ignore
            };
            name.AddToClassList("inventory-name");
            cell.Add(name);
            Label quantity = new(stack.Quantity.ToString()) { pickingMode = PickingMode.Ignore };
            quantity.AddToClassList("inventory-quantity");
            cell.Add(quantity);
            row.Add(cell);
            cells.Add(cell);
        }

        int previousIndex = visibleStacks.FindIndex(stack => stack.Item == previous);
        selectedItem = previousIndex >= 0 ? previousIndex : Mathf.Clamp(selectedItem, 0, cells.Count - 1);

        if (cells.Count == 0)
        {
            selectedItem = 0;
            selectingItems = false;
        }

        FocusSelection();
    }

    public void Show(bool reset)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (reset)
        {
            selectedTab = 0;
            selectedItem = 0;
            selectingItems = false;

            if (IsReady)
            {
                list.scrollOffset = Vector2.zero;
            }
        }

        IsShown = true;

        if (IsReady)
        {
            screenInstance.style.display = DisplayStyle.Flex;
            Refresh();
        }
    }

    public void Select()
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || selectingItems || cells.Count == 0)
        {

            return;
        }

        selectingItems = true;
        selectedItem = 0;
        FocusSelection();
    }

    // True consumes Back inside the inventory; false lets the menu return to its sidebar.
    public bool BackToTabs()
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || !selectingItems)
        {

            return false;
        }

        selectingItems = false;
        FocusSelection();

        return true;
    }

    public void MoveSelection(int horizontal, int vertical)
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady)
        {

            return;
        }

        if (!selectingItems)
        {
            if (horizontal != 0)
            {
                selectedTab = 1 - selectedTab;
                selectedItem = 0;
                list.scrollOffset = Vector2.zero;
                Refresh();
            }

            return;
        }

        int next = selectedItem + (vertical != 0 ? vertical * 2 : horizontal);

        if (next >= 0 && next < cells.Count && (vertical != 0 || next / 2 == selectedItem / 2))
        {
            selectedItem = next;
            FocusSelection();
        }
    }

    private void FocusSelection()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (!IsReady)
        {

            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].EnableInClassList("inventory-tab-selected", i == selectedTab);
            tabs[i].Blur();
        }

        foreach (VisualElement cell in cells)
        {
            cell.Blur();
        }

        description.text = selectingItems && cells.Count > 0
            ? GameLocalization.Text(visibleStacks[selectedItem].Item.DescriptionKey) : "";
        description.style.display = selectingItems && cells.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

        if (!isActiveAndEnabled || !IsShown)
        {

            return;
        }

        VisualElement target = selectingItems ? cells[selectedItem] : tabs[selectedTab];
        target.Focus();
        pendingFocus = screenInstance.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsShown && IsReady && target.panel != null)
            {
                target.Focus();

                if (selectingItems)
                {
                    list.ScrollTo(target);
                }
            }
        });
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

        foreach (VisualElement cell in cells)
        {
            cell.Blur();
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
        IsReady = false;
        screenInstance = null;
        list = null;
        description = null;
        System.Array.Clear(tabs, 0, tabs.Length);
        System.Array.Clear(tabLabels, 0, tabLabels.Length);
        cells.Clear();
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= Refresh;

        if (inventory != null)
        {
            inventory.Changed -= Refresh;
        }

        inventory = null;

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        UnbindUI();
    }
}
