using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterAnimationController), typeof(CharacterMovementController))]
public sealed class RecruitableNpc : MonoBehaviour, IInteractable, IPlayerVisible
{
    [SerializeField] private string npcId;
    [SerializeField] private DialogSequence dialog;
    [SerializeField] private PartyCharacterDefinition recruit;
    [SerializeField] private Vector2 initialFacing = Vector2.down;
    [SerializeField] private Collider2D solidCollider;
    private CharacterAnimationController animationController;
    private CharacterMovementController movement;
    private PlayerController conversationPlayer;
    private IDisposable controlsLock;
    private Coroutine approachRoutine;
    private bool approaching;
    private Vector2 approachOrigin;
    private Vector2 approachFacing;
    private bool colliderWasEnabled;
    private bool checkedObstacles;
    private PartyService party;
    private DialogService dialogService;
    private Action<bool> completion;

    public bool HasInteracted { get; private set; }
    public bool IsPlayerVisible { get; private set; }
    public bool IsBusy => completion != null || approaching;
    public bool IsRecruited => SessionStateService.Instance?.IsNpcRecruited(npcId) == true || IsInParty();
    public event Action Interacted;
    public event Action<bool> VisibilityChanged;

    private void Awake()
    {
        animationController = GetComponent<CharacterAnimationController>();
        movement = GetComponent<CharacterMovementController>();

        if (solidCollider == null)
        {
            solidCollider = GetComponent<Collider2D>();
        }
    }

    private void OnEnable()
    {
        animationController.SetAnimationParameters(initialFacing, false);
        BindParty();
        HideIfRecruited();
    }

    private void Start()
    {
        BindParty();
        HideIfRecruited();
    }

    private void BindParty()
    {
        if (party != null)
        {
            party.Changed -= HideIfRecruited;
        }

        party = PartyService.Instance;

        if (party != null)
        {
            party.Changed += HideIfRecruited;
        }
    }

    private bool IsInParty()
    {
        if (party != null && recruit != null && recruit.CharacterPrefab != null)
        {
            foreach (PartyMember member in party.Members)
            {
                if (member.CharacterPrefab == recruit.CharacterPrefab)
                {

                    return true;
                }
            }
        }

        return false;
    }

    private void HideIfRecruited()
    {
        if (SessionStateService.Instance?.IsNpcRecruited(npcId) == true || IsInParty())
        {
            SessionStateService.Instance?.MarkNpcRecruited(npcId);
            gameObject.SetActive(false);
        }
    }

    public void SetPlayerVisible(bool visible)
    {
        visible &= isActiveAndEnabled;

        if (IsPlayerVisible == visible)
        {

            return;
        }

        IsPlayerVisible = visible;
        VisibilityChanged?.Invoke(visible);
    }

    public bool TryInteract()
    {

        return TryInteract(null);
    }

    public bool TryInteract(DialogSequence introduction,
        DialogSequence.StartMode startMode = DialogSequence.StartMode.PlayerInteraction)
    {
        if (!isActiveAndEnabled || completion != null || approaching || string.IsNullOrWhiteSpace(npcId)
            || recruit == null || SessionStateService.Instance == null
            || (UIService.Instance != null && UIService.Instance.HasOpenUI))
        {

            return false;
        }

        BindParty();
        HideIfRecruited();
        dialogService = DialogService.Instance;

        if (!isActiveAndEnabled || party == null || !party.isActiveAndEnabled || dialogService == null)
        {

            return false;
        }

        conversationPlayer = FindFirstObjectByType<PlayerController>();

        if (conversationPlayer == null || !conversationPlayer.isActiveAndEnabled
            || conversationPlayer.ActiveCharacter == null || !movement.isActiveAndEnabled)
        {

            return false;
        }

        Vector2 offset = conversationPlayer.ActiveCharacter.transform.position - transform.position;
        approachFacing = GetFacing(offset);
        animationController.SetAnimationParameters(approachFacing, false);
        controlsLock = conversationPlayer.BlockControls();
        completion = CompleteDialog;

        if (!dialogService.TryShowSequence(dialog, completion, introduction, startMode))
        {
            CompleteDialog(false);

            return false;
        }

        if (!isActiveAndEnabled || completion == null)
        {

            return false;
        }

        HasInteracted = true;
        Interacted?.Invoke();

        return true;
    }

