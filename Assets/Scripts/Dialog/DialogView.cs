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
    private string currentText;
    public bool IsOpen { get; private set; }
    public bool IsReady => dialogBox != null && dialogLabel != null;
    public event Action Hidden;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        Hide();

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

        dialogLabel.text = currentText;
        dialogBox.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public bool Show(string text)
    {
        if (!isActiveAndEnabled || !IsReady || UIService.Instance == null)
        {

            return false;
        }

        currentText = text;
        dialogLabel.text = currentText;
        dialogBox.style.display = DisplayStyle.Flex;
        IsOpen = true;
        uiService = UIService.Instance;
        uiService.Register(this);

        return IsOpen;
    }

    public void Hide()
    {
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
}
