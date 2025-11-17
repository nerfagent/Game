// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UnityAction onGameStart = () => { };
    public static UnityAction onGameOver = () => { };
    public static UnityAction onGamePaused = () => { };
    public static UnityAction onGameResumed = () => { };
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
        onGameStart.Invoke();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        PauseManager.Instance.Pause();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        PauseManager.Instance.Resume();
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        onGameOver.Invoke();
    }
}
