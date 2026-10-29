using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class InputContextService : MonoBehaviour
{
    public enum DeviceScheme { KeyboardAndMouse, Gamepad }

    private static InputDevice lastUsedDevice;

    public DeviceScheme ActiveDeviceScheme
    {
        get
        {
            if (lastUsedDevice != null && lastUsedDevice.added)
            {

                return lastUsedDevice is Gamepad ? DeviceScheme.Gamepad : DeviceScheme.KeyboardAndMouse;
            }

            // Before the first command, prefer the keyboard when both device types are present.

            return Gamepad.all.Count > 0 && Keyboard.current == null && Mouse.current == null
                ? DeviceScheme.Gamepad
                : DeviceScheme.KeyboardAndMouse;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetDeviceScheme()
    {
        lastUsedDevice = null;
    }

    [SerializeField]
    private InputContext initialContext = InputContext.Player;
    public static InputContextService Instance { get; private set; }
    public InputSystem_Actions Actions { get; private set; }

    public InputContext CurrentContext =>
        contextStack.Count > 0
            ? contextStack.Peek().Context
            : baseContext;

    private InputContext baseContext;

    private readonly Stack<InputContextHandle> contextStack = new();

    public event Action<InputContext> ContextChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        Instance = this;
        Actions = new InputSystem_Actions();
        baseContext = initialContext;
    }

    private void OnEnable()
    {
        if (Actions == null)
        {

            return;
        }

        Actions.Player.Get().actionTriggered += TrackDevice;
        Actions.UI.Get().actionTriggered += TrackDevice;
        Actions.Dialog.Get().actionTriggered += TrackDevice;
        RefreshActiveContext();
    }

    private void OnDisable()
    {
        if (Actions != null)
        {
            Actions.Player.Get().actionTriggered -= TrackDevice;
            Actions.UI.Get().actionTriggered -= TrackDevice;
            Actions.Dialog.Get().actionTriggered -= TrackDevice;
        }

        DisableAllContexts();
    }

    private void TrackDevice(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || !context.performed)
        {

            return;
        }

        InputDevice device = context.control?.device;

        if (!(device is Keyboard) && !(device is Mouse) && !(device is Gamepad))
        {

            return;
        }

        // Track the commands handled by this game; automatic pointer/navigation events do not count.
        if (context.action.actionMap == Actions.UI.Get())
        {
            switch (context.action.name)
            {
                case "Up":
                case "Down":
                case "Left":
                case "Right":
                case "Select":
                case "Back":
                    break;
                default:

                    return;
            }
        }
        else if (context.action.actionMap == Actions.Player.Get())
        {
            switch (context.action.name)
            {
                case "Move":
                    if (context.ReadValue<Vector2>().sqrMagnitude <= 0f)
                    {

                        return;
                    }

                    break;
                case "Interact":
                case "Open Menu":
                case "Next Character":
                case "Previous Character":
                    break;
                default:

                    return;
            }
        }
        else if (context.action.name != "Continue")
        {

            return;
        }

        lastUsedDevice = device;
    }

    private void OnDestroy()
    {
        DisableAllContexts();

        Actions?.Dispose();
        Actions = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetBaseContext(InputContext context)
    {
        baseContext = context;

        if (contextStack.Count == 0)
        {
            RefreshActiveContext();
        }
    }

    public IDisposable PushContext(InputContext context)
    {
        InputContextHandle handle = new(this, context);
        contextStack.Push(handle);
        RefreshActiveContext();

        return handle;
    }

    private bool PopContext(InputContextHandle handle)
    {
        if (contextStack.Count == 0)
        {

            return false;
        }

        if (contextStack.Peek() != handle)
        {
            // A disposed covered context is removed when it reaches the top.

            return contextStack.Contains(handle);
        }

        contextStack.Pop();
        while (contextStack.Count > 0 && contextStack.Peek().IsDisposed)
            contextStack.Pop();

        RefreshActiveContext();

        return true;
    }

    private void RefreshActiveContext()
    {
        if (Actions == null)
        {

            return;
        }

        DisableAllContexts();

        if (isActiveAndEnabled)
        {
            EnableContext(CurrentContext);
        }
    }

    private void EnableContext(InputContext context)
    {
        switch(context)
        {
            case InputContext.Player:
                Actions.Player.Enable();
                break;

            case InputContext.Dialog:
                Actions.Dialog.Enable();
                break;

            case InputContext.UI:
                Actions.UI.Enable();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(context),
                    context,
                    null
                );
        }

        ContextChanged?.Invoke(CurrentContext);
    }

    private void DisableAllContexts()
    {
        if (Actions == null)
        {

            return;
        }

        Actions.Player.Disable();
        Actions.Dialog.Disable();
        Actions.UI.Disable();
    }

    private sealed class InputContextHandle : IDisposable
    {
        private InputContextService router;
        private bool isDisposed;
        public bool IsDisposed => isDisposed;
        public InputContext Context { get; }

        public InputContextHandle(InputContextService router, InputContext context)
        {
            this.router = router;
            Context = context;
        }
        public void Dispose()
        {
            if (isDisposed)
            {

                return;
            }

            if (router != null && !router.PopContext(this))
            {

                return;
            }

            isDisposed = true;
            router = null;
        }
    }
}
