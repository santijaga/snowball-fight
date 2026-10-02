using System;
using UnityEngine;

[
    RequireComponent(
        typeof(PlayerMovementController),
        typeof(PlayerAnimationController),
        typeof(PlayerInteractionsController)
    )
]
public class PlayerController : MonoBehaviour
{
    [Min(0f)] public float bufferTime = 0.5f;
    [Min(0f)] public float holdRepeatDelay = 0.2f;

    private PlayerMovementController movementController;
    private PlayerAnimationController animationController;
    private PlayerInteractionsController interactionController;

    // Direction of the last accepted step; preserved while idle.
    private Vector2 targetDirection = Vector2.zero;

    // One pending command and the input observed in the previous frame.
    private Vector2 bufferedInput = Vector2.zero;
    private Vector2 previousInput = Vector2.zero;

    private float bufferTimer;
    private float holdRepeatDelayTimer;

    private InputSystem_Actions.PlayerActions input;
    private int controlsLockCount;

    public IDisposable BlockControls()
    {
        controlsLockCount++;
        ClearMovementInput();
        interactionController.ClearFocus();

        return new ControlsLock(this);
    }

    private sealed class ControlsLock : IDisposable
    {
        private PlayerController owner;

        public ControlsLock(PlayerController owner)
        {
            this.owner = owner;
        }

        public void Dispose()
        {
            if (owner != null)
            {
                owner.controlsLockCount--;
                owner.ClearMovementInput();
            }

            owner = null;
        }
    }

    private void Awake()
    {
        movementController = GetComponent<PlayerMovementController>();
        animationController = GetComponent<PlayerAnimationController>();
        interactionController = GetComponent<PlayerInteractionsController>();
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (direction != Vector2.up && direction != Vector2.down
            && direction != Vector2.left && direction != Vector2.right)
        {
            Debug.LogError("PlayerController facing direction must be up, down, left or right.", this);

            return;
        }

        targetDirection = direction;
        ClearMovementInput();
        interactionController.ClearFocus();
        animationController.SetAnimationParameters(targetDirection, movementController.IsMoving);
    }

    private void Start()
    {
        input = InputContextService.Instance.Actions.Player;
        InputContextService.Instance.ContextChanged += OnInputContextChanged;
    }

    private void OnDestroy()
    {
        if (InputContextService.Instance != null)
        {
            InputContextService.Instance.ContextChanged -= OnInputContextChanged;
        }
    }

    private void OnInputContextChanged(InputContext context)
    {
        if (context != InputContext.Player)
        {
            ClearMovementInput();
        }
    }

    private void OnDisable()
    {
        if (interactionController != null)
        {
            interactionController.ClearFocus();
        }

        ClearMovementInput();
    }

    private void ClearMovementInput()
    {
        ClearBuffer();
        previousInput = Vector2.zero;
        holdRepeatDelayTimer = 0f;
    }

    private void Update()
    {
        HandleMovement();

        interactionController.Scan(targetDirection);

        HandleInteractions();

        animationController.SetAnimationParameters(
            targetDirection,
            movementController.IsMoving
        );
    }

    private void HandleMovement()
    {
        if (controlsLockCount > 0 || !input.enabled || (UIService.Instance != null && UIService.Instance.HasOpenUI))
        {
            ClearMovementInput();

            return;
        }

        float deltaTime = Time.deltaTime;

        holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelayTimer - deltaTime);

        // Expire the previous command before accepting or executing input.
        if (bufferedInput != Vector2.zero)
        {
            bufferTimer -= deltaTime;

            if (bufferTimer <= 0f)
            {
                ClearBuffer();
            }
        }

        Vector2 requestedDirection = ReadDirection();
        bool isNewInput = requestedDirection != Vector2.zero
            && requestedDirection != previousInput;

        // Remember releases too, so pressing the same direction again is new.
        previousInput = requestedDirection;

        if (requestedDirection != Vector2.zero
            && (isNewInput || holdRepeatDelayTimer <= 0f))
        {
            // The latest eligible request replaces the pending command.
            bufferedInput = requestedDirection;
            bufferTimer = Mathf.Max(0f, bufferTime);

            if (isNewInput)
            {
                holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelay);
            }
        }

        // A pending command can execute even after the button is released.
        MoveCharacter();
    }

    private void HandleInteractions()
    {
        if (controlsLockCount == 0 && input.enabled
            && (UIService.Instance == null || !UIService.Instance.HasOpenUI)
            && input.Interact.WasPressedThisFrame())
        {
            interactionController.TryInteract();
        }
    }

    private Vector2 ReadDirection()
    {
        if (!input.Move.IsPressed())
        {

            return Vector2.zero;
        }

        Vector2 moveValue = input.Move.ReadValue<Vector2>();

        if (moveValue.sqrMagnitude == 0f)
        {

            return Vector2.zero;
        }

        // Compare cardinal directions, not small changes in stick magnitude.
        if (Mathf.Abs(moveValue.y) >= Mathf.Abs(moveValue.x))
        {

            return new Vector2(0f, Mathf.Sign(moveValue.y));
        }

        return new Vector2(Mathf.Sign(moveValue.x), 0f);
    }

    private void MoveCharacter()
    {
        if (bufferedInput == Vector2.zero)
        {

            return;
        }

        if (!movementController.TryMove(bufferedInput))
        {

            return;
        }

        targetDirection = bufferedInput;

        ClearBuffer();

        // Each accepted step starts a new delay for held-input repeats.
        holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelay);
    }

    private void ClearBuffer()
    {
        bufferedInput = Vector2.zero;
        bufferTimer = 0f;
    }
}
