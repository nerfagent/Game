// Assets/Scripts/Core/PauseManager.cs
using UnityEngine;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    private bool isPaused = false;
    public bool IsPaused => isPaused;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    private void Start()
    {
        // Subscribe to game events
        GameManager.onGamePaused += OnGamePausedEvent;
        GameManager.onGameResumed += OnGameResumedEvent;
    }


    private void OnDestroy()
    {
        GameManager.onGamePaused -= OnGamePausedEvent;
        GameManager.onGameResumed -= OnGameResumedEvent;
    }
    
    private void OnGamePausedEvent()
    {
        // Add any PauseManager-specific logic here
        Debug.Log("PauseManager detected game paused");
    }

    private void OnGameResumedEvent()
    {
        // Add any PauseManager-specific logic here
        Debug.Log("PauseManager detected game resumed");
    }
    
    private void Update()
    {
        if (InputManager.GetPauseInput())
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    /// <summary>
    /// Pause the game.
    /// </summary>
    public void Pause()
    {
        if (isPaused)
            return;
        
        isPaused = true;
        Time.timeScale = 0f;
        GameManager.onGamePaused.Invoke();
        Debug.Log("Game paused");
    }
    
    /// <summary>
    /// Resume the game.
    /// </summary>
    public void Resume()
    {
        if (!isPaused)
            return;
        
        isPaused = false;
        Time.timeScale = 1f;
        GameManager.onGameResumed.Invoke();
        Debug.Log("Game resumed");
    }
}
