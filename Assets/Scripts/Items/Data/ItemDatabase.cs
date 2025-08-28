using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Item Collections")]
    public List<WeaponData> weapons = new List<WeaponData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();
    public List<BombData> bombs = new List<BombData>();

    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDatabase()
    {
        // Загрузка всех предметов из Resources
        LoadAllItems();

        // Заполнение словаря для быстрого поиска
        foreach (var item in GetAllItems())
        {
            if (!itemDictionary.ContainsKey(item.itemName))
            {
                itemDictionary.Add(item.itemName, item);
            }
        }
    }

    void LoadAllItems()
    {
        weapons = Resources.LoadAll<WeaponData>("Items/Weapons").ToList();
        consumables = Resources.LoadAll<ConsumableData>("Items/Consumables").ToList();
        bombs = Resources.LoadAll<BombData>("Items/Bombs").ToList();
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> allItems = new List<ItemData>();
        allItems.AddRange(weapons);
        allItems.AddRange(consumables);
        allItems.AddRange(bombs);
        return allItems;
    }

    public ItemData GetItemByName(string name)
    {
        return itemDictionary.ContainsKey(name) ? itemDictionary[name] : null;
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => new List<ItemData>(weapons),
            ItemType.Consumable => new List<ItemData>(consumables),
            ItemType.Bomb => new List<ItemData>(bombs),
            _ => new List<ItemData>()
        };
    }

    public ItemData GetNextGradeItem(ItemData currentItem)
    {
        if (currentItem.grade == ItemGrade.Grade3) return null; // Максимальный грейд

        ItemGrade nextGrade = currentItem.grade + 1;
        var sameTypeItems = GetItemsByType(currentItem.itemType);

        return sameTypeItems.FirstOrDefault(item =>
            item.itemName.Contains(currentItem.itemName.Split(' ')[0]) &&
            item.grade == nextGrade);
    }
}