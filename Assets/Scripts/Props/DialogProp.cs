using System;
using UnityEngine;

// A prop owns its dialog and optional open/closed appearance.
public class DialogProp : MonoBehaviour, IPlayerVisible, IInteractable
{
    [SerializeField, TextArea] private string dialogLine = "";
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite interactedSprite;
    [SerializeField] private bool shouldToggleBack;

    private DialogService dialogService;
    private Action dialogCompletion;

    public bool HasInteracted { get; private set; }
    public bool IsPlayerVisible { get; private set; }
    public event Action Interacted;
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
            || (UIService.Instance != null && UIService.Instance.HasOpenUI))
        {

            return false;
        }

        dialogService = DialogService.Instance;

        if (dialogService == null)
        {

            return false;
        }

        dialogCompletion = CompleteDialog;

        if (!dialogService.TryShowDialog(dialogLine, dialogCompletion))
        {
            CompleteDialog();

            return false;
        }

        // A UI subscriber may disable this object synchronously while opening.
        if (!isActiveAndEnabled || dialogCompletion == null)
        {

            return false;
        }

        HasInteracted = true;

        if (spriteRenderer != null && interactedSprite != null)
        {
            spriteRenderer.sprite = interactedSprite;
        }

        Interacted?.Invoke();

        return true;
    }

    private void OnDisable()
    {
        SetPlayerVisible(false);

        if (dialogService != null)
        {
            dialogService.HideDialog(dialogCompletion);
        }

        CompleteDialog();
    }

    private void CompleteDialog()
    {
        dialogCompletion = null;
        dialogService = null;

        if (shouldToggleBack && spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }
}
