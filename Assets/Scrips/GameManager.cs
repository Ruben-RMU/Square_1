using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameData Data { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Data = SaveSystem.Load();
    }

    // Call from your Player movement script when jumping
    public void IncrementJumps() => Data.totalJumps++;

    // Call when player dies
    public void OnPlayerDeath()
    {
        Data.totalDeaths++;
        SaveSystem.Save(Data);
    }

    // Call upon reaching a level goal
    public void CompleteLevel(int levelNumber)
    {
        if (levelNumber >= Data.highestLevelUnlocked)
        {
            Data.highestLevelUnlocked = levelNumber + 1;
        }
        SaveSystem.Save(Data);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save(Data);
    }
}