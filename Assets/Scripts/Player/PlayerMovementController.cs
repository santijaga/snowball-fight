using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    // String Refs
    string impassableLayerString = "Impassable";

    // Components
    Rigidbody2D rigidbody;

    [SerializeField, Min(0.01f)] private float speed = 2f;

    // Positions
    Vector2 targetPosition;
    Vector2 lastPosition;

    // States
    bool isMoving;
    bool isReturning;

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

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        targetPosition = rigidbody.position;
        lastPosition = rigidbody.position;

        isMoving = false;
        isReturning = false;
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

        if (rigidbody.position == targetPosition)
        {
            bool completedStep = !isReturning;
            isMoving = false;
            isReturning = false;

            if (completedStep)
            {
                StepComplete?.Invoke(currentPosition);
            }

            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,speed * Time.fixedDeltaTime
        );

        rigidbody.MovePosition(nextPosition);
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
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.angularVelocity = 0f;

        targetPosition = position;
        lastPosition = position;
        isMoving = false;
        isReturning = false;
    }

    public bool TryMove(Vector2 direction)
    {
        if (!isActiveAndEnabled || direction == Vector2.zero || isMoving || speed <= 0f)
        {

            return false;
        }

        lastPosition = rigidbody.position;
        targetPosition = lastPosition + direction;
        isReturning = false;
        isMoving = true;

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(impassableLayerString) && isMoving)
        {
            targetPosition = lastPosition;
            isReturning = true;
        }
    }
}
