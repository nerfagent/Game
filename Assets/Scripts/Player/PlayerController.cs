// Assets/Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerState playerState;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerState = GetComponent<PlayerState>();

        if (playerMovement == null || playerCombat == null || playerState == null)
        {
            Debug.LogError("PlayerController missing required components.");
            enabled = false;
            return;
        }

        // Subscribe to events for any high-level player coordination
        EventManager.StartListening("OnGameOver", OnGameOver);
        EventManager.StartListening("OnGamePaused", OnGamePaused);
    }

    void OnDestroy()
    {
        EventManager.StopListening("OnGameOver", OnGameOver);
        EventManager.StopListening("OnGamePaused", OnGamePaused);
    }

    private void OnGameOver()
    {
        playerState.SetState(PlayerState.State.Dead);
        enabled = false;
    }

    private void OnGamePaused()
    {
        // Disable all player systems when paused
        playerMovement.enabled = false;
        playerCombat.enabled = false;
    }
}
