using System;
using System.Collections.Generic;
using UnityEngine;

// Register windows/panels, not their labels or world-space markers.
[DefaultExecutionOrder(-90)]
public class UIService : MonoBehaviour
{
    public static UIService Instance { get; private set; }
    private readonly HashSet<MonoBehaviour> openElements = new();
    public int OpenCount => openElements.Count;
    public bool HasOpenUI => OpenCount > 0;
    public event Action<MonoBehaviour> Opened;
    public event Action<MonoBehaviour> Closed;
    public event Action<int> OpenCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);

            return;
        }

        Instance = this;
    }

    public bool IsOpen(MonoBehaviour element) => element != null && openElements.Contains(element);

    // The owner unregisters on hide/disable. Repeated calls are harmless.
    // Disabling this service does not hide actual windows or erase their state.
    public bool Register(MonoBehaviour element)
    {
        if (Instance != this || element == null || !element.isActiveAndEnabled
            || !openElements.Add(element))
        {

            return false;
        }

        Opened?.Invoke(element);
        OpenCountChanged?.Invoke(OpenCount);

        return true;
    }

    public bool Unregister(MonoBehaviour element)
    {
        if (!openElements.Remove(element))
        {

            return false;
        }

        Closed?.Invoke(element);
        OpenCountChanged?.Invoke(OpenCount);

        return true;
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {

            return;
        }

        foreach (MonoBehaviour element in new List<MonoBehaviour>(openElements))
            Unregister(element);
        Instance = null;
    }
}
