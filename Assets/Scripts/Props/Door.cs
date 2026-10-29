using System;
using UnityEngine;

public class Door : MonoBehaviour, IPlayerVisible, IInteractable
{
    [Tooltip("Unique permanent ID per placed door. Empty uses scene path and hierarchy; assign an explicit ID before Save/Load.")]
    [SerializeField] private string doorId = "";
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField] private GameObject blocker;
    [SerializeField] private bool isLocked;
    [Tooltip("Reset only the open pose when a new instance binds to saved session state. Keeps the lock and remaining contents; disabling/re-enabling this instance does not reset it.")]
    [SerializeField] private bool resetOpenOnSceneReload = true;
    [SerializeField] private ItemDefinition unlockItem;
    [SerializeField] private bool consumeUnlockItem;

    [Header("Locked Door Dialog")]
    [SerializeField] private bool showLockedDialog;
    [SerializeField] private string lockedDialogKey = "";
    [SerializeField] private DialogService lockedDialogService;

    [Header("Unlock Dialog")]
    [SerializeField] private bool showUnlockDialog;
    [SerializeField] private string unlockDialogKey = "dialog.unlock.used_item";

    private string stateId;
    private DoorStateService stateService;
    private DoorStateService.State state;
    private bool hasInteracted;
    private DialogService activeDialogService;
    private Action dialogCompletion;

    public bool ResetOpenOnSceneReload => resetOpenOnSceneReload;
    public bool Locked => state?.Locked ?? isLocked;
    public bool HasInteracted => hasInteracted || (state != null && state.Opened);
    public event Action Interacted;

    public bool IsPlayerVisible { get; private set; }
    public event Action<bool> VisibilityChanged;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && defaultSprite == null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        // Support existing doors; an explicit Inspector reference takes priority.
        if (blocker == null)
        {
            Transform child = transform.Find("Blocker");

            if (child != null)
            {
                blocker = child.gameObject;
            }
        }

        // Cache once so hierarchy changes during play cannot reset the door.
        string hierarchy = "";

        for (Transform node = transform; node != null; node = node.parent)
        {
            hierarchy = "/" + node.GetSiblingIndex() + ":" + node.name + hierarchy;
        }

        stateId = string.IsNullOrWhiteSpace(doorId) ? "scene:" + gameObject.scene.path + hierarchy : "id:" + doorId;
    }

    private void OnEnable()
    {
        BindState();
        Refresh();
        Interacted?.Invoke();
    }

    private bool BindState()
    {
        stateService = DoorStateService.Instance;

        return stateService != null && stateService.TryBind(stateId, this, isLocked, out state);
    }

    public void SetPlayerVisible(bool visible)
    {
        visible &= isActiveAndEnabled;

        if (visible && dialogCompletion == null
            && (UIService.Instance == null || !UIService.Instance.HasOpenUI)
            && BindState() && !state.Locked && !state.Opened)
        {
            if (stateService.TryOpen(stateId, this, null, false))
            {
                Refresh();
                Interacted?.Invoke();
            }
        }

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
            || (UIService.Instance != null && UIService.Instance.HasOpenUI)
            || !BindState() || !state.Locked || state.Opened)
        {

            return false;
        }

        if (stateService.TryOpen(stateId, this, unlockItem, consumeUnlockItem))
        {
            Refresh();
            TryShowUnlockDialog();
            Interacted?.Invoke();

            return true;
        }

        if (!showLockedDialog || string.IsNullOrWhiteSpace(lockedDialogKey))
        {

            return false;
        }

        activeDialogService = lockedDialogService != null ? lockedDialogService : DialogService.Instance;

        if (activeDialogService == null)
        {

            return false;
        }

        dialogCompletion = CompleteDialog;

        if (!activeDialogService.TryShowDialog(lockedDialogKey, dialogCompletion))
        {
            CompleteDialog();

            return false;
        }

        // Opening UI may synchronously disable the door and cancel its dialog.
        if (!isActiveAndEnabled || dialogCompletion == null)
        {

            return false;
        }

        hasInteracted = true;
        Interacted?.Invoke();

        return true;
    }

    private void TryShowUnlockDialog()
    {
        if (!isActiveAndEnabled || !showUnlockDialog || unlockItem == null
            || string.IsNullOrWhiteSpace(unlockDialogKey))
        {

            return;
        }

        activeDialogService = lockedDialogService != null ? lockedDialogService : DialogService.Instance;

        if (activeDialogService == null)
        {

            return;
        }

        string itemNameKey = unlockItem.NameKey;
        dialogCompletion = CompleteDialog;

        if (!activeDialogService.TryShowDialog(unlockDialogKey, dialogCompletion,
            () => new object[] { GameLocalization.Text(itemNameKey) }))
        {
            CompleteDialog();
        }
    }

    private void CompleteDialog()
    {
        dialogCompletion = null;
        activeDialogService = null;
    }

    private void Refresh()
    {
        bool opened = state != null && state.Opened;
        Sprite sprite = opened ? openedSprite : defaultSprite;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (blocker != null)
        {
            blocker.SetActive(!opened);
        }
    }

    private void OnDisable()
    {
        SetPlayerVisible(false);

        if (activeDialogService != null)
        {
            activeDialogService.HideDialog(dialogCompletion);
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
}
