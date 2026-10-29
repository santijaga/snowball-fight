using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Starts an NPC encounter when the leader reaches this area with the required item.
[DisallowMultipleComponent]
// Inspect completed steps before PlayerController can consume held movement again.
[DefaultExecutionOrder(-20)]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class LostShayaTrigger : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private RecruitableNpc npc;
    [SerializeField] private ItemDefinition requiredItem;
    [SerializeField] private DialogSequence introduction;
    [SerializeField, Min(1)] private int searchRadius = 32;
    private BoxCollider2D area;
    private PartyFormation formation;
    private CharacterMovementController movement;
    private CharacterAnimationController animationController;
    private Collider2D npcCollider;
    private Coroutine routine;
    private IDisposable controlsLock;
    private IDisposable inputContext;
    private Vector3 origin;
    private Vector2 facing = Vector2.down;
    private bool originalObstacleCheck;
    private bool attempted;
    private bool ownsApproach;
    private static readonly Vector2Int[] Directions =
        { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };

    private void Awake()
    {
        area = GetComponent<BoxCollider2D>();

        if (player != null)
        {
            formation = player.GetComponent<PartyFormation>();
        }
    }

    private void Update()
    {
        if (player == null || player.ActiveCharacter == null || area == null || !area.enabled)
        {

            return;
        }

        Character leader = player.ActiveCharacter;
        bool inside = area.OverlapPoint(leader.transform.position);

        if (!inside)
        {
            attempted = false;
        }

        // Wait for the completed grid step so the leader stays centred on a cell.
        if (!inside || attempted || ownsApproach || !player.isActiveAndEnabled
            || formation == null || !formation.isActiveAndEnabled || formation.IsMoving
            || npc == null || !npc.isActiveAndEnabled || npc.IsBusy || npc.IsRecruited
            || requiredItem == null || InventoryService.Instance == null
            || !InventoryService.Instance.isActiveAndEnabled
            || InventoryService.Instance.GetQuantity(requiredItem.Id) < 1
            || introduction == null || !introduction.IsValid || !GameLocalization.IsReady
            || DialogService.Instance == null || !DialogService.Instance.isActiveAndEnabled
            || UIService.Instance == null || UIService.Instance.HasOpenUI
            || InputContextService.Instance == null || !InputContextService.Instance.isActiveAndEnabled
            || InputContextService.Instance.CurrentContext != InputContext.Player)
        {

            return;
        }

        attempted = true;
        movement = npc.GetComponent<CharacterMovementController>();
        animationController = npc.GetComponent<CharacterAnimationController>();
        npcCollider = npc.GetComponent<Collider2D>();

        if (movement == null || !movement.isActiveAndEnabled || movement.IsMoving
            || animationController == null || !animationController.isActiveAndEnabled
            || npcCollider == null || !npcCollider.enabled)
        {

            return;
        }

        Physics2D.SyncTransforms();
        List<Vector2> route = FindRoute(leader.Movement.Position);

        if (route == null)
        {
            Debug.LogWarning("NPC encounter could not find a free route to the leader. Leave and re-enter the area to retry.", this);

            return;
        }

        origin = npc.transform.position;
        originalObstacleCheck = movement.CheckObstacles;
        ownsApproach = true;
        controlsLock = player.BlockControls();
        inputContext = InputContextService.Instance.PushContext(InputContext.Dialog);
        movement.CheckObstacles = true;
        routine = StartCoroutine(Approach(route, leader));
    }

    private bool CanContinue(Character leader)
    {

        return isActiveAndEnabled && area != null && area.enabled
            && player != null && player.isActiveAndEnabled && player.ActiveCharacter == leader
            && leader != null && leader.isActiveAndEnabled
            && npc != null && npc.isActiveAndEnabled && !npc.IsBusy && !npc.IsRecruited
            && movement != null && movement.isActiveAndEnabled
            && npcCollider != null && npcCollider.enabled
            && animationController != null && animationController.isActiveAndEnabled
            && InputContextService.Instance != null && InputContextService.Instance.isActiveAndEnabled;
    }

    private IEnumerator Approach(List<Vector2> route, Character leader)
    {
        yield return null;

        foreach (Vector2 destination in route)
        {
            if (!CanContinue(leader))
            {
                CancelApproach();

                yield break;
            }

            facing = destination - movement.Position;
            animationController.SetAnimationParameters(facing, true);

            if (!movement.TryMove(facing))
            {
                CancelApproach();

                yield break;
            }

            while (CanContinue(leader) && movement.IsMoving)
            {
                yield return null;
            }

            if (!CanContinue(leader) || (movement.Position - destination).sqrMagnitude > 0.000001f)
            {
                CancelApproach();

                yield break;
            }
        }

        routine = null;

        if (!CanContinue(leader))
        {
            CancelApproach();

            yield break;
        }

        movement.CheckObstacles = originalObstacleCheck;
        animationController.SetAnimationParameters(facing, false);
        bool started = npc.TryInteract(introduction, DialogSequence.StartMode.NpcApproach);

        if (!started)
        {
            CancelApproach();

            yield break;
        }

        // Recruitment now owns the conversation, final approach and its controls lock.
        ownsApproach = false;
        ReleaseControls();
    }

    private List<Vector2> FindRoute(Vector2 target)
    {
        Vector2 start = movement.Position;
        var pending = new Queue<Vector2Int>();
        var previous = new Dictionary<Vector2Int, Vector2Int>();
        pending.Enqueue(Vector2Int.zero);
        previous.Add(Vector2Int.zero, Vector2Int.zero);

        while (pending.Count > 0)
        {
            Vector2Int cell = pending.Dequeue();
            Vector2 position = start + (Vector2)cell;
            Vector2 distance = position - target;

            if ((Mathf.Abs(distance.x) < 0.001f && Mathf.Abs(Mathf.Abs(distance.y) - 1f) < 0.001f)
                || (Mathf.Abs(distance.y) < 0.001f && Mathf.Abs(Mathf.Abs(distance.x) - 1f) < 0.001f))
            {
                var route = new List<Vector2>();

                while (cell != Vector2Int.zero)
                {
                    route.Add(start + (Vector2)cell);
                    cell = previous[cell];
                }

                route.Reverse();

                return route;
            }

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = cell + direction;

                if (Mathf.Abs(next.x) > searchRadius || Mathf.Abs(next.y) > searchRadius
                    || previous.ContainsKey(next) || !CanTraverse(position, direction))
                {
                    continue;
                }

                previous.Add(next, cell);
                pending.Enqueue(next);
            }
        }

        return null;
    }

    private bool CanTraverse(Vector2 position, Vector2 direction)
    {
        Bounds bounds = npcCollider.bounds;
        Vector2 centre = position + (Vector2)bounds.center - movement.Position;
        int mask = LayerMask.GetMask("Impassable", "NPC", "Player", "PartyFollower");
        RaycastHit2D[] hits = Physics2D.BoxCastAll(centre, bounds.size, 0f, direction, 1f, mask);

        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider.isTrigger && !hit.transform.IsChildOf(npc.transform)
                && Vector2.Dot(hit.normal, direction) < -0.001f)
            {

                return false;
            }
        }

        return true;
    }

    private void ReleaseControls()
    {
        inputContext?.Dispose();
        inputContext = null;
        controlsLock?.Dispose();
        controlsLock = null;
    }

    private void CancelApproach()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (ownsApproach && movement != null)
        {
            movement.CancelMove();
            movement.CheckObstacles = originalObstacleCheck;

            if (npc != null && !npc.IsRecruited)
            {
                Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
                body.position = origin;
                movement.transform.position = origin;
            }

            if (animationController != null)
            {
                animationController.SetAnimationParameters(facing, false);
            }
        }

        ownsApproach = false;
        ReleaseControls();
    }

    private void OnDisable()
    {
        CancelApproach();
        attempted = false;
    }
}
