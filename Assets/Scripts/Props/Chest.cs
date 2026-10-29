using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class Chest : MonoBehaviour, IPlayerVisible, IInteractable
{
    [Tooltip("Unique permanent ID per placed chest. Empty uses scene path and hierarchy; assign an explicit ID before Save/Load.")]
    [SerializeField] private string chestId = "";
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField] private bool locked;
    [Tooltip("Reset only the open pose when a new instance binds to saved session state. Keeps the lock and remaining contents; disabling/re-enabling this instance does not reset it.")]
    [SerializeField] private bool resetOpenOnSceneReload;
    [SerializeField] private ItemDefinition unlockItem;
    [SerializeField] private bool consumeUnlockItem;
    [SerializeField] private ItemDefinition content;
    [SerializeField, Min(1)] private int quantity = 1;
    [SerializeField] private string foundDialogKey = "dialog.chest.found";
    [SerializeField] private string emptyDialogKey = "dialog.chest.empty";
    [SerializeField] private string lockedDialogKey = "dialog.chest.locked";

    private string stateId;
    private ChestStateService stateService;
    private ChestStateService.State state;
    private DialogService activeDialog;
    private Action dialogCompletion;

    public bool ResetOpenOnSceneReload => resetOpenOnSceneReload;
    public bool Locked => state?.Locked ?? locked;
    public bool HasInteracted => state != null && state.Opened;
    public bool IsPlayerVisible { get; private set; }
    public event Action Interacted;
    public event Action<bool> VisibilityChanged;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (closedSprite == null)
        {
            closedSprite = spriteRenderer.sprite;
        }

        // Cache once: changes to hierarchy while playing must not reset this chest.
        string hierarchy = "";

        for (Transform node = transform; node != null; node = node.parent)
        {
            hierarchy = "/" + node.GetSiblingIndex() + ":" + node.name + hierarchy;
        }

        stateId = string.IsNullOrWhiteSpace(chestId) ? "scene:" + gameObject.scene.path + hierarchy : "id:" + chestId;
    }

    private void OnEnable()
    {
        BindState();
        RefreshSprite();
        Interacted?.Invoke();
    }

    private bool BindState()
    {
        stateService = ChestStateService.Instance;

        return stateService != null && stateService.TryBind(stateId, this, locked, content, quantity, out state);
    }

    public void SetPlayerVisible(bool visible)
    {
        visible &= isActiveAndEnabled;

        if (IsPlayerVisible == visible)
        {

            return;
        }

        IsPlayerVisible = visible;
        VisibilityChanged?.Invoke(visible);
    }

    public bool TryInteract()
    {
        if (!isActiveAndEnabled || dialogCompletion != null
            || (UIService.Instance != null && UIService.Instance.HasOpenUI) || !BindState())
        {

            return false;
        }

        InventoryService inventory = InventoryService.Instance;
        bool cannotUnlock = state.Locked && (unlockItem == null || inventory == null
            || !inventory.isActiveAndEnabled || inventory.GetQuantity(unlockItem.Id) < 1);
        ItemDefinition loot = null;

        if (!cannotUnlock && state.ContentQuantity > 0
            && (inventory == null || !inventory.Database.TryGet(state.ContentItemId, out loot)))
        {
            Debug.LogError($"Chest '{stateId}' has content missing from the inventory database.", this);

            return false;
        }

        string key = cannotUnlock ? lockedDialogKey : loot != null ? foundDialogKey : emptyDialogKey;
        activeDialog = DialogService.Instance;

        if (activeDialog == null)
        {

            return false;
        }

        dialogCompletion = CompleteDialog;
        int lootQuantity = state.ContentQuantity;
        Func<object[]> arguments = loot == null ? null
            : () => new object[] { GameLocalization.Text(loot.NameKey), lootQuantity };

        if (!activeDialog.TryShowDialog(key, dialogCompletion, arguments))
        {
            CompleteDialog();

            return false;
        }

        // Opening UI can synchronously disable this object and cancel its dialog.
        if (!isActiveAndEnabled || dialogCompletion == null)
        {

            return false;
        }

        if (!cannotUnlock && !state.Opened)
        {
            if (!stateService.TryOpen(stateId, this, unlockItem, consumeUnlockItem))
            {
                activeDialog.HideDialog(dialogCompletion);

                return false;
            }

            RefreshSprite();
            Interacted?.Invoke();
        }

        return true;
    }

    private void RefreshSprite()
    {
        Sprite sprite = HasInteracted ? openedSprite : closedSprite;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void OnDisable()
    {
        SetPlayerVisible(false);

        if (activeDialog != null)
        {
            activeDialog.HideDialog(dialogCompletion);
        }

        CompleteDialog();
    }

    private void OnDestroy()
    {
        if (stateService != null)
        {
            stateService.Unbind(stateId, this);
        }
    }

    private void CompleteDialog()
    {
        activeDialog = null;
        dialogCompletion = null;
    }
}
