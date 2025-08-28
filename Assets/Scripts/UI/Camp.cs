using System;
using UnityEngine;


public class Camp : MonoBehaviour
{
    [Header("Available Backpacks")]
    public BackpackData[] backpackOptions; // Все доступные рюкзаки
    public int selectedBackpackIndex = 0;  // Индекс выбранного рюкзака

    [Header("UI References")]
    public Transform backpackPanel;      // Родитель для рюкзака в канвасе

    private GameObject currentBackpackInstance;
    [Header("Grid Builder")]
    [SerializeField] private BackpackGridBuilder gridBuilder;
    private void Start()
    {
        // Автоматически загружаем все BackpackData из папки Resources
        backpackOptions = Resources.LoadAll<BackpackData>("Items/Backpack");
        gridBuilder = GetComponent<BackpackGridBuilder>();
        // Показываем выбранный рюкзак при старте
        ShowSelectedBackpack();
    }
    private void OnValidate()
    {
        ShowSelectedBackpack();
    }

    #region ShowSelectedBackpack
    // Метод для отображения выбранного рюкзака
    public void ShowSelectedBackpack()
    {
        // Проверяем есть ли рюкзаки для выбора
        if (backpackOptions == null || backpackOptions.Length == 0)
        {
            Debug.LogWarning("Нет доступных рюкзаков!");
            return;
        }

        // Ограничиваем индекс в пределах массива
        selectedBackpackIndex = Mathf.Clamp(selectedBackpackIndex, 0, backpackOptions.Length - 1);

        // Получаем выбранный рюкзак
        BackpackData selectedBackpack = backpackOptions[selectedBackpackIndex];

        // Создаем и отображаем рюкзак
        CreateBackpackUI(selectedBackpack);
    }

    // Создание UI рюкзака
    private void CreateBackpackUI(BackpackData backpackData)
    {
        // Удаляем предыдущий рюкзак если есть
        if (currentBackpackInstance != null)
        {
            Destroy(currentBackpackInstance);
        }

        // Проверяем есть ли префаб
        if (backpackData.backpackPrefab == null)
        {
            Debug.LogError($"У рюкзака {backpackData.backpackName} нет UI префаба!");
            return;
        }

        // Создаем экземпляр рюкзака в канвасе
        currentBackpackInstance = Instantiate(backpackData.backpackPrefab, backpackPanel);
        currentBackpackInstance.name = backpackData.backpackName;

        Debug.Log($"Создан рюкзак: {backpackData.backpackName}");
        FindAndBuildInventoryZones(currentBackpackInstance, backpackData);
    }

    // Метод для переключения рюкзаков (можно вызывать из инспектора)
    public void SelectBackpack(int index)
    {
        selectedBackpackIndex = index;
        ShowSelectedBackpack();
    }

    // Метод для следующего рюкзака
    public void SelectNextBackpack()
    {
        selectedBackpackIndex = (selectedBackpackIndex + 1) % backpackOptions.Length;
        ShowSelectedBackpack();
    }

    // Метод для предыдущего рюкзака
    public void SelectPreviousBackpack()
    {
        selectedBackpackIndex = (selectedBackpackIndex - 1 + backpackOptions.Length) % backpackOptions.Length;
        ShowSelectedBackpack();
    }
    #endregion
    #region
    private void FindAndBuildInventoryZones(GameObject backpackInstance, BackpackData backpackData)
    {
        // Ищем основную зону инвентаря
        Transform mainZone = FindChildRecursive(backpackInstance.transform, backpackData.mainInventoryZoneName);
        if (mainZone != null && gridBuilder != null)
        {
            gridBuilder.BuildInventoryGrid(mainZone, backpackData.size, "Основной инвентарь", backpackData.cellPrefab);
        }

        // Ищем дополнительные кармашки
        if (backpackData.additionalPockets != null)
        {
            foreach (var pocket in backpackData.additionalPockets)
            {
                Transform pocketZone = FindChildRecursive(backpackInstance.transform, pocket.pocketObjectName);
                if (pocketZone != null && gridBuilder != null)
                {
                    gridBuilder.BuildInventoryGrid(pocketZone, pocket.size, pocket.pocketObjectName, backpackData.cellPrefab);
                }
            }
        }
    }

    // Вспомогательная функция для поиска дочерних объектов
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
    #endregion
}