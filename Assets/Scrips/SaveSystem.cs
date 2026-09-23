using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFolderName = "Save";
    private const string SaveFileName = "player_stats.json";

    private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);
    private static string SavePath => Path.Combine(SaveFolderPath, SaveFileName);
    private static string TempSavePath => SavePath + ".tmp";
    private static string BackupSavePath => SavePath + ".bak";

    public static void Save(GameData data)
    {
        if (data == null)
        {
            Debug.LogError("SaveSystem.Save called with null data. Aborting save.");
            return;
        }

        try
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }

            string json = JsonUtility.ToJson(data, true);
            
            File.WriteAllText(TempSavePath, json);

            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupSavePath, true);
            }

            File.Copy(TempSavePath, SavePath, true);
            File.Delete(TempSavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem.Save failed: {e}");
        }
    }

    public static GameData Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                GameData defaultData = new GameData();
                Save(defaultData);
                return defaultData;
            }

            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data == null)
            {
                throw new InvalidDataException("Save file parsed to null.");
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem.Load failed, attempting backup: {e}");
            return LoadBackupOrDefault();
        }
    }

    private static GameData LoadBackupOrDefault()
    {
        try
        {
            if (File.Exists(BackupSavePath))
            {
                string json = File.ReadAllText(BackupSavePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                if (data != null)
                {
                    Debug.LogWarning("SaveSystem loaded from backup file.");
                    Save(data); // Repair the primary save file
                    return data;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem backup load also failed: {e}");
        }

        Debug.LogWarning("SaveSystem falling back to fresh GameData.");
        GameData fallback = new GameData();
        Save(fallback);
        return fallback;
    }
    
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            if (File.Exists(BackupSavePath)) File.Delete(BackupSavePath);
            if (File.Exists(TempSavePath)) File.Delete(TempSavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem.DeleteSave failed: {e}");
        }
    }
}