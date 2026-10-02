using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// Reveals the scene through the existing UI Toolkit panel.
[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class SceneRevealScreen : MonoBehaviour, ISceneTransitionScreen
{
    [SerializeField, Min(0f)] private float holdDuration = 0.5f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    private PanelRenderer panelRenderer;
    private VisualElement screenElement;
    private UIService uiService;
    private Coroutine revealRoutine;
    private float opacity;

    public bool IsOpen { get; private set; }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        uiService = UIService.Instance;

        if (uiService == null || !panelRenderer.enabled)
        {
            Debug.LogError("SceneRevealScreen requires a UI service and an enabled panel renderer.", this);

            return;
        }

        IsOpen = true;
        opacity = 1f;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (!IsOpen)
        {

            return;
        }

        uiService.Register(this);

        // UI events may hide or disable the panel during registration.
        if (IsOpen && isActiveAndEnabled)
        {
            revealRoutine = StartCoroutine(Reveal());
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        screenElement = rootElement?.Q<VisualElement>("SceneRevealScreen");

        if (screenElement == null)
        {
            Debug.LogError("SceneRevealScreen requires a SceneRevealScreen element in its UI template.", this);
            Hide();

            return;
        }

        ApplyAppearance();
    }

    private void Update()
    {
        if (IsOpen && (panelRenderer == null || !panelRenderer.enabled || uiService == null))
        {
            Hide();
        }
    }

    private IEnumerator Reveal()
    {
        // Wait for UI creation, then allow an initial frame before timing the reveal.
        while (screenElement == null)
        {
            yield return null;
        }

        yield return null;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, holdDuration));

        float duration = Mathf.Max(0f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            opacity = 1f - Mathf.SmoothStep(0f, 1f, elapsed / duration);
            ApplyAppearance();

            yield return null;

            elapsed += Time.unscaledDeltaTime;
        }

        revealRoutine = null;
        Hide();
    }

    public void ShowImmediate()
    {
        TryShowImmediate();
    }

    public bool TryShowImmediate()
    {
        if (!isActiveAndEnabled || panelRenderer == null || !panelRenderer.enabled
            || screenElement == null || UIService.Instance == null)
        {

            return false;
        }

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        IsOpen = true;
        opacity = 1f;
        ApplyAppearance();
        uiService = UIService.Instance;
        uiService.Register(this);

        return IsOpen && isActiveAndEnabled;
    }

    public void Hide()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        IsOpen = false;
        opacity = 0f;
        ApplyAppearance();

        // Clear ownership before events are raised by Unregister.
        UIService registeredService = uiService;
        uiService = null;

        if (registeredService != null)
        {
            registeredService.Unregister(this);
        }
    }

    private void ApplyAppearance()
    {
        if (screenElement == null)
        {

            return;
        }

        screenElement.style.opacity = opacity;
        screenElement.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnDisable()
    {
        Hide();

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        screenElement = null;
    }
}
