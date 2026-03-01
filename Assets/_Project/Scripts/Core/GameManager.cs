using UnityEngine;

/// <summary>
/// 게임 전체 생명주기를 관리하는 싱글턴.
/// 씬 전환, 일시정지, 게임 상태 등을 담당한다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState State { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        State = newState;
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
    }
}
