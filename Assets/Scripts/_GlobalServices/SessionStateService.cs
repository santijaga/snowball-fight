using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SessionStateService : MonoBehaviour
{
    public static SessionStateService Instance { get; private set; }

    public string PreviousSceneName { get; private set; }
    public string PreviousSceneId { get; private set; }
    public string SourceTransitionId { get; private set; }
    public string TargetTransitionId { get; private set; }
    public string TargetScene { get; private set; }

    private TransitionPoint transitionOwner;
    private bool transitionInProgress;
    private string savedPreviousSceneName;
    private string savedPreviousSceneId;
    private string savedSourceTransitionId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateSession()
    {
        if (Instance != null)
        {

            return;
        }

        GameObject sessionObject = new(nameof(SessionStateService));
        sessionObject.AddComponent<SessionStateService>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ResetSession()
    {
        PreviousSceneName = null;
        PreviousSceneId = null;
        SourceTransitionId = null;
        ClearPendingTransition();
    }

    public bool TryBeginTransition(TransitionPoint owner)
    {
        if (!isActiveAndEnabled || owner == null || transitionInProgress)
        {

            return false;
        }

        transitionOwner = owner;
        transitionInProgress = true;
        savedPreviousSceneName = PreviousSceneName;
        savedPreviousSceneId = PreviousSceneId;
        savedSourceTransitionId = SourceTransitionId;

        return true;
    }

    public void SetDestination(TransitionPoint owner, string sourceScene, string sourceId,
        string targetScene, string targetId)
    {
        if (!transitionInProgress || transitionOwner != owner)
        {
            throw new InvalidOperationException("Only the active transition point can set the destination.");
        }

        PreviousSceneId = sourceScene;
        PreviousSceneName = owner.gameObject.scene.name;
        SourceTransitionId = sourceId;
        TargetScene = targetScene;
        TargetTransitionId = targetId;
    }

    public void CancelTransition(TransitionPoint owner)
    {
        if (!transitionInProgress || transitionOwner != owner)
        {

            return;
        }

        PreviousSceneName = savedPreviousSceneName;
        PreviousSceneId = savedPreviousSceneId;
        SourceTransitionId = savedSourceTransitionId;
        ClearPendingTransition();
    }

    public void InitializePlayer(GameObject player)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (player == null || !player.TryGetComponent(out PlayerMovementController movement)
            || !movement.isActiveAndEnabled || !player.TryGetComponent(out PlayerController controller)
            || !controller.isActiveAndEnabled)
        {
            Debug.LogError("Session spawn requires an active player with movement and player controllers.", this);
            ClearPendingTransition();

            return;
        }

        Scene scene = player.scene;
        Vector3 position;
        Vector2 facing;

        if (!string.IsNullOrWhiteSpace(TargetTransitionId))
        {
            if (TargetScene != scene.name && TargetScene != scene.path)
            {
                Debug.LogError($"Pending transition targets '{TargetScene}', but the player is in '{scene.path}'.", player);
                ClearPendingTransition();

                return;
            }

            TransitionPoint selected = null;

            foreach (TransitionPoint point in ActiveComponents<TransitionPoint>(scene))
            {
                if (point.Id != TargetTransitionId)
                {
                    continue;
                }

                if (selected == null)
                {
                    selected = point;
                }
                else
                {
                    Debug.LogWarning($"Duplicate transition ID '{TargetTransitionId}' in '{scene.name}'. Using the first active point '{selected.name}' in hierarchy order.", point);
                }
            }

            if (selected == null)
            {
                Debug.LogError($"TransitionPoint '{TargetTransitionId}' was not found in '{scene.name}'. Player was not repositioned.", player);
                ClearPendingTransition();

                return;
            }

            position = selected.SpawnPosition;
            facing = selected.SpawnFacing;
        }
        else
        {
            StartingPoint selected = null;

            foreach (StartingPoint point in ActiveComponents<StartingPoint>(scene))
            {
                if (selected == null)
                {
                    selected = point;
                }
                else
                {
                    Debug.LogWarning($"Multiple StartingPoints in '{scene.name}'. Using '{selected.name}'.", point);
                }
            }

            if (selected == null)
            {
                Debug.LogError($"StartingPoint was not found in '{scene.name}'. Player was not repositioned.", player);
                ClearPendingTransition();

                return;
            }

            position = selected.SpawnPosition;
            facing = selected.SpawnFacing;
        }

        // TeleportTo never raises StepComplete, including when the offset is zero.
        movement.TeleportTo(position);
        controller.SetFacingDirection(facing);
        ClearPendingTransition();
    }

    private static IEnumerable<T> ActiveComponents<T>(Scene scene) where T : MonoBehaviour
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (T component in root.GetComponentsInChildren<T>())
            {
                if (component.isActiveAndEnabled)
                {
                    yield return component;
                }
            }
        }
    }

    private void ClearPendingTransition()
    {
        TargetScene = null;
        TargetTransitionId = null;
        transitionOwner = null;
        transitionInProgress = false;
        savedPreviousSceneName = null;
        savedPreviousSceneId = null;
        savedSourceTransitionId = null;
    }
}
