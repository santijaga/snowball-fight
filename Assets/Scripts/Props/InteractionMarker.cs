using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractionMarker : MonoBehaviour
{
    [SerializeField] private Sprite notInteractedSprite;
    [SerializeField] private Sprite interactedSprite;
    [SerializeField] private bool shouldToggle;

    private MonoBehaviour target;
    private IPlayerVisible visibility;
    private IInteractable interaction;
    private SpriteRenderer spriteRenderer;
    private UIService uiService;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        foreach (MonoBehaviour candidate in GetComponentsInParent<MonoBehaviour>())
        {
            if (candidate is IPlayerVisible && candidate is IInteractable)
            {
                target = candidate;
                break;
            }
        }

        visibility = target as IPlayerVisible;
        interaction = target as IInteractable;

        if (visibility != null)
        {
            visibility.VisibilityChanged += OnVisibilityChanged;
        }

        if (interaction != null)
        {
            interaction.Interacted += Refresh;
        }

        uiService = UIService.Instance;

        if (uiService != null)
        {
            uiService.OpenCountChanged += OnUIChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (visibility != null)
        {
            visibility.VisibilityChanged -= OnVisibilityChanged;
        }

        if (interaction != null)
        {
            interaction.Interacted -= Refresh;
        }

        if (uiService != null)
        {
            uiService.OpenCountChanged -= OnUIChanged;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void Bind(MonoBehaviour owner)
    {
        if (isActiveAndEnabled)
        {
            OnDisable();
        }

        target = owner;

        if (isActiveAndEnabled)
        {
            OnEnable();
        }
    }

    private void OnVisibilityChanged(bool visible) => Refresh();
    private void OnUIChanged(int count) => Refresh();

    private void Refresh()
    {
        spriteRenderer.sprite = interaction != null && interaction.HasInteracted && shouldToggle
            ? interactedSprite : notInteractedSprite;

        spriteRenderer.enabled = isActiveAndEnabled && target != null && target.isActiveAndEnabled
            && visibility != null && visibility.IsPlayerVisible
            && (uiService == null || !uiService.HasOpenUI);
    }
}
