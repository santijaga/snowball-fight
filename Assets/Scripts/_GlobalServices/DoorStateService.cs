using System;
using System.Collections.Generic;
using UnityEngine;

// Session database. Future Save/Load must persist these records together with inventory.
public sealed class DoorStateService : MonoBehaviour
{
    [Serializable]
    public sealed class State
    {
        [SerializeField] private bool locked;
        [SerializeField] private bool opened;

        public bool Locked => locked;
        public bool Opened => opened;

        internal State(bool initiallyLocked)
        {
            locked = initiallyLocked;
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
        }
    }

    public static DoorStateService Instance { get; private set; }
    private readonly Dictionary<string, State> states = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Door> owners = new(StringComparer.Ordinal);
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
            new GameObject(nameof(DoorStateService)).AddComponent<DoorStateService>();
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

    public bool TryBind(string id, Door owner, bool locked, out State state)
    {
        state = null;

        if (!isActiveAndEnabled || Instance != this || string.IsNullOrWhiteSpace(id) || owner == null)
        {

            return false;
        }

        if (owners.TryGetValue(id, out Door existing) && existing != null && existing != owner)
        {
            Debug.LogError($"Duplicate Door Id '{id}'. Assign a unique Door Id to each instance.", owner);

            return false;
        }

        bool firstBinding = existing != owner;
        owners[id] = owner;

        if (!states.TryGetValue(id, out state))
        {
            state = new State(locked);
            states.Add(id, state);
        }
        else if (firstBinding && owner.ResetOpenOnSceneReload)
        {
            state.Close();
        }

        return true;
    }

    public void Unbind(string id, Door owner)
    {
        if (id != null && owners.TryGetValue(id, out Door existing) && existing == owner)
        {
            owners.Remove(id);
        }
    }

    public bool TryOpen(string id, Door owner, ItemDefinition unlockItem, bool consumeUnlockItem)
    {
        if (!isActiveAndEnabled || Instance != this || owner == null || !owner.isActiveAndEnabled
            || !owners.TryGetValue(id, out Door existing) || existing != owner
            || !states.TryGetValue(id, out State state) || state.Opened
            || (state.Locked && unlockItem == null))
        {

            return false;
        }

        if (!state.Locked)
        {
            state.Open();

            return true;
        }

        InventoryService inventory = InventoryService.Instance;

        return inventory != null && inventory.TryExchange(null, 0, unlockItem, consumeUnlockItem, state.Open);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
