using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class DialogView : MonoBehaviour
{
    private PanelRenderer panelRenderer;

    private VisualElement dialogBox;

    private Label dialogLabel;
    private UIService uiService;
    private string currentKey;
    public bool IsOpen { get; private set; }
    public bool IsReady => dialogBox != null && dialogLabel != null;
    public event Action Hidden;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += RefreshText;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= RefreshText;

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        dialogBox = null;
        dialogLabel = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        dialogBox = rootElement.Q<VisualElement>("DialogBox");
        dialogLabel = rootElement.Q<Label>("DialogText");

        if (!IsReady)
        {
            Hide();

            return;
        }

        dialogLabel.enableRichText = false;
        RefreshText();
        dialogBox.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public bool Show(string dialogKey)
    {
        if (!isActiveAndEnabled || !IsReady || UIService.Instance == null || !GameLocalization.IsReady
            || string.IsNullOrWhiteSpace(dialogKey))
        {

            return false;
        }

        currentKey = dialogKey;
        RefreshText();
        dialogBox.style.display = DisplayStyle.Flex;
        IsOpen = true;
        uiService = UIService.Instance;
        uiService.Register(this);

        return IsOpen;
    }

    public void Hide()
    {
        currentKey = null;

        if (dialogBox != null)
        {
            dialogBox.style.display = DisplayStyle.None;
        }

        if (!IsOpen)
        {

            return;
        }

        IsOpen = false;

        if (uiService != null)
        {
            uiService.Unregister(this);
        }

        uiService = null;
        Hidden?.Invoke();
    }

    private void RefreshText()
    {
        if (isActiveAndEnabled && IsReady)
        {
            dialogLabel.text = GameLocalization.Text(currentKey);
        }
    }
}
