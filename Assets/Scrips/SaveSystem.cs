using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath
    {
        get
        {
            string folderPath = Path.Combine(Application.dataPath, "Save");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, "player_stats.json");
        }
    }

    public static void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static GameData Load()
    {
        if (!File.Exists(SavePath))
        {
            GameData defaultData = new GameData();
            Save(defaultData); // Creates the save file immediately on first launch
            return defaultData;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<GameData>(json);
    }
}