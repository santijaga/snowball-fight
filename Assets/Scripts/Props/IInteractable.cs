using System;

public interface IInteractable
{
    bool HasInteracted { get; }
    event Action Interacted;
    bool TryInteract();
}
