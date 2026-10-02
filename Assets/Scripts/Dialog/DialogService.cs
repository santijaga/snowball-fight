using System;
using UnityEngine;

public class DialogService : MonoBehaviour
{
    [SerializeField] private DialogView dialogView;
    public static DialogService Instance { get; private set; }
    private IDisposable dialogContextHandle;
    private Action onDialogCompleted;
    private bool closing;
    private bool opening;
    public bool IsDialogOpened { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        Instance = this;

        if (dialogView != null)
        {
            dialogView.Hidden += HideDialog;
        }
    }

    private void Update()
    {
        if (IsDialogOpened && InputContextService.Instance != null
            && InputContextService.Instance.Actions.Dialog.Continue.WasPressedThisFrame())
        {
            HideDialog();
        }
    }

    private void OnDisable() => HideDialog();

    private void OnDestroy()
    {
        HideDialog();

        if (dialogView != null)
        {
            dialogView.Hidden -= HideDialog;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryShowDialog(string line, Action onCompleted = null)
    {
        if (CantShowDialog())
        {

            return false;
        }

        opening = true;
        IsDialogOpened = true;
        onDialogCompleted = onCompleted;
        try
        {
            IDisposable context = InputContextService.Instance.PushContext(InputContext.Dialog);

            if (!IsDialogOpened)
            {
                context.Dispose();

                return false;
            }

            dialogContextHandle = context;

            if (!dialogView.Show(line))
            {
                HideDialog();
            }

            return IsDialogOpened;
        }
        catch
        {
            HideDialog();
            throw;
        }
        finally { opening = false; }
    }

    private bool CantShowDialog()
    {

        return !isActiveAndEnabled || Instance != this || IsDialogOpened || closing || opening
            || dialogView == null || !dialogView.IsReady || !dialogView.isActiveAndEnabled
            || UIService.Instance == null
            || InputContextService.Instance == null || !InputContextService.Instance.isActiveAndEnabled;
    }

    // An action may cancel only the dialog it opened.
    public void HideDialog(Action owner)
    {
        if (owner != null && onDialogCompleted == owner)
        {
            HideDialog();
        }
    }

    public void HideDialog()
    {
        if (!IsDialogOpened || closing)
        {

            return;
        }

        closing = true;
        IsDialogOpened = false;
        IDisposable context = dialogContextHandle;
        Action completed = onDialogCompleted;
        dialogContextHandle = null;
        onDialogCompleted = null;

        try
        {
            if (dialogView != null)
            {
                dialogView.Hide();
            }
        }
        finally
        {
            try { context?.Dispose(); }
            finally
            {
                closing = false;
                completed?.Invoke();
            }
        }
    }
}
