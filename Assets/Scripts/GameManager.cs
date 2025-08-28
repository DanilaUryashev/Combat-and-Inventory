using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BackpackGridBuilder BackpackGridBuilder;
    public Button buttonStartRaid;
    public List<(ItemData item, int count)> currentBombs = new List<(ItemData, int)>();
    public List<(ItemData item, int count)> currentPainkillers = new List<(ItemData, int)>();
    public List<(ItemData item, int count)> currentCoffees = new List<(ItemData, int)>();
    public StorageAllObject storageAllObject;
    public WeaponData currentRifle;
    public WeaponData currentPistol;

    [SerializeField] private List<GameObject> camp;
    [SerializeField] private List<GameObject> fight;

    [Header("Точки спавна")]
    public Transform[] spawnPoints;

    [Header("Задержка между спавнами (сек)")]
    public float spawnDelay = 0.5f;

    [Header("Менеджер JSON")]
    public JsonManager jsonManager;

    private void Start()
    {
        storageAllObject = FindFirstObjectByType<StorageAllObject>();
        BackpackGridBuilder = storageAllObject.BackpackPanel.GetComponent<BackpackGridBuilder>();
        if (buttonStartRaid) buttonStartRaid.onClick.AddListener(() => UpdateInventoryInfo());
        if (jsonManager == null)
            jsonManager = FindFirstObjectByType<JsonManager>();
        jsonManager.LoadGame();
        StartCoroutine(SpawnCampItems());
    }

    public void UpdateInventoryInfo()
    {
        if (BackpackGridBuilder == null || BackpackGridBuilder.cells == null)
        {
            Debug.LogWarning("BackpackGridBuilder или его сетка не инициализированы!");
            return;
        }

        Dictionary<ItemData, int> itemCounts = new Dictionary<ItemData, int>();

        int width = BackpackGridBuilder.cells.GetLength(0);
        int height = BackpackGridBuilder.cells.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                InventoryCell cell = BackpackGridBuilder.cells[x, y];
                if (cell != null && cell.currentItem != null && cell.currentItem.itemData != null)
                {
                    ItemData data = cell.currentItem.itemData;

                    if (!itemCounts.ContainsKey(data))
                        itemCounts[data] = 0;

                    itemCounts[data]++;
                }
            }
        }

        // чистим перед обновлением
        currentBombs.Clear();
        currentPainkillers.Clear();
        currentCoffees.Clear();
        currentRifle = null;
        currentPistol = null;

        foreach (var kvp in itemCounts)
        {
            int realCount = kvp.Value / kvp.Key.occupiedCells.Length;
            ItemData data = kvp.Key;

            // тут решаем, куда кидать
            switch (data.itemType)
            {
                case ItemType.Bomb:
                    currentBombs.Add((data as BombData, realCount));
                    break;

                case ItemType.Consumable:
                    if (data.itemName.Contains("Painkiller")) // или другой маркер
                        currentPainkillers.Add((data as ConsumableData, realCount));
                    else if (data.itemName.Contains("Coffee"))
                        currentCoffees.Add((data as ConsumableData, realCount));
                    break;

                case ItemType.Weapon:
                    if (data.itemName.Contains("M4"))
                        currentRifle = data as WeaponData;
                    else if (data.itemName.Contains("G17"))
                        currentPistol = data as WeaponData;
                    break;
            }
        }

        // выводим для проверки
        Debug.Log("=== Инвентарь ===");
        foreach (var b in currentBombs)
            Debug.Log($"Bomb: {b.item.itemName} x{b.count}");

        foreach (var p in currentPainkillers)
            Debug.Log($"Painkiller: {p.item.itemName} x{p.count}");

        foreach (var c in currentCoffees)
            Debug.Log($"Coffee: {c.item.itemName} x{c.count}");

        if (currentRifle != null)
            Debug.Log($"Rifle: {currentRifle.itemName} (Grade {currentRifle.grade})");

        if (currentPistol != null)
            Debug.Log($"Pistol: {currentPistol.itemName} (Grade {currentPistol.grade})");
        OpenScene();
    }
    private void OpenScene()
    {
        foreach (GameObject b in camp)
        {
            if (b.activeSelf)
            {
                storageAllObject.UpdateCampItemData();
                b.SetActive(false);
            }
            else
            {
                b.SetActive(true);
            }
        }
        foreach (GameObject b in fight)
        {
            if (b.activeSelf)
            {
                b.SetActive(false);
            }
            else
            {
                b.SetActive(true);
            }
        }
    }
    private IEnumerator SpawnCampItems()
    {
        if (jsonManager == null || jsonManager.gameData == null)
        {
            Debug.LogWarning("JsonManager или GameData пуст!");
            yield break;
        }
        Debug.Log("<color=red>dsfjbdsfhgkbdsjkfklnadsknfkmnnks;dfnmk;");
        // Собираем все guids предметов
        List<string> allGuids = new List<string>();
        allGuids.AddRange(jsonManager.gameData.campItems.weaponGuids);
        allGuids.AddRange(jsonManager.gameData.campItems.bombGuids);
        allGuids.AddRange(jsonManager.gameData.campItems.consumableGuids);

        int spawnIndex = 0;

        foreach (string guid in allGuids)
        {
            if (jsonManager.TryGetItem(guid, out ItemData item))
            {
                if (item.worldPrefab != null)
                {
                    // выбираем точку по очереди
                    Transform point = spawnPoints[spawnIndex % spawnPoints.Length];

                    Instantiate(item.worldPrefab, point.position, point.rotation);
                    Debug.Log("Spawn " + item.worldPrefab.name);
                    spawnIndex++;
                    yield return new WaitForSeconds(spawnDelay);
                }
                else
                {
                    Debug.LogWarning($"У предмета {item.itemName} нет worldPrefab!");
                }
            }
            else
            {
                Debug.LogWarning($"GUID {guid} не найден в словаре JsonManager!");
            }
        }
    }
}
