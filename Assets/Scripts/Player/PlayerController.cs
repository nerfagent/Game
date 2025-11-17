// Assets/Scripts/Level/Player/PlayerController.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private SkillSystem skillSystem;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerState = GetComponent<PlayerState>();
        skillSystem = GetComponent<SkillSystem>();

        if (playerMovement == null || playerCombat == null || playerState == null || skillSystem == null)
        {
            Debug.LogError("PlayerController missing required components.");
            enabled = false;
            return;
        }

        // Subscribe to game events
        GameManager.onGameOver += OnGameOver;
        GameManager.onGamePaused += OnGamePaused;
        GameManager.onGameResumed += OnGameResumed;
    }

    void OnDestroy()
    {
        GameManager.onGameOver -= OnGameOver;
        GameManager.onGamePaused -= OnGamePaused;
        GameManager.onGameResumed -= OnGameResumed;
    }

    private void OnGameOver()
    {
        playerState.SetState(PlayerState.State.Dead);
        enabled = false;
    }

    private void OnGamePaused()
    {
        playerMovement.enabled = false;
        playerCombat.enabled = false;
        skillSystem.enabled = false;
    }

    private void OnGameResumed()
    {
        playerMovement.enabled = true;
        playerCombat.enabled = true;
        skillSystem.enabled = true;
    }
}
