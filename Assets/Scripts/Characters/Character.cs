using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterMovementController), typeof(CharacterAnimationController), typeof(CharacterInteractionsController))]
public sealed class Character : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    private CharacterAnimationController animationController;
    private CharacterInteractionsController interactions;
    private bool controlled;

    public PartyMember Member { get; private set; }
    public CharacterMovementController Movement { get; private set; }
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        Movement = GetComponent<CharacterMovementController>();
        animationController = GetComponent<CharacterAnimationController>();
        interactions = GetComponent<CharacterInteractionsController>();
    }

    public void Bind(PartyMember member)
    {
        Member = member;
    }

    public void SetControlled(bool value)
    {
        controlled = value;
        gameObject.layer = LayerMask.NameToLayer(controlled ? "Player" : "PartyFollower");
        Movement.CheckObstacles = controlled;

        if (visual != null)
        {
            visual.sortingOrder = controlled ? 1 : 0;
        }

        if (!controlled)
        {
            ClearFocus();
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {

            return;
        }

        FacingDirection = Mathf.Abs(direction.y) >= Mathf.Abs(direction.x)
            ? new Vector2(0f, Mathf.Sign(direction.y))
            : new Vector2(Mathf.Sign(direction.x), 0f);
        animationController.SetAnimationParameters(FacingDirection, Movement.IsMoving);
    }

    public void TeleportTo(Vector3 position, Vector2 facing)
    {
        Movement.TeleportTo(position);
        SetFacingDirection(facing);
        ClearFocus();
    }

    public void Scan()
    {
        if (isActiveAndEnabled && controlled)
        {
            interactions.Scan(FacingDirection);
        }
    }

    public void ClearFocus()
    {
        if (interactions != null)
        {
            interactions.ClearFocus();
        }
    }

    public bool TryInteract()
    {

        return isActiveAndEnabled && controlled && interactions.TryInteract();
    }

    private void Update()
    {
        animationController.SetAnimationParameters(FacingDirection, Movement.IsMoving);
    }

    private void OnDisable()
    {
        ClearFocus();

        if (Movement != null)
        {
            Movement.CancelMove();
        }

        if (animationController != null)
        {
            animationController.SetAnimationParameters(FacingDirection, false);
        }
    }
}
