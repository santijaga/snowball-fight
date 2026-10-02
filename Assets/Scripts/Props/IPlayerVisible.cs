using System;

public interface IPlayerVisible
{
    bool IsPlayerVisible { get; }
    event Action<bool> VisibilityChanged;
    void SetPlayerVisible(bool visible);
}
