using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Run from the menu or Unity -batchmode -executeMethod ComponentArchitectureChecks.Run.
public static class ComponentArchitectureChecks
{
    private static readonly List<GameObject> objects = new();
    private static int checks;

    [MenuItem("Tools/Tests/Check Component Architecture")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Run outside Play Mode.");
        }

        if (InputContextService.Instance != null || DialogService.Instance != null || UIService.Instance != null)
        {
            throw new InvalidOperationException("Runtime services are already initialized.");
        }

        checks = 0;
        try
        {
            var ui = Make("UI").AddComponent<UIService>();
            Invoke(ui, "Awake");
            var input = Make("input").AddComponent<InputContextService>();
            Invoke(input, "Awake");
            Invoke(input, "OnEnable");
            var view = Make("view").AddComponent<DialogView>();
            Set(view, "dialogBox", new VisualElement());
            Set(view, "dialogLabel", new Label());
            var dialog = Make("dialog").AddComponent<DialogService>();
            Set(dialog, "dialogView", view);
            Invoke(dialog, "Awake");
            CheckUI(ui);
            CheckDialog(ui, input, view, dialog);
            CheckPropAndMarker(ui, dialog);
            CheckScanning();
            CheckPrefabs();
            Debug.Log($"Component architecture checks passed: {checks}");
        }
        finally
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(objects[i]);
                }
            }
            objects.Clear();
        }
    }

    private static void CheckUI(UIService ui)
    {
        var first = Make("first window").AddComponent<ArchitectureWindow>();
        var second = Make("second window").AddComponent<ArchitectureWindow>();
        int opened = 0, closed = 0;
        Action<MonoBehaviour> onOpen = _ => opened++;
        Action<MonoBehaviour> onClose = _ => closed++;
        ui.Opened += onOpen;
        ui.Closed += onClose;
        Require(ui.Register(first) && !ui.Register(first), "Duplicate registration is ignored");
        ui.Register(second);
        Require(ui.OpenCount == 2 && ui.IsOpen(first), "Multiple windows are tracked");
        ui.Unregister(first);
        Require(ui.HasOpenUI && ui.OpenCount == 1, "Closing one window preserves the other");
        ui.Unregister(second);
        ui.Unregister(second);
        Require(!ui.HasOpenUI && opened == 2 && closed == 2, "One event per actual transition");
        ui.Opened -= onOpen;
        ui.Closed -= onClose;
    }

    private static void CheckDialog(UIService ui, InputContextService input, DialogView view, DialogService dialog)
    {
        int completed = 0;
        Require(dialog.TryShowDialog("first", () => completed++), "Dialog opens");
        Require(ui.IsOpen(view) && input.CurrentContext == InputContext.Dialog, "Dialog registers UI and owns input");
        Require(!dialog.TryShowDialog("replacement"), "Open transaction cannot be overwritten");
        dialog.HideDialog(() => completed++);
        Require(dialog.IsDialogOpened, "Foreign callback cannot close the dialog");
        dialog.HideDialog();
        dialog.HideDialog();
        Require(completed == 1 && !ui.HasOpenUI && input.CurrentContext == InputContext.Player,
            "Close completes once and releases UI/input");
        dialog.TryShowDialog("covered", () => completed++);
        IDisposable overlay = input.PushContext(InputContext.Dialog);
        dialog.HideDialog();
        Require(input.CurrentContext == InputContext.Dialog, "Covered dialog leaves overlay intact");
        overlay.Dispose();
        Require(input.CurrentContext == InputContext.Player, "Disposed covered context is not revived");
        dialog.TryShowDialog("view hidden", () => completed++);
        view.Hide();
        Require(!dialog.IsDialogOpened && !ui.HasOpenUI && completed == 3, "View hide closes its transaction");
        dialog.TryShowDialog("disabled service", () => completed++);
        Invoke(dialog, "OnDisable");
        Require(!ui.HasOpenUI && completed == 4, "Disabled service releases transaction");
        Action<MonoBehaviour> closeOnOpen = _ => dialog.HideDialog();
        ui.Opened += closeOnOpen;
        Require(!dialog.TryShowDialog("immediately closed") && !ui.HasOpenUI
            && input.CurrentContext == InputContext.Player, "Reentrant close while opening leaves no ownership behind");
        ui.Opened -= closeOnOpen;
    }

    private static void CheckPropAndMarker(UIService ui, DialogService dialog)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Props/Room/Closet.prefab");
        GameObject instance = UnityEngine.Object.Instantiate(prefab);
        objects.Add(instance);
        var prop = instance.GetComponent<DialogProp>();
        Invoke(prop, "Awake");
        var marker = instance.GetComponentInChildren<InteractionMarker>(true);
        marker.Bind(prop);
        var renderer = marker.GetComponent<SpriteRenderer>();
        var propRenderer = prop.GetComponent<SpriteRenderer>();
        Sprite closedSprite = propRenderer.sprite;
        prop.SetPlayerVisible(true);
        Sprite unused = renderer.sprite;
        Require(renderer.enabled, "Focused object shows its marker");
        Require(prop.TryInteract() && prop.HasInteracted && !renderer.enabled, "Accepted interaction hides marker behind dialog");
        Require(propRenderer.sprite != closedSprite, "Closet opens");
        Require(!prop.TryInteract(), "Repeated interaction rejected while UI open");
        var other = Make("other window").AddComponent<ArchitectureWindow>();
        ui.Register(other);
        dialog.HideDialog();
        Require(!renderer.enabled && propRenderer.sprite == closedSprite, "Closet closes but other UI still hides marker");
        ui.Unregister(other);
        Require(renderer.enabled && renderer.sprite != unused, "Last UI close shows interacted sprite");
        prop.SetPlayerVisible(false);
        Require(!renderer.enabled, "Lost focus hides marker");
        prop.SetPlayerVisible(true);
        prop.TryInteract();
        Invoke(prop, "OnDisable");
        Require(!dialog.IsDialogOpened && !ui.HasOpenUI && !renderer.enabled, "Owner disable releases dialog and focus");
        prop.enabled = false;
        Require(!prop.TryInteract(), "Disabled prop cannot interact");
        prop.enabled = true;
        prop.SetPlayerVisible(true);
        Require(renderer.enabled && prop.HasInteracted, "Re-enabled prop retains interacted state");
        prop.SetPlayerVisible(false);
    }

    private static void CheckScanning()
    {
        GameObject target = Make("target");
        target.transform.position = new Vector3(100, 100, 0);
        var prop = target.AddComponent<DialogProp>();
        GameObject child = Make("child collider");
        child.transform.SetParent(target.transform, false);
        child.AddComponent<BoxCollider2D>();
        var scanner = Make("scanner").AddComponent<PlayerInteractionsController>();
        scanner.transform.position = new Vector3(98, 100, 0);
        Set(scanner, "interactiveLayerMask", (LayerMask)(-1));
        Set(scanner, "distance", 3f);
        Physics2D.SyncTransforms();
        scanner.Scan(Vector2.right);
        Require(scanner.CurrentInteractiveObject == (IInteractable)prop && prop.IsPlayerVisible,
            "Child collider resolves parent and focus through interfaces");
        scanner.Scan(Vector2.left);
        Require(!prop.IsPlayerVisible && !scanner.HasInteractiveObject, "Looking away clears focus");
        scanner.Scan(Vector2.right);
        scanner.ClearFocus();
        Require(!prop.IsPlayerVisible, "Player disable can clear focus");
    }

    private static void CheckPrefabs()
    {
        foreach (string name in new[] { "Cabinet", "Closet" })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Props/Room/{name}.prefab");
            Require(prefab.GetComponents<MonoBehaviour>().Length == 1 && prefab.GetComponent<DialogProp>() != null,
                name + " has one behavior");
            Require(prefab.GetComponentsInChildren<InteractionMarker>(true).Length == 1, name + " has one marker");
        }
        var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        Require(player.GetComponent<PlayerMovementController>().Speed == 3f, "Player speed preserved");
    }

    private static GameObject Make(string name)
    {
        var item = new GameObject("Architecture check: " + name);
        item.hideFlags = HideFlags.HideAndDontSave;
        objects.Add(item);

        return item;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }

        checks++;
    }
    private static void Invoke(object target, string name) => target.GetType()
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(target, null);
    private static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
}

public class ArchitectureWindow : MonoBehaviour { }