    private static Vector2 GetFacing(Vector2 offset)
    {

        return Mathf.Abs(offset.y) >= Mathf.Abs(offset.x)
            ? new Vector2(0f, Mathf.Sign(offset.y)) : new Vector2(Mathf.Sign(offset.x), 0f);
    }

    private void CompleteDialog(bool completed)
    {
        completion = null;
        dialogService = null;

        if (!completed || !isActiveAndEnabled || conversationPlayer == null
            || !conversationPlayer.isActiveAndEnabled || conversationPlayer.ActiveCharacter == null
            || !movement.isActiveAndEnabled)
        {
            ReleaseControls();

            return;
        }

        Vector2 destination = conversationPlayer.ActiveCharacter.transform.position;
        approachOrigin = movement.Position;
        checkedObstacles = movement.CheckObstacles;
        colliderWasEnabled = solidCollider != null && solidCollider.enabled;
        approaching = true;
        SetPlayerVisible(false);
        movement.CheckObstacles = false;

        // The adjacent NPC must be able to overlap the leader while joining the chain.
        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }

        Vector2 offset = destination - movement.Position;

        if (offset != Vector2.zero)
        {
            approachFacing = GetFacing(offset);
        }

        animationController.SetAnimationParameters(approachFacing, offset != Vector2.zero);
        movement.FollowTo(destination);
        approachRoutine = StartCoroutine(ApproachPlayer(destination));
    }

    private IEnumerator ApproachPlayer(Vector2 destination)
    {
        // Defer completion so the owner always holds the coroutine handle during cleanup.
        yield return null;

        while (movement.isActiveAndEnabled && movement.IsMoving
            && conversationPlayer != null && conversationPlayer.isActiveAndEnabled
            && conversationPlayer.ActiveCharacter != null)
        {
            yield return null;
        }

        bool arrived = movement.isActiveAndEnabled && conversationPlayer != null
            && conversationPlayer.isActiveAndEnabled && conversationPlayer.ActiveCharacter != null
            && (movement.Position - destination).sqrMagnitude < 0.000001f;
        approachRoutine = null;

        if (!arrived)
        {
            CancelApproach();
            ReleaseControls();

            yield break;
        }

        approaching = false;
        RestoreMovement();

        if (party != null && party.TryAddMember(recruit))
        {
            SessionStateService.Instance?.MarkNpcRecruited(npcId);
            gameObject.SetActive(false);
        }
        else
        {
            movement.TeleportTo(approachOrigin);
            Debug.LogWarning("NPC recruitment failed. Check party capacity and initial equipment; the dialog can be retried.", this);
        }

        ReleaseControls();
    }

    private void RestoreMovement()
    {
        movement.CancelMove();
        movement.CheckObstacles = checkedObstacles;
        animationController.SetAnimationParameters(approachFacing, false);

        if (solidCollider != null)
        {
            solidCollider.enabled = colliderWasEnabled;
        }
    }

    private void CancelApproach()
    {
        if (approachRoutine != null)
        {
            StopCoroutine(approachRoutine);
            approachRoutine = null;
        }

        if (approaching)
        {
            approaching = false;
            RestoreMovement();
            // Restore the NPC even when its movement component has been disabled.
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            body.position = approachOrigin;
            transform.position = new Vector3(approachOrigin.x, approachOrigin.y, transform.position.z);
        }
    }

    private void ReleaseControls()
    {
        controlsLock?.Dispose();
        controlsLock = null;
        conversationPlayer = null;
    }

    private void OnDisable()
    {
        CancelApproach();
        ReleaseControls();
        SetPlayerVisible(false);

        if (party != null)
        {
            party.Changed -= HideIfRecruited;
        }

        if (dialogService != null)
        {
            dialogService.CancelSequence(completion);
        }

        completion = null;
        dialogService = null;
        party = null;
    }
}
