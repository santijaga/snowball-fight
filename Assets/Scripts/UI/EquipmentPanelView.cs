using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class EquipmentPanelView : MonoBehaviour
{
    private enum Selection { Tabs, Slots, Items }

    private PanelRenderer panelRenderer;
    private PartyService party;
    private InventoryService inventory;
    private VisualElement screen;
    private VisualElement slotPanel;
    private VisualElement statsPanel;
    private VisualElement portraitHost;
    private Image portrait;
    private ScrollView choices;
    private Label description;
    private readonly VisualElement[] tabs = new VisualElement[PartyRules.MaximumMembers];
    private readonly Label[] names = new Label[PartyRules.MaximumMembers];
    private readonly VisualElement[] slots = new VisualElement[6];
    private readonly Label[] slotNames = new Label[6];
    private readonly Label[] stats = new Label[6];
    private readonly List<ItemDefinition> candidates = new();
    private readonly List<VisualElement> rows = new();
    private static readonly string[] SlotKeys =
        { "weapon", "artifact", "amulet", "ring1", "ring2", "trinket" };
    private PartyMember member;
    private int selectedTab;
    private int selectedSlot;
    private int selectedItem;
    private Selection selection;
    private IVisualElementScheduledItem pendingFocus;

    public bool IsShown { get; private set; }
    public bool IsReady { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        BindServices();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        if (!ReferenceEquals(party, PartyService.Instance) || !ReferenceEquals(inventory, InventoryService.Instance))
        {
            BindServices();
        }
    }

    private void BindServices()
    {
        if (party != null)
        {
            party.Changed -= Refresh;
        }

        if (inventory != null)
        {
            inventory.Changed -= Refresh;
        }

        party = PartyService.Instance;
        inventory = InventoryService.Instance;

        if (party != null)
        {
            party.Changed += Refresh;
        }

        if (inventory != null)
        {
            inventory.Changed += Refresh;
        }

        Refresh();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root)
    {
        UnbindUI();
        screen = root?.Q<VisualElement>("EquipmentScreenInstance");
        slotPanel = screen?.Q<VisualElement>("EquipmentSlots");
        statsPanel = screen?.Q<VisualElement>("EquipmentStats");
        portraitHost = screen?.Q<VisualElement>("EquipmentPortraitHost");
        portrait = screen?.Q<Image>("EquipmentPortrait");
        choices = screen?.Q<ScrollView>("EquipmentChoices");
        description = screen?.Q<Label>("EquipmentDescription");

        if (screen == null || slotPanel == null || statsPanel == null || portraitHost == null || portrait == null
            || choices == null || description == null)
        {
            Debug.LogError("EquipmentPanelView requires the complete EquipmentScreen template.", this);

            return;
        }

        for (int index = 0; index < tabs.Length; index++)
        {
            tabs[index] = screen.Q<VisualElement>($"EquipmentTab{index + 1}");
            names[index] = tabs[index]?.Q<Label>();

            if (tabs[index] == null || names[index] == null)
            {
                Debug.LogError("EquipmentPanelView requires three character tabs.", this);

                return;
            }

            names[index].enableRichText = false;
        }

        for (int index = 0; index < slots.Length; index++)
        {
            slots[index] = screen.Q<VisualElement>($"EquipmentSlot{index}");
            slotNames[index] = slots[index]?.Q<Label>();

            if (slots[index] == null || slotNames[index] == null)
            {
                Debug.LogError("EquipmentPanelView requires six equipment slots.", this);

                return;
            }

            slotNames[index].enableRichText = false;
        }

        for (int index = 0; index < stats.Length; index++)
        {
            stats[index] = screen.Q<Label>($"EquipmentStat{index}");

            if (stats[index] == null)
            {
                Debug.LogError("EquipmentPanelView requires seven stat labels.", this);

                return;
            }

            stats[index].enableRichText = false;
            stats[index].style.color = Color.white;
        }

        description.enableRichText = false;
        choices.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        choices.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        portrait.scaleMode = ScaleMode.ScaleToFit;
        portraitHost.RegisterCallback<GeometryChangedEvent>(ResizePortrait);
        IsReady = true;
        screen.style.display = IsShown ? DisplayStyle.Flex : DisplayStyle.None;
        Refresh();
        ResizePortrait(null);
    }

    public void Show(bool reset)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (reset)
        {
            selection = Selection.Tabs;
            selectedTab = selectedSlot = selectedItem = 0;
            member = null;
        }

        IsShown = true;

        if (IsReady)
        {
            screen.style.display = DisplayStyle.Flex;
            Refresh();
        }
    }

    public void Select()
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || member == null)
        {

            return;
        }

        if (selection == Selection.Tabs)
        {
            selection = Selection.Slots;
        }
        else if (selection == Selection.Slots)
        {
            selection = Selection.Items;
            selectedItem = 0;
            choices.scrollOffset = Vector2.zero;
        }
        else
        {
            ItemDefinition item = candidates[selectedItem];

            if (!party.TryEquip(member, (EquipmentSlot)selectedSlot, item))
            {
                Refresh();

                return;
            }

            selection = Selection.Slots;
        }

        Refresh();
    }

    public bool BackToTabs()
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || selection == Selection.Tabs)
        {

            return false;
        }

        selection = selection == Selection.Items ? Selection.Slots : Selection.Tabs;
        Refresh();

        return true;
    }

    public void MoveSelection(int horizontal, int vertical)
    {
        if (!isActiveAndEnabled || !IsShown || !IsReady || member == null)
        {

            return;
        }

        if (selection == Selection.Tabs)
        {
            int count = party.Members.Count;
            selectedTab = (selectedTab + horizontal + count) % count;
            member = party.Members[selectedTab];
        }
        else if (selection == Selection.Slots)
        {
            int next = selectedSlot + (vertical != 0 ? vertical * 2 : horizontal);

            if (next >= 0 && next < slots.Length && (vertical != 0 || next / 2 == selectedSlot / 2))
            {
                selectedSlot = next;
            }
        }
        else
        {
            selectedItem = Mathf.Clamp(selectedItem + vertical, 0, candidates.Count - 1);
        }

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
        PartyMember previousMember = member;
        int count = party?.Members?.Count ?? 0;
        selectedTab = Mathf.Clamp(selectedTab, 0, Mathf.Max(0, count - 1));

        for (int index = 0; index < count; index++)
        {
            if (ReferenceEquals(party.Members[index], member))
            {
                selectedTab = index;
                break;
            }
        }

        member = count > 0 ? party.Members[selectedTab] : null;

        if (!ReferenceEquals(member, previousMember))
        {
            selection = Selection.Tabs;
            selectedItem = 0;
        }

        for (int index = 0; index < tabs.Length; index++)
        {
            bool occupied = index < count;
            names[index].text = occupied ? party.Members[index].Name : "";
            tabs[index].SetEnabled(occupied);
            tabs[index].EnableInClassList("equipment-tab-selected", occupied && index == selectedTab);
        }

        portrait.sprite = member?.Portrait;

        for (int index = 0; index < slots.Length; index++)
        {
            ItemDefinition item = member?.GetEquipment((EquipmentSlot)index);
            slotNames[index].text = GameLocalization.Text("equipment.slot." + SlotKeys[index]) + ": "
                + (item == null ? GameLocalization.Text("equipment.empty") : GameLocalization.Text(item.NameKey));
            slots[index].SetEnabled(member != null);
        }

        for (int index = 0; index < stats.Length; index++)
        {
            stats[index].text = member == null ? "" : index switch
            {
                0 => GameLocalization.Text("status.health", member.CurrentHealth, member.MaximumHealth),
                1 => GameLocalization.Text("status.mana", member.CurrentMana, member.MaximumMana),
                2 => GameLocalization.Text("status.strength", member.CurrentStrength),
                3 => GameLocalization.Text("status.agility", member.CurrentAgility),
                4 => GameLocalization.Text("status.intellect", member.CurrentIntellect),
                _ => GameLocalization.Text("status.damage", member.Damage)
            };
        }

        ItemDefinition previousItem = selectedItem > 0 && selectedItem < candidates.Count
            ? candidates[selectedItem] : null;
        candidates.Clear();
        rows.Clear();
        choices.Clear();
        candidates.Add(null);

        if (inventory != null && member != null)
        {
            foreach (InventoryService.Stack stack in inventory.Stacks)
            {
                if (stack.Item.Fits((EquipmentSlot)selectedSlot) && inventory.GetAvailableQuantity(stack.Item.Id) > 0)
                {
                    candidates.Add(stack.Item);
                }
            }
        }

        int previousIndex = previousItem == null ? 0 : candidates.IndexOf(previousItem);
        selectedItem = previousIndex >= 0 ? previousIndex : Mathf.Clamp(selectedItem, 0, candidates.Count - 1);

        foreach (ItemDefinition item in candidates)
        {
            VisualElement row = new() { focusable = true, tabIndex = -1, pickingMode = PickingMode.Ignore };
            row.AddToClassList("equipment-choice");
            Label label = new(item == null ? GameLocalization.Text("equipment.remove")
                : GameLocalization.Text(item.NameKey))
            { enableRichText = false, pickingMode = PickingMode.Ignore };
            label.AddToClassList("menu-item-label");
            row.Add(label);
            VisualElement cursor = new() { pickingMode = PickingMode.Ignore };
            cursor.AddToClassList("menu-cursor");
            row.Add(cursor);
            choices.Add(row);
            rows.Add(row);
        }

        slotPanel.style.display = selection == Selection.Items ? DisplayStyle.None : DisplayStyle.Flex;
        choices.style.display = selection == Selection.Items ? DisplayStyle.Flex : DisplayStyle.None;
        ItemDefinition described = selection == Selection.Items ? candidates[selectedItem] : null;
        description.text = described == null ? "" : described.GetDescription();
        description.style.display = selection == Selection.Items ? DisplayStyle.Flex : DisplayStyle.None;
        statsPanel.style.display = selection == Selection.Items ? DisplayStyle.None : DisplayStyle.Flex;
        FocusSelection();
    }

    private void FocusSelection()
    {
        if (!IsShown || member == null)
        {

            return;
        }

        VisualElement target = selection switch
        {
            Selection.Tabs => tabs[selectedTab],
            Selection.Slots => slots[selectedSlot],
            _ => rows[selectedItem]
        };
        target.Focus();
        pendingFocus = screen.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsShown && IsReady && target.panel != null && target.enabledInHierarchy)
            {
                target.Focus();

                if (selection == Selection.Items)
                {
                    choices.ScrollTo(target);
                }
            }
        });
    }

    private void ResizePortrait(GeometryChangedEvent change)
    {
        float size = Mathf.Max(0f, Mathf.Min(portraitHost.contentRect.width, portraitHost.contentRect.height));
        portrait.style.width = size;
        portrait.style.height = size;
    }

    public void Hide()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        IsShown = false;

        if (screen != null)
        {
            screen.Query<VisualElement>().ForEach(element => element.Blur());
            screen.style.display = DisplayStyle.None;
        }
    }

    private void UnbindUI()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (portraitHost != null)
        {
            portraitHost.UnregisterCallback<GeometryChangedEvent>(ResizePortrait);
        }

        IsReady = false;
        screen = null;
        portraitHost = null;
        portrait = null;
        choices = null;
        description = null;
        slotPanel = null;
        statsPanel = null;
        System.Array.Clear(tabs, 0, tabs.Length);
        System.Array.Clear(names, 0, names.Length);
        System.Array.Clear(slots, 0, slots.Length);
        System.Array.Clear(slotNames, 0, slotNames.Length);
        System.Array.Clear(stats, 0, stats.Length);
        rows.Clear();
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= Refresh;

        if (party != null)
        {
            party.Changed -= Refresh;
        }

        if (inventory != null)
        {
            inventory.Changed -= Refresh;
        }

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        party = null;
        inventory = null;
        UnbindUI();
    }
}
