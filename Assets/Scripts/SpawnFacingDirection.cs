using UnityEngine;

public enum SpawnFacingDirection
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public static class SpawnFacingDirectionExtensions
{
    public static Vector2 ToVector(this SpawnFacingDirection direction)
    {

        return direction switch
        {
            SpawnFacingDirection.Up => Vector2.up,
            SpawnFacingDirection.Down => Vector2.down,
            SpawnFacingDirection.Left => Vector2.left,
            SpawnFacingDirection.Right => Vector2.right,
            _ => Vector2.down
        };
    }
}
