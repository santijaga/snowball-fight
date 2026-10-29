using UnityEngine;

public class CharacterInteractionsController : MonoBehaviour
{
    [SerializeField] private LayerMask interactiveLayerMask;
    [SerializeField, Min(0f)] private float distance = 1f;
    private MonoBehaviour currentTarget;
    private MonoBehaviour currentVisibleTarget;
    public IInteractable CurrentInteractiveObject => currentTarget as IInteractable;
    public bool HasInteractiveObject => currentTarget != null && currentTarget.isActiveAndEnabled;

    public void Scan(Vector2 direction)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, interactiveLayerMask);
        MonoBehaviour detected = null;
        MonoBehaviour detectedVisible = null;

        if (hit.collider != null)
        {
            foreach (MonoBehaviour candidate in hit.collider.GetComponentsInParent<MonoBehaviour>())
            {
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }

                if (detected == null && candidate is IInteractable)
                {
                    detected = candidate;
                }

                if (detectedVisible == null && candidate is IPlayerVisible)
                {
                    detectedVisible = candidate;
                }

                if (detected != null && detectedVisible != null)
                {
                    break;
                }
            }
        }

        if (currentVisibleTarget != detectedVisible)
        {
            ClearFocus();
        }

        currentTarget = detected;
        currentVisibleTarget = detectedVisible;

        if (currentVisibleTarget != null && currentVisibleTarget is IPlayerVisible visible)
        {
            visible.SetPlayerVisible(true);
        }
    }

    public bool TryInteract()
    {
        if (!isActiveAndEnabled || !HasInteractiveObject
            || (UIService.Instance != null && UIService.Instance.HasOpenUI))
        {

            return false;
        }

        return CurrentInteractiveObject.TryInteract();
    }

    public void ClearFocus()
    {
        MonoBehaviour previousVisibleTarget = currentVisibleTarget;
        currentTarget = null;
        currentVisibleTarget = null;

        if (previousVisibleTarget != null && previousVisibleTarget is IPlayerVisible visible)
        {
            visible.SetPlayerVisible(false);
        }
    }

    private void OnDisable() => ClearFocus();
}
