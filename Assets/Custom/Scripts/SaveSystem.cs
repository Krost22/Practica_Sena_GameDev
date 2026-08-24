using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Sistema de guardado simple basado en JSON en Application.persistentDataPath.
/// Guarda: nivel actual, vidas, puzzles completados, niveles completados, posición del jugador (checkpoint).
/// </summary>
[Serializable]
public class SaveData
{
    public int currentLevelIndex;
    public int currentLives;
    public int maxLives;
    public string[] completedPuzzles;
    public float playerX, playerY, playerZ;
    public bool hasCheckpoint;
    public int[] completedLevels = new int[0];
}

public static class SaveSystem
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "savegame.json");

    // Cache en memoria de niveles completados
    private static HashSet<int> _completedLevelsCache;
    private static bool _cacheLoaded = false;

    // El Level 1 (índice 1) siempre está desbloqueado
    private const int FirstPlayableLevel = 1;

    /// <summary>
    /// Carga el cache de niveles completados desde disco (una sola vez).
    /// </summary>
    private static void EnsureCacheLoaded()
    {
        if (_cacheLoaded) return;

        _completedLevelsCache = new HashSet<int>();
        SaveData data = Load();
        if (data != null && data.completedLevels != null)
        {
            foreach (int level in data.completedLevels)
            {
                _completedLevelsCache.Add(level);
            }
        }
        _cacheLoaded = true;
    }

    /// <summary>
    /// Marca un nivel como completado y guarda en disco.
    /// </summary>
    public static void MarkLevelCompleted(int levelIndex)
    {
        EnsureCacheLoaded();
        if (_completedLevelsCache.Add(levelIndex))
        {
            SaveCompletedLevels();
        }
    }

    /// <summary>
    /// Verifica si un nivel está completado.
    /// </summary>
    public static bool IsLevelCompleted(int levelIndex)
    {
        EnsureCacheLoaded();
        return _completedLevelsCache.Contains(levelIndex);
    }

    /// <summary>
    /// Verifica si un nivel está desbloqueado.
    /// Un nivel está desbloqueado si:
    /// - Es el primer nivel jugable (Level 1, índice 1), O
    /// - El nivel anterior está completado, O
    /// - Ya está completado (se puede rejugar)
    /// </summary>
    public static bool IsLevelUnlocked(int levelIndex)
    {
        EnsureCacheLoaded();

        // El primer nivel siempre está desbloqueado
        if (levelIndex <= FirstPlayableLevel) return true;

        // Si ya está completado, se puede rejugar
        if (_completedLevelsCache.Contains(levelIndex)) return true;

        // Si el nivel anterior está completado, está desbloqueado
        if (_completedLevelsCache.Contains(levelIndex - 1)) return true;

        return false;
    }

    /// <summary>
    /// Devuelve el índice del nivel más alto desbloqueado.
    /// </summary>
    public static int GetHighestUnlockedLevel()
    {
        EnsureCacheLoaded();
        int highest = FirstPlayableLevel;
        foreach (int level in _completedLevelsCache)
        {
            if (level + 1 > highest) highest = level + 1;
        }
        return highest;
    }

    /// <summary>
    /// Devuelve el array de niveles completados.
    /// </summary>
    public static int[] GetCompletedLevels()
    {
        EnsureCacheLoaded();
        int[] result = new int[_completedLevelsCache.Count];
        _completedLevelsCache.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Guarda solo los niveles completados en el archivo existente (o crea uno nuevo).
    /// </summary>
    private static void SaveCompletedLevels()
    {
        SaveData data = Load() ?? new SaveData();
        int[] levels = new int[_completedLevelsCache.Count];
        _completedLevelsCache.CopyTo(levels);
        data.completedLevels = levels;

        string json = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al guardar niveles completados: {e.Message}");
        }
    }

    // === Métodos existentes (sin cambios) ===

    public static void Save(GameManager gm, Vector3? checkpointPosition = null)
    {
        EnsureCacheLoaded();
        var data = new SaveData
        {
            currentLevelIndex = (int)gm.Level,
            currentLives = gm.CurrentLives,
            maxLives = gm.MaxLives,
            completedPuzzles = new string[0],
            hasCheckpoint = checkpointPosition.HasValue,
            completedLevels = new int[_completedLevelsCache.Count]
        };
        _completedLevelsCache.CopyTo(data.completedLevels, 0);

        if (checkpointPosition.HasValue)
        {
            var pos = checkpointPosition.Value;
            data.playerX = pos.x;
            data.playerY = pos.y;
            data.playerZ = pos.z;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath)) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al cargar: {e.Message}");
            return null;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
        _completedLevelsCache?.Clear();
        _cacheLoaded = false;
    }

    public static Vector3? GetCheckpointPosition(SaveData data)
    {
        if (data == null || !data.hasCheckpoint) return null;
        return new Vector3(data.playerX, data.playerY, data.playerZ);
    }

    /// <summary>
    /// Resetea el cache (útil para testing o "nueva partida").
    /// </summary>
    public static void ResetProgress()
    {
        _completedLevelsCache?.Clear();
        _cacheLoaded = false;
        DeleteSave();
    }
}
