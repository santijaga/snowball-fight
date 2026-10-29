using System;
using System.Collections.Generic;
using UnityEngine;

// Session database. Future Save/Load must persist these records together with inventory.
public sealed class ChestStateService : MonoBehaviour
{
    [Serializable]
    public sealed class State
    {
        [SerializeField] private bool locked;
        [SerializeField] private bool opened;
        [SerializeField] private string contentItemId;
        [SerializeField] private int contentQuantity;

        public bool Locked => locked;
        public bool Opened => opened;
        public string ContentItemId => contentItemId;
        public int ContentQuantity => contentQuantity;

        internal State(bool initiallyLocked, ItemDefinition content, int quantity)
        {
            locked = initiallyLocked;
            contentItemId = content == null ? string.Empty : content.Id;
            contentQuantity = content == null ? 0 : Mathf.Max(0, quantity);
        }

        // Closing the pose must not restore the lock or already collected contents.
        internal void Close()
        {
            opened = false;
        }

        internal void Open()
        {
            locked = false;
            opened = true;
            contentItemId = string.Empty;
            contentQuantity = 0;
        }
    }

    public static ChestStateService Instance { get; private set; }
    private readonly Dictionary<string, State> states = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Chest> owners = new(StringComparer.Ordinal);
    private IReadOnlyDictionary<string, State> readOnlyStates;
    public IReadOnlyDictionary<string, State> States => readOnlyStates;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (Instance == null)
        {
            new GameObject(nameof(ChestStateService)).AddComponent<ChestStateService>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        Instance = this;
        readOnlyStates = new System.Collections.ObjectModel.ReadOnlyDictionary<string, State>(states);
        DontDestroyOnLoad(gameObject);
    }

    public bool TryBind(string id, Chest owner, bool locked, ItemDefinition content, int quantity, out State state)
    {
        state = null;

        if (!isActiveAndEnabled || Instance != this || string.IsNullOrWhiteSpace(id) || owner == null)
        {

            return false;
        }

        if (owners.TryGetValue(id, out Chest existing) && existing != null && existing != owner)
        {
            Debug.LogError($"Duplicate Chest Id '{id}'. Assign a unique Chest Id to each instance.", owner);

            return false;
        }

        bool firstBinding = existing != owner;
        owners[id] = owner;

        if (!states.TryGetValue(id, out state))
        {
            state = new State(locked, content, quantity);
            states.Add(id, state);
        }
        else if (firstBinding && owner.ResetOpenOnSceneReload)
        {
            state.Close();
        }

        return true;
    }

    public void Unbind(string id, Chest owner)
    {
        if (id != null && owners.TryGetValue(id, out Chest existing) && existing == owner)
        {
            owners.Remove(id);
        }
    }

    public bool TryOpen(string id, Chest owner, ItemDefinition unlockItem, bool consumeUnlockItem)
    {
        if (!isActiveAndEnabled || Instance != this || owner == null || !owner.isActiveAndEnabled
            || !owners.TryGetValue(id, out Chest existing) || existing != owner
            || !states.TryGetValue(id, out State state) || state.Opened
            || (state.Locked && unlockItem == null))
        {

            return false;
        }

        InventoryService inventory = InventoryService.Instance;
        ItemDefinition reward = null;

        if (inventory == null || (state.ContentQuantity > 0
            && !inventory.Database.TryGet(state.ContentItemId, out reward)))
        {

            return false;
        }

        return inventory.TryExchange(reward, state.ContentQuantity, state.Locked ? unlockItem : null,
            state.Locked && consumeUnlockItem, state.Open);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
