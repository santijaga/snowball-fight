using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class SettingsPanelView : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private VisualElement screenInstance;
    private VisualElement languageRow;
    private Label languageLabel;
    private Label languageValue;
    private IVisualElementScheduledItem pendingFocus;

    public bool IsShown { get; private set; }
    public bool IsReady { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindUI();
        screenInstance = rootElement?.Q<VisualElement>("SettingsScreenInstance");
        languageRow = screenInstance?.Q<VisualElement>("LanguageSetting");
        languageLabel = languageRow?.Q<Label>("LanguageSettingLabel");
        languageValue = languageRow?.Q<Label>("LanguageSettingValue");

        if (screenInstance == null || languageRow == null || languageLabel == null || languageValue == null)
        {
            Debug.LogError("SettingsPanelView requires the complete SettingsScreen template.", this);

            return;
        }

        languageLabel.enableRichText = false;
        languageValue.enableRichText = false;
        IsReady = true;
        screenInstance.style.display = IsShown ? DisplayStyle.Flex : DisplayStyle.None;
        Refresh();

        if (IsShown)
        {
            FocusLanguage();
        }
    }

    private void Refresh()
    {
        if (!isActiveAndEnabled || !IsReady)
        {

            return;
        }

        languageLabel.text = GameLocalization.Text("settings.language");
        languageValue.text = GameLocalization.LanguageName;
    }

    public void Show()
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        IsShown = true;

        if (IsReady)
        {
            screenInstance.style.display = DisplayStyle.Flex;
            Refresh();
            FocusLanguage();
        }
    }

    public void MoveLanguage(int direction)
    {
        if (isActiveAndEnabled && IsShown && IsReady && languageRow.focusController?.focusedElement == languageRow)
        {
            GameLocalization.MoveLanguage(direction);
        }
    }

    public void FocusLanguage()
    {
        pendingFocus?.Pause();
        pendingFocus = null;

        if (!isActiveAndEnabled || !IsShown || !IsReady)
        {

            return;
        }

        languageRow.Focus();
        pendingFocus = screenInstance.schedule.Execute(() =>
        {
            pendingFocus = null;

            if (isActiveAndEnabled && IsShown && IsReady && languageRow.enabledInHierarchy)
            {
                languageRow.Focus();
            }
        });
    }

    public void Hide()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        IsShown = false;
        languageRow?.Blur();

        if (screenInstance != null)
        {
            screenInstance.style.display = DisplayStyle.None;
        }
    }

    private void UnbindUI()
    {
        pendingFocus?.Pause();
        pendingFocus = null;
        languageRow?.Blur();
        screenInstance = null;
        languageRow = null;
        languageLabel = null;
        languageValue = null;
        IsReady = false;
    }

    private void OnDisable()
    {
        Hide();
        GameLocalization.Changed -= Refresh;

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        UnbindUI();
    }
}
