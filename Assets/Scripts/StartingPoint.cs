using UnityEngine;

[DisallowMultipleComponent]
public sealed class StartingPoint : MonoBehaviour
{
    [SerializeField] private SpawnFacingDirection facingDirection = SpawnFacingDirection.Down;

    public Vector3 SpawnPosition => transform.position;
    public Vector2 SpawnFacing => facingDirection.ToVector();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(SpawnPosition, 0.15f);
        Gizmos.DrawRay(SpawnPosition, SpawnFacing * 0.5f);
    }
}
