using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PartyFormation))]
public sealed class PlayerController : MonoBehaviour
{
    [Min(0f)] public float bufferTime = 0.5f;
    [Min(0f)] public float holdRepeatDelay = 0.2f;

    private PartyFormation formation;
    private InputContextService inputService;
    private InputSystem_Actions.PlayerActions input;
    private Vector2 bufferedInput;
    private Vector2 previousInput;
    private float bufferTimer;
    private float holdRepeatDelayTimer;
    private int controlsLockCount;
    private int pendingSwitchOffset;

    public Character ActiveCharacter => formation != null ? formation.ActiveCharacter : null;
    public event Action<Vector2> StepComplete;

    private bool CanControl => isActiveAndEnabled && formation != null && formation.isActiveAndEnabled
        && inputService != null
        && inputService.isActiveAndEnabled && input.enabled && controlsLockCount == 0
        && (UIService.Instance == null || !UIService.Instance.HasOpenUI)
        && ActiveCharacter != null && ActiveCharacter.isActiveAndEnabled;

    private void Awake()
    {
        formation = GetComponent<PartyFormation>();
    }

    private void OnEnable()
    {
        formation = GetComponent<PartyFormation>();
        formation.StepComplete += OnStepComplete;
        formation.ActiveCharacterChanged += OnActiveCharacterChanged;
        BindInput();
    }

    private void BindInput()
    {
        if (inputService != null)
        {
            inputService.ContextChanged -= OnInputContextChanged;
        }

        inputService = InputContextService.Instance;

        if (inputService != null && inputService.Actions != null)
        {
            input = inputService.Actions.Player;
            inputService.ContextChanged += OnInputContextChanged;
        }

        ClearCommands();
    }

    private void OnInputContextChanged(InputContext context)
    {
        if (context != InputContext.Player)
        {
            ClearCommands();
            ActiveCharacter?.ClearFocus();
        }
    }

    private void OnActiveCharacterChanged()
    {
        ClearCommands();
    }

    private void OnStepComplete(Vector2 position)
    {
        if (isActiveAndEnabled)
        {
            StepComplete?.Invoke(position);
        }
    }

    public IDisposable BlockControls()
    {
        controlsLockCount++;
        ClearCommands();
        ActiveCharacter?.ClearFocus();

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
                owner.controlsLockCount = Mathf.Max(0, owner.controlsLockCount - 1);
                owner.ClearCommands();
            }

            owner = null;
        }
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

        formation.EnsureInitialized();
        ActiveCharacter?.SetFacingDirection(direction);
        ActiveCharacter?.ClearFocus();
        ClearCommands();
    }

    public void TeleportTo(Vector3 position, Vector2 facing)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        formation.TeleportTo(position, facing);
        ClearCommands();
    }

    public bool TrySwitchCharacter(int offset)
    {
        if (!CanControl || formation.IsMoving)
        {

            return false;
        }

        return formation.TrySwitch(offset);
    }

    private void Update()
    {
        if (inputService != InputContextService.Instance)
        {
            BindInput();
        }

        formation.EnsureInitialized();

        if (!CanControl)
        {
            ClearCommands();
            ActiveCharacter?.ClearFocus();

            return;
        }

        int switchOffset = (input.NextCharacter.WasPressedThisFrame() ? 1 : 0)
            - (input.PreviousCharacter.WasPressedThisFrame() ? 1 : 0);
        int memberCount = PartyService.Instance != null ? PartyService.Instance.Members.Count : 1;
        pendingSwitchOffset = (pendingSwitchOffset + switchOffset) % Mathf.Max(1, memberCount);

        if (pendingSwitchOffset != 0 && !formation.IsMoving)
        {
            int requestedOffset = pendingSwitchOffset;
            ClearCommands();
            formation.TrySwitch(requestedOffset);
            ActiveCharacter?.Scan();

            return;
        }

        HandleMovement();
        ActiveCharacter.Scan();

        if (input.Interact.WasPressedThisFrame())
        {
            ActiveCharacter.TryInteract();
        }
    }

    private void HandleMovement()
    {
        float deltaTime = Time.deltaTime;
        holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelayTimer - deltaTime);

        if (bufferedInput != Vector2.zero)
        {
            bufferTimer -= deltaTime;

            if (bufferTimer <= 0f)
            {
                ClearBuffer();
            }
        }

        Vector2 requestedDirection = ReadDirection();
        bool isNewInput = requestedDirection != Vector2.zero && requestedDirection != previousInput;
        previousInput = requestedDirection;

        if (requestedDirection != Vector2.zero && (isNewInput || holdRepeatDelayTimer <= 0f))
        {
            bufferedInput = requestedDirection;
            bufferTimer = Mathf.Max(0f, bufferTime);

            if (isNewInput)
            {
                holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelay);
            }
        }

        if (bufferedInput == Vector2.zero || formation.IsMoving || pendingSwitchOffset != 0
            || !ActiveCharacter.Movement.isActiveAndEnabled)
        {

            return;
        }

        // Blocked attempts still turn the leader and consume the command.
        formation.TryMove(bufferedInput);
        ClearBuffer();
        holdRepeatDelayTimer = Mathf.Max(0f, holdRepeatDelay);
    }

    private Vector2 ReadDirection()
    {
        if (!input.Move.IsPressed())
        {

            return Vector2.zero;
        }

        Vector2 value = input.Move.ReadValue<Vector2>();

        if (value.sqrMagnitude == 0f)
        {

            return Vector2.zero;
        }

        if (Mathf.Abs(value.y) >= Mathf.Abs(value.x))
        {

            return new Vector2(0f, Mathf.Sign(value.y));
        }

        return new Vector2(Mathf.Sign(value.x), 0f);
    }

    private void ClearBuffer()
    {
        bufferedInput = Vector2.zero;
        bufferTimer = 0f;
    }

    private void ClearCommands()
    {
        ClearBuffer();
        previousInput = Vector2.zero;
        holdRepeatDelayTimer = 0f;
        pendingSwitchOffset = 0;
    }

    private void OnDisable()
    {
        if (inputService != null)
        {
            inputService.ContextChanged -= OnInputContextChanged;
        }

        inputService = null;
        formation.StepComplete -= OnStepComplete;
        formation.ActiveCharacterChanged -= OnActiveCharacterChanged;
        ActiveCharacter?.ClearFocus();
        ClearCommands();
    }
}
