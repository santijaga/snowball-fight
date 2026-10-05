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
    private static void CreateInventory()
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

        if (database == null || !database.IsValid()
            || !database.TryGet("potion", out ItemDefinition potion)
            || !database.TryGet("long_sword", out ItemDefinition sword)
            || !database.TryGet("bronze_key", out ItemDefinition key))
        {
            Debug.LogError("InventoryService requires a valid Inventory/Items database with starting items.", this);
            Destroy(gameObject);

            return;
        }

        readOnlyStacks = stacks.AsReadOnly();
        stacks.Add(new Stack(potion, 5));
        stacks.Add(new Stack(sword, 1));
        stacks.Add(new Stack(key, 1));
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Quantity zero removes the stack. New stacks are appended in acquisition order.
    public bool SetQuantity(string itemId, int quantity)
    {
        if (!isActiveAndEnabled || Instance != this || quantity < 0
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Changed?.Invoke();
        }
    }
}
