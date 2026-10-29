using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryService : MonoBehaviour
{
    public sealed class Stack
    {
        public ItemDefinition Item { get; }
        public int Quantity { get; internal set; }

        internal Stack(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    public static InventoryService Instance { get; private set; }
    [SerializeField] private ItemDatabase database;
    private readonly List<Stack> stacks = new();
    private IReadOnlyList<Stack> readOnlyStacks;

    public ItemDatabase Database => database;
    public IReadOnlyList<Stack> Stacks => readOnlyStacks;
    public event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    internal static void EnsureCreated()
    {
        if (Instance == null)
        {
            new GameObject(nameof(InventoryService)).AddComponent<InventoryService>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        if (database == null)
        {
            database = Resources.Load<ItemDatabase>("Inventory/Items");
        }

        if (database == null || !database.IsValid())
        {
            Debug.LogError("InventoryService requires a valid Inventory/Items database.", this);
            Destroy(gameObject);

            return;
        }

        readOnlyStacks = stacks.AsReadOnly();
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Quantity zero removes the stack. New stacks are appended in acquisition order.
    public bool SetQuantity(string itemId, int quantity)
    {
        if (!isActiveAndEnabled || Instance != this || quantity < GetEquippedQuantity(itemId)
            || !database.TryGet(itemId, out ItemDefinition item))
        {

            return false;
        }

        Stack stack = stacks.Find(candidate => candidate.Item == item);

        if (stack == null && quantity > 0)
        {
            stacks.Add(new Stack(item, quantity));
        }
        else if (stack != null)
        {
            if (stack.Quantity == quantity)
            {

                return true;
            }

            if (quantity == 0)
            {
                stacks.Remove(stack);
            }
            else
            {
                stack.Quantity = quantity;
            }
        }
        else
        {

            return true;
        }

        Changed?.Invoke();

        return true;
    }

    public int GetQuantity(string itemId)
    {

        return stacks.Find(stack => stack.Item.Id == itemId)?.Quantity ?? 0;
    }

    public int GetEquippedQuantity(string itemId)
    {

        return PartyService.Instance != null ? PartyService.Instance.GetEquippedQuantity(itemId) : 0;
    }

    public int GetAvailableQuantity(string itemId)
    {

        return Math.Max(0, GetQuantity(itemId) - GetEquippedQuantity(itemId));
    }

    internal void NotifyEquipmentChanged()
    {
        Changed?.Invoke();
    }

    // Grant a character's whole loadout atomically, then publish the committed party and inventory.
    internal bool TryGrantEquipment(ItemDefinition[] loadout, Action onCommitted)
    {
        if (!isActiveAndEnabled || Instance != this)
        {

            return false;
        }

        Dictionary<ItemDefinition, int> additions = new();

        foreach (ItemDefinition item in loadout)
        {
            if (item == null)
            {
                continue;
            }

            if (!database.TryGet(item.Id, out ItemDefinition known) || known != item)
            {

                return false;
            }

            additions[item] = additions.TryGetValue(item, out int count) ? count + 1 : 1;
        }

        foreach (KeyValuePair<ItemDefinition, int> addition in additions)
        {
            if ((long)GetQuantity(addition.Key.Id) + addition.Value > int.MaxValue)
            {

                return false;
            }
        }

        foreach (KeyValuePair<ItemDefinition, int> addition in additions)
        {
            Stack stack = stacks.Find(candidate => candidate.Item == addition.Key);

            if (stack == null)
            {
                stacks.Add(new Stack(addition.Key, addition.Value));
            }
            else
            {
                stack.Quantity += addition.Value;
            }
        }

        onCommitted();
        Changed?.Invoke();

        return true;
    }

    // Validate both sides before mutation; commit world state before notifying inventory observers.
    internal bool TryExchange(ItemDefinition reward, int quantity, ItemDefinition required, bool consume,
        Action onCommitted)
    {
        if (!isActiveAndEnabled || Instance != this || quantity < 0
            || (quantity > 0 && (reward == null || !database.TryGet(reward.Id, out ItemDefinition knownReward)
                || knownReward != reward))
            || (required != null && (!database.TryGet(required.Id, out ItemDefinition knownRequired)
                || knownRequired != required
                || (consume ? GetAvailableQuantity(required.Id) : GetQuantity(required.Id)) < 1)))
        {

            return false;
        }

        Stack rewardStack = reward == null ? null : stacks.Find(stack => stack.Item == reward);
        Stack costStack = required == null ? null : stacks.Find(stack => stack.Item == required);
        int cost = consume && required != null ? 1 : 0;
        long total = (long)(rewardStack?.Quantity ?? 0) + quantity - (reward == required ? cost : 0);

        if (total > int.MaxValue)
        {

            return false;
        }

        if (cost > 0)
        {
            costStack.Quantity--;
        }

        if (quantity > 0)
        {
            if (rewardStack == null)
            {
                stacks.Add(new Stack(reward, quantity));
            }
            else
            {
                rewardStack.Quantity += quantity;
            }
        }

        if (costStack != null && costStack.Quantity == 0)
        {
            stacks.Remove(costStack);
        }

        onCommitted?.Invoke();

        if (quantity > 0 || cost > 0)
        {
            Changed?.Invoke();
        }

        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Changed?.Invoke();
        }
    }
}
