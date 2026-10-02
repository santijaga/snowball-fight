using UnityEngine;

// Scene-local player reference; spawn selection belongs to the session.
public sealed class PlayerSceneEntry : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Start()
    {
        SessionStateService activeSession = SessionStateService.Instance;

        if (activeSession == null)
        {
            Debug.LogError("PlayerSceneEntry could not find SessionStateService.Instance. Player initialization was cancelled.", this);

            return;
        }

        activeSession.InitializePlayer(player);
    }
}
