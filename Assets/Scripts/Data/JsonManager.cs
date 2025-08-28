using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    private string savePath;

    public GameData gameData;

    private Dictionary<string, ItemData> itemLookup;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        // Создаём словарь всех предметов из Resources
        itemLookup = new Dictionary<string, ItemData>();
        ItemData[] allItems = Resources.LoadAll<ItemData>(""); // папка Resources
        foreach (var item in allItems)
        {
            if (!itemLookup.ContainsKey(item.guid))
                itemLookup[item.guid] = item;
        }
    }

    public List<ItemData> GetWeaponsFromSave()
    {
        List<ItemData> weapons = new List<ItemData>();
        foreach (string guid in gameData.campItems.weaponGuids)
        {
            if (itemLookup.TryGetValue(guid, out var item))
                weapons.Add(item);
        }
        return weapons;
    }
    /// <summary>
    /// Сохраняем GameData в JSON
    /// </summary>
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"GameData сохранён в {savePath}");
    }

    /// <summary>
    /// Загружаем GameData из JSON
    /// </summary>
    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("GameData загружен!");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден, создаётся новый GameData.");
            gameData = new GameData(); // создаём новый с пустыми списками
        }

        // защищаемся от null
        if (gameData.campItems == null)
            gameData.campItems = new CampItems();
    }
    public bool TryGetItem(string guid, out ItemData item)
    {
        return itemLookup.TryGetValue(guid, out item);
    }
}

[System.Serializable]
public class GameData
{
    public InvenrotyItems[] inventoryItems;
    public CampItems campItems = new CampItems(); // <-- сразу инициализация
    public PlayerData[] playerData;
}

[System.Serializable]
public class InvenrotyItems
{
    public string itemGuid;
    public Vector2Int topLeftCell;
    public bool isRotated;
}

[System.Serializable]
public class CampItems
{
    public List<string> weaponGuids = new List<string>();
    public List<string> bombGuids = new List<string>();
    public List<string> consumableGuids = new List<string>();
}

[System.Serializable]
public class PlayerData
{
    public BackpackData backpackData;
}
