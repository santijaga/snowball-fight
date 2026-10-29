using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovementController : MonoBehaviour
{
    // Components
    Rigidbody2D rigidbody;
    ContactFilter2D movementFilter;
    readonly List<RaycastHit2D> movementHits = new List<RaycastHit2D>(8);

    [SerializeField, Min(0.01f)] private float speed = 3f;

    // Positions
    Vector2 targetPosition;

    // States
    bool isMoving;

    // Events
    public event Action<Vector2> StepComplete;

    // Setters
    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0.01f, value);
    }

    // Public properties
    public bool IsMoving => isMoving;
    public Vector2 Position => rigidbody.position;
    public bool CheckObstacles { get; set; } = true;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        targetPosition = rigidbody.position;
        movementFilter.SetLayerMask(LayerMask.GetMask("Impassable", "NPC"));
        movementFilter.useTriggers = false;

        isMoving = false;
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            Move();
        }
    }

    void Move()
    {
        Vector2 currentPosition = rigidbody.position;

        if (currentPosition == targetPosition)
        {
            isMoving = false;
            StepComplete?.Invoke(currentPosition);

            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition, speed * Time.fixedDeltaTime
        );

        // Stop without returning if a blocker appears during the step.
        if (CheckObstacles && !CanMove(nextPosition - currentPosition))
        {
            StopMoving();

            return;
        }

        rigidbody.MovePosition(nextPosition);
    }

    private bool CanMove(Vector2 displacement)
    {
        Vector2 direction = displacement.normalized;
        int hitCount = rigidbody.Cast(direction, movementFilter, movementHits, displacement.magnitude);

        for (int i = 0; i < hitCount; i++)
        {
            // Contacts beside or behind the player must not block moving away.
            if (Vector2.Dot(movementHits[i].normal, direction) < -0.001f)
            {

                return false;
            }
        }

        return true;
    }

    public void CancelMove()
    {
        if (rigidbody != null)
        {
            StopMoving();
        }
    }

    // Followers replay the route already travelled by the leader.
    public void FollowTo(Vector2 position)
    {
        if (!isActiveAndEnabled || CheckObstacles || isMoving || position == rigidbody.position)
        {

            return;
        }

        targetPosition = position;
        isMoving = true;
    }

    private void StopMoving()
    {
        targetPosition = rigidbody.position;
        isMoving = false;
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.angularVelocity = 0f;
    }

    private void OnDisable()
    {
        if (rigidbody != null)
        {
            StopMoving();
        }
    }

    // Teleportation cancels a step without reporting it as completed.
    public void TeleportTo(Vector3 position)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        rigidbody.position = position;
        transform.position = position;
        StopMoving();
    }

    public bool TryMove(Vector2 direction)
    {
        if (!isActiveAndEnabled || direction == Vector2.zero || isMoving || speed <= 0f)
        {

            return false;
        }

        // Check the entire step with the body's actual collider shapes.
        if (!CanMove(direction))
        {

            return false;
        }

        targetPosition = rigidbody.position + direction;
        isMoving = true;

        return true;
    }
}
