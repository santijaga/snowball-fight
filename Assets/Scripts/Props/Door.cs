using System;
using UnityEngine;

public class Door : MonoBehaviour, IPlayerVisible, IInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField] private GameObject blocker;
    [SerializeField] private bool isLocked;

    [Header("Locked Door Dialog")]
    [SerializeField] private bool showLockedDialog;
    [SerializeField, TextArea] private string lockedDialogLine = "";
    [SerializeField] private DialogService lockedDialogService;

    private bool isOpened;
    private DialogService activeDialogService;
    private Action dialogCompletion;

    public bool HasInteracted { get; private set; }
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
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void SetPlayerVisible(bool visible)
    {
        visible &= isActiveAndEnabled;

        if (visible && !isLocked && !isOpened)
        {
            isOpened = true;
            Refresh();
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
        if (!isActiveAndEnabled || !isLocked || isOpened || !showLockedDialog
            || string.IsNullOrWhiteSpace(lockedDialogLine) || dialogCompletion != null
            || lockedDialogService == null)
        {

            return false;
        }

        activeDialogService = lockedDialogService;
        dialogCompletion = CompleteDialog;

        if (!activeDialogService.TryShowDialog(lockedDialogLine, dialogCompletion))
        {
            CompleteDialog();

            return false;
        }

        // Opening UI may synchronously disable the door and cancel its dialog.
        if (!isActiveAndEnabled || dialogCompletion == null)
        {

            return false;
        }

        HasInteracted = true;
        Interacted?.Invoke();

        return true;
    }

    private void CompleteDialog()
    {
        dialogCompletion = null;
        activeDialogService = null;
    }

    private void Refresh()
    {
        Sprite sprite = isOpened ? openedSprite : defaultSprite;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (blocker != null)
        {
            blocker.SetActive(!isOpened);
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
}
