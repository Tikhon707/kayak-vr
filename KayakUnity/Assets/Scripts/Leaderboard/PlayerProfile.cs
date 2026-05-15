using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PlayerProfile
{
    private const string FileName = "profiles.json";

    public const int MaxProfiles = 5;

    private static PlayerProfileData _data;
    private static bool _pathLogged;

    public static string CurrentName => Data.currentName;

    public static bool HasName => !string.IsNullOrEmpty(Data.currentName);

    public static IReadOnlyList<string> KnownNames => Data.knownNames;

    public static bool Exists(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return Data.knownNames.Contains(name);
    }

    public static void Select(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (!Data.knownNames.Contains(name))
            Data.knownNames.Add(name);

        Data.currentName = name;
        Save();
    }

    public static void Forget(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!Data.knownNames.Remove(name)) return;

        if (Data.currentName == name) Data.currentName = "";
        Save();
    }

    public static void ClearCurrent()
    {
        if (string.IsNullOrEmpty(Data.currentName)) return;
        Data.currentName = "";
        Save();
    }

    public static void ResetAll()
    {
        _data = new PlayerProfileData();
        try
        {
            string path = GetFilePath();
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerProfile] Failed to delete file: {e.Message}");
        }
    }

    private static PlayerProfileData Data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    private static void Load()
    {
        LogPathOnce();

        string path = GetFilePath();
        if (!File.Exists(path))
        {
            _data = new PlayerProfileData();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            _data = JsonUtility.FromJson<PlayerProfileData>(json) ?? new PlayerProfileData();
            if (_data.knownNames == null) _data.knownNames = new List<string>();
            if (_data.currentName == null) _data.currentName = "";
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerProfile] Failed to read {path}, starting fresh: {e.Message}");
            _data = new PlayerProfileData();
        }
    }

    private static void Save()
    {
        string path = GetFilePath();
        string tmp = path + ".tmp";
        string json = JsonUtility.ToJson(_data);

        try
        {
            File.WriteAllText(tmp, json);

            try
            {
                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            catch
            {
                File.Copy(tmp, path, overwrite: true);
                if (File.Exists(tmp)) File.Delete(tmp);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerProfile] Failed to save {path}: {e.Message}");
        }
    }

    private static string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    private static void LogPathOnce()
    {
        if (_pathLogged) return;
        _pathLogged = true;
        Debug.Log($"[PlayerProfile] File: {GetFilePath()}");
    }
}