using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // Points directly to the Assets/Save folder inside your Unity project
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
            // Refresh Unity's AssetDatabase so the file appears in the Project window immediately
            UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    public static GameData Load()
    {
        if (!File.Exists(SavePath))
        {
            return new GameData();
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<GameData>(json);
    }
}