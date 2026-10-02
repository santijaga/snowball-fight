using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class InputContextService : MonoBehaviour
{
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
        RefreshActiveContext();
    }

    private void OnDisable()
    {
        DisableAllContexts();
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
