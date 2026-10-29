using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogService : MonoBehaviour
{
    [SerializeField] private DialogView dialogView;
    public static DialogService Instance { get; private set; }
    private IDisposable dialogContextHandle;
    private Action onDialogCompleted;
    private List<DialogSequence.Line> sequence;
    private DialogSequence.StartMode sequenceStartMode;
    private int lineIndex;
    private int openedFrame;
    private Action<bool> sequenceCompletion;
    private bool sequenceReachedEnd;
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
        if (IsDialogOpened && (sequence == null || Time.frameCount > openedFrame)
            && InputContextService.Instance != null
            && InputContextService.Instance.Actions.Dialog.Continue.WasPressedThisFrame())
        {
            ContinueDialog();
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

    // dialogKey refers to an entry in the official GameText String Tables.
    public bool TryShowDialog(string dialogKey, Action onCompleted = null, Func<object[]> arguments = null)
    {
        if (CantShowDialog() || !GameLocalization.IsReady || string.IsNullOrWhiteSpace(dialogKey))
        {

            return false;
        }

        opening = true;
        IsDialogOpened = true;
        openedFrame = Time.frameCount;
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

            if (!dialogView.Show(dialogKey, arguments))
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

    // Only advancing past the final line reports success; hiding cancels the sequence.
    public bool TryShowSequence(DialogSequence value, Action<bool> onFinished, DialogSequence introduction = null,
        DialogSequence.StartMode startMode = DialogSequence.StartMode.PlayerInteraction)
    {
        if (CantShowDialog() || !GameLocalization.IsReady || value == null || !value.IsValid
            || (introduction != null && !introduction.IsValid))
        {

            return false;
        }

        var lines = new List<DialogSequence.Line>();
        AppendLines(lines, introduction);
        AppendLines(lines, value);
        int firstLine = FindNextLine(lines, 0, startMode);

        if (firstLine == lines.Count)
        {

            return false;
        }

        sequence = lines;
        sequenceStartMode = startMode;
        lineIndex = firstLine;
        sequenceReachedEnd = false;
        sequenceCompletion = onFinished;

        bool shown = TryShowDialog("dialog.sequence.line", FinishSequence, GetLineArguments);

        if (!shown && sequence != null)
        {
            FinishSequence();
        }

        return shown;
    }

    private static void AppendLines(List<DialogSequence.Line> target, DialogSequence source)
    {
        if (source == null)
        {

            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source.GetLine(i));
        }
    }

    private static int FindNextLine(List<DialogSequence.Line> lines, int start, DialogSequence.StartMode startMode)
    {
        while (start < lines.Count && !lines[start].CanPlay(startMode))
        {
            start++;
        }

        return start;
    }

    public void CancelSequence(Action<bool> owner)
    {
        if (owner != null && sequenceCompletion == owner)
        {
            HideDialog();
        }
    }

    private object[] GetLineArguments()
    {
        DialogSequence.Line line = sequence[lineIndex];

        return new object[] { GameLocalization.Text(line.SpeakerKey), GameLocalization.Text(line.TextKey) };
    }

    private void ContinueDialog()
    {
        int nextLine = sequence == null ? 0 : FindNextLine(sequence, lineIndex + 1, sequenceStartMode);

        if (sequence != null && nextLine < sequence.Count)
        {
            lineIndex = nextLine;

            if (!dialogView.Show("dialog.sequence.line", GetLineArguments))
            {
                HideDialog();
            }

            return;
        }

        sequenceReachedEnd = sequence != null;
        HideDialog();
    }

    private void FinishSequence()
    {
        Action<bool> finished = sequenceCompletion;
        bool reachedEnd = sequenceReachedEnd;
        sequence = null;
        sequenceCompletion = null;
        sequenceReachedEnd = false;
        lineIndex = 0;
        finished?.Invoke(reachedEnd);
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
