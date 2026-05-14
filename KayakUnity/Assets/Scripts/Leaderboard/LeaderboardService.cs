using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LeaderboardService
{
    private const string FolderName = "leaderboards";

    private static readonly Dictionary<string, SceneLeaderboard> _cache = new Dictionary<string, SceneLeaderboard>();
    private static bool _pathLogged;

    public static void AddRecord(string sceneName, string playerName, float time)
    {
        if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(playerName)) return;

        SceneLeaderboard board = LoadOrGet(sceneName);
        LeaderboardEntry existing = board.entries.Find(e => e.playerName == playerName);

        if (existing != null)
        {
            if (time >= existing.time) return;
            existing.time = time;
        }
        else
        {
            board.entries.Add(new LeaderboardEntry { playerName = playerName, time = time });
        }

        board.entries = board.entries.OrderBy(e => e.time).ToList();
        Save(sceneName, board);
    }

    public static List<LeaderboardEntry> GetTop(string sceneName, int n)
    {
        if (string.IsNullOrEmpty(sceneName) || n <= 0) return new List<LeaderboardEntry>();
        SceneLeaderboard board = LoadOrGet(sceneName);
        return board.entries.Take(n).ToList();
    }

    public static void Clear()
    {
        _cache.Clear();
        string folder = GetFolderPath();
        try
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Failed to clear folder: {e.Message}");
        }
    }

    private static SceneLeaderboard LoadOrGet(string sceneName)
    {
        if (_cache.TryGetValue(sceneName, out SceneLeaderboard cached)) return cached;

        LogPathOnce();

        string path = GetFilePath(sceneName);
        SceneLeaderboard board;

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                board = JsonUtility.FromJson<SceneLeaderboard>(json) ?? new SceneLeaderboard();
                if (board.entries == null) board.entries = new List<LeaderboardEntry>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] Failed to read {path}, starting fresh: {e.Message}");
                board = new SceneLeaderboard();
            }
        }
        else
        {
            board = new SceneLeaderboard();
        }

        _cache[sceneName] = board;
        return board;
    }

    private static void Save(string sceneName, SceneLeaderboard board)
    {
        string path = GetFilePath(sceneName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        string tmp = path + ".tmp";
        string json = JsonUtility.ToJson(board);

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
            Debug.LogError($"[Leaderboard] Failed to save {path}: {e.Message}");
        }
    }

    private static string GetFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, FolderName);
    }

    private static string GetFilePath(string sceneName)
    {
        return Path.Combine(GetFolderPath(), Sanitize(sceneName) + ".json");
    }

    private static string Sanitize(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0) chars[i] = '_';
        }
        return new string(chars);
    }

    private static void LogPathOnce()
    {
        if (_pathLogged) return;
        _pathLogged = true;
        Debug.Log($"[Leaderboard] Folder: {GetFolderPath()}");
    }
}
