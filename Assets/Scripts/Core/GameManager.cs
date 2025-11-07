//GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        CurrentState = GameState.Menu;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        EventManager.TriggerEvent("OnGameStarted");
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        EventManager.TriggerEvent("OnGamePaused");
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        EventManager.TriggerEvent("OnGameResumed");
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        EventManager.TriggerEvent("OnGameOver");
    }
}
