using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class TransitionPoint : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private string targetId;
    [Tooltip("Scene name or full scene path from Build Profiles.")]
    [SerializeField] private string targetScene;
    [SerializeField] private SpawnFacingDirection facingDirection = SpawnFacingDirection.Down;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private PlayerController player;
    [Tooltip("A component implementing ISceneTransitionScreen, such as SceneRevealScreen.")]
    [SerializeField] private MonoBehaviour transitionScreen;

    private PlayerController subscribedPlayer;
    private SessionStateService activeSession;
    private Coroutine transitionRoutine;
    private IDisposable controlsLock;
    private bool transitioning;
    private bool loadRequested;

    public string Id => id;
    public Vector3 SpawnPosition => transform.TransformPoint(spawnOffset);
    public Vector2 SpawnFacing => facingDirection.ToVector();

    private void OnEnable()
    {
        if (player == null)
        {
            Debug.LogError("TransitionPoint requires a PlayerController.", this);

            return;
        }

        subscribedPlayer = player;
        subscribedPlayer.StepComplete += OnStepComplete;
    }

    private void OnStepComplete(Vector2 position)
    {
        if (!isActiveAndEnabled || transitioning || (Vector2)transform.position != position)
        {

            return;
        }

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(targetId)
            || string.IsNullOrWhiteSpace(targetScene) || !Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError($"TransitionPoint '{id}' requires an ID, target ID and an available target scene ('{targetScene}').", this);

            return;
        }

        if (transitionScreen == null || !transitionScreen.isActiveAndEnabled
            || transitionScreen is not ISceneTransitionScreen screen
            || subscribedPlayer == null || !subscribedPlayer.isActiveAndEnabled
            || subscribedPlayer.ActiveCharacter == null)
        {
            Debug.LogError("TransitionPoint requires an active player and an active ISceneTransitionScreen component.", this);

            return;
        }

        activeSession = SessionStateService.Instance;

        if (activeSession == null)
        {
            Debug.LogError("TransitionPoint could not find SessionStateService.Instance. Transition was cancelled.", this);

            return;
        }

        if (!activeSession.TryBeginTransition(this))
        {

            return;
        }

        transitioning = true;

        try
        {
            controlsLock = subscribedPlayer.BlockControls();

            if (!isActiveAndEnabled || !transitioning)
            {
                CancelTransition();

                return;
            }

            if (!screen.TryShowImmediate())
            {
                Debug.LogError("TransitionPoint could not show the transition screen.", this);
                CancelTransition();

                return;
            }

            if (!isActiveAndEnabled || !transitioning)
            {
                CancelTransition();

                return;
            }

            transitionRoutine = StartCoroutine(LoadAfterPresentation());
        }
        catch (Exception exception)
        {
            CancelTransition();
            Debug.LogException(exception, this);
        }
    }

    private IEnumerator LoadAfterPresentation()
    {
        // A completed step can arrive in FixedUpdate. Allow the cover to render.
        yield return null;
        yield return null;

        transitionRoutine = null;

        if (activeSession == null || !activeSession.isActiveAndEnabled)
        {
            Debug.LogError("TransitionPoint lost its active SessionStateService before loading. Transition was cancelled.", this);
            CancelTransition();

            yield break;
        }

        if (transitionScreen == null || !transitionScreen.isActiveAndEnabled
            || subscribedPlayer == null || !subscribedPlayer.isActiveAndEnabled
            || subscribedPlayer.ActiveCharacter == null)
        {
            CancelTransition();

            yield break;
        }

        try
        {
            activeSession.SetDestination(this, gameObject.scene.path, id, targetScene, targetId);
            loadRequested = true;
            SceneManager.LoadScene(targetScene);
        }
        catch (Exception exception)
        {
            loadRequested = false;
            CancelTransition();
            Debug.LogException(exception, this);
        }
    }

    private void CancelTransition()
    {
        transitioning = false;

        if (activeSession != null)
        {
            activeSession.CancelTransition(this);
        }

        controlsLock?.Dispose();
        controlsLock = null;

        if (transitionScreen != null && transitionScreen is ISceneTransitionScreen screen)
        {
            try
            {
                screen.Hide();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void OnDisable()
    {
        if (subscribedPlayer != null)
        {
            subscribedPlayer.StepComplete -= OnStepComplete;
        }

        subscribedPlayer = null;

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (transitioning && !loadRequested)
        {
            CancelTransition();
        }

        controlsLock?.Dispose();
        controlsLock = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.3f, 0.3f, 0f));
        Gizmos.DrawLine(transform.position, SpawnPosition);
        Gizmos.DrawWireSphere(SpawnPosition, 0.12f);
        Gizmos.DrawRay(SpawnPosition, SpawnFacing * 0.5f);
    }
}
