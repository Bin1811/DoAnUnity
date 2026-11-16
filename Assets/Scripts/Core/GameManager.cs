using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public GameState State = GameState.Playing;
    public int Fish;
    public int Lives;
    public int Wave;
    public int Kills;
    public int FishEarned;
    public float GameSpeed = 1f;
    public bool Paused;

    public const int StartFish = 240;
    public const int MaxLives = 12;

    public WaveManager Waves;
    public Transform TowersRoot;
    public Transform EnemiesRoot;
    public Transform ProjectilesRoot;
    public Transform FxRoot;

    void Awake()
    {
        I = this;
        TowersRoot = MakeRoot("Towers");
        EnemiesRoot = MakeRoot("Enemies");
        ProjectilesRoot = MakeRoot("Projectiles");
        FxRoot = MakeRoot("Fx");
        Waves = gameObject.AddComponent<WaveManager>();
    }

    static Transform MakeRoot(string name)
    {
        var go = new GameObject(name);
        return go.transform;
    }

    public void NewGame()
    {
        State = GameState.Playing;
        Fish = StartFish;
        Lives = MaxLives;
        Wave = 0;
        Kills = 0;
        FishEarned = 0;
        GameSpeed = 1f;
        Paused = false;
        Time.timeScale = 1f;
        Enemy.All.Clear();
        Waves.Begin();
        UIManager.I?.RefreshImmediate();
    }

    public bool TrySpend(int amount)
    {
        if (Fish < amount) return false;
        Fish -= amount;
        return true;
    }

    public void AddFish(int amount)
    {
        if (amount <= 0) return;
        Fish += amount;
        FishEarned += amount;
    }

    public void LoseLife(int damage)
    {
        if (State != GameState.Playing) return;
        Lives -= damage;
        if (CameraShake.I != null) CameraShake.I.Shake(0.3f);
        UIManager.I?.FlashDamage();
        if (Lives <= 0)
        {
            Lives = 0;
            Lose();
        }
    }

    public void Lose()
    {
        if (State != GameState.Playing) return;
        State = GameState.Lost;
        Paused = false;
        Time.timeScale = 1f;
        AudioKit.I.Play("lose");
        UIManager.I?.ShowResult(false);
    }

    public void Win()
    {
        if (State != GameState.Playing) return;
        State = GameState.Won;
        Paused = false;
        Time.timeScale = 1f;
        AudioKit.I.Play("win");
        UIManager.I?.ShowResult(true);
    }

    public void TogglePause()
    {
        if (State != GameState.Playing || DevCapture.InputBlocked) return;
        Paused = !Paused;
        Time.timeScale = Paused ? 0f : GameSpeed;
        AudioKit.I?.Play("click");
    }

    public void ToggleSpeed()
    {
        if (DevCapture.InputBlocked) return;
        GameSpeed = GameSpeed > 1f ? 1f : 2f;
        if (!Paused) Time.timeScale = GameSpeed;
        AudioKit.I?.Play("click");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.volume = AudioKit.I != null && AudioKit.I.Muted ? 0f : 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnWaveCleared(int wave)
    {
        int reward = 25 + wave * 7;
        AddFish(reward);
        UIManager.I?.Toast("Sạch đợt " + wave + "! +" + reward + " cá", Pal.Mint);
    }
}
