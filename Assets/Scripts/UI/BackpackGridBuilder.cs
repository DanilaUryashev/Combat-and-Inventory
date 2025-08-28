using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BackpackGridBuilder : MonoBehaviour
{
    [Header("Settings")]
    public GameObject cellPrefab;          // Префаб ячейки инвентаря
    public Vector2 cellSize = new Vector2(50, 50); // Размер ячейки
    public Vector2 cellSpacing = new Vector2(5, 5); // Расстояние между ячейками
    [Tooltip("Отступы от краев контейнера")]
    public Vector4 containerPadding = new Vector4(10, 10, 10, 10); // left, top, right, bottom

    public InventoryCell[,] cells;
    private JsonManager jsonManager;
    private GameObject upgradePanel;
    private GameObject viewInfoItem;

    private void Start()
    {
        StorageAllObject storage = FindFirstObjectByType<StorageAllObject>();
        upgradePanel = storage.CreatePanel;
        viewInfoItem = storage.infoItemPanel;
        jsonManager = storage.jsonManager;

        StartCoroutine(SpawnItemscFromSave());
    }

    public void OpenCloseButton()
    {
        if (gameObject.activeSelf)
        {
            foreach (InventoryCell cell in cells)
            {
                if (cell.currentItem != null)
                {
                    if (cell.currentItem.gameObject.activeSelf)
                        cell.currentItem.gameObject.SetActive(false);
                }
               
            }
            gameObject.SetActive(false);
        }
        else
        {
            upgradePanel.SetActive(false);
            viewInfoItem.SetActive(false);
            gameObject.SetActive(true);
        }
       
    }

    // Создает сетку ячеек в указанном контейнере
    public void BuildInventoryGrid(Transform container, Vector2Int gridSize, string zoneName, GameObject cellPrefab)
    {
        if (container == null)
        {
            Debug.LogError("Контейнер не указан!");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("Префаб ячейки не назначен!");
            return;
        }

        // Очищаем старые ячейки
        ClearExistingCells(container);

        // Добавляем GridLayoutGroup для автоматического расположения
        GridLayoutGroup gridLayout = container.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();

        // Рассчитываем размеры ячеек и spacing
        CalculateGridLayout(gridLayout, container, gridSize);

        // Настраиваем сетку
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize.x;

        // Сохраняем размер ячеек
        cellSize.x = gridLayout.cellSize.x;
        cellSize.y = gridLayout.cellSize.y;

        // Создаём массив для координатной сетки
        cells = new InventoryCell[gridSize.x, gridSize.y];

        // Создаем ячейки
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, container);
                cellObj.name = $"Cell_{x}_{y}";

                InventoryCell invCell = cellObj.AddComponent<InventoryCell>();
                invCell.gridPosition = new Vector2Int(x, y);

                // Добавляем Image если нет
                if (cellObj.GetComponent<Image>() == null)
                {
                    Image img = cellObj.AddComponent<Image>();
                    img.color = new Color(1, 1, 1, 0.3f);
                }

                cells[x, y] = invCell;
            }
        }

        Debug.Log($"Создана сетка {zoneName}: {gridSize.x}x{gridSize.y} ячеек");
    }

    // Создает одну ячейку
    private void CreateCell(Transform parent, Vector2Int position)
    {
        GameObject cell = Instantiate(cellPrefab, parent);
        cell.name = $"Cell_{position.x}_{position.y}";
        // Настраиваем компонент ячейки
        //InventoryCell inventoryCell = cell.GetComponent<InventoryCell>();
        //if (inventoryCell != null)
        //{
        //    inventoryCell.Initialize(position);
        //}

        // Добавляем компоненты для UI
        if (cell.GetComponent<Image>() == null)
        {
            Image image = cell.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0.3f); // Полупрозрачный фон
        }

    }

    // Очищает все ячейки в контейнере
    private void ClearExistingCells(Transform container)
    {
        foreach (Transform child in container)
        {
            // Удаляем только ячейки, а не другие объекты
            if (child.name.StartsWith("Cell_"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Обновляет размер контейнера based on размера сетки
    private void CalculateGridLayout(GridLayoutGroup gridLayout, Transform container, Vector2Int gridSize)
    {
        // Получаем RectTransform контейнера
        RectTransform containerRect = container.GetComponent<RectTransform>();

        // Рассчитываем доступное пространство с учетом отступов
        float availableWidth = containerRect.rect.width - containerPadding.x - containerPadding.z;
        float availableHeight = containerRect.rect.height - containerPadding.y - containerPadding.w;

        // Рассчитываем оптимальный размер ячейки с учетом пропорций
        float cellWidth = (availableWidth - (gridSize.x - 1) * gridLayout.spacing.x) / gridSize.x;
        float cellHeight = (availableHeight - (gridSize.y - 1) * gridLayout.spacing.y) / gridSize.y;

        // Берем минимальный размер, чтобы все ячейки поместились
        float maxCellSize = Mathf.Min(cellWidth, cellHeight);

        // Устанавливаем квадратные ячейки
        gridLayout.cellSize = new Vector2(
            Mathf.Max(30f, maxCellSize),
            Mathf.Max(30f, maxCellSize)
        );

        // Пересчитываем spacing чтобы заполнить всю зону
        float totalCellsWidth = gridSize.x * gridLayout.cellSize.x;
        float totalCellsHeight = gridSize.y * gridLayout.cellSize.y;

        float remainingWidth = availableWidth - totalCellsWidth;
        float remainingHeight = availableHeight - totalCellsHeight;

        // Равномерно распределяем оставшееся пространство между ячейками
        if (gridSize.x > 1)
        {
            gridLayout.spacing = new Vector2(Mathf.Max(10f, remainingWidth / (gridSize.x - 1))
                ,
                Mathf.Max(10f, gridLayout.spacing.y)
            );
        }

        if (gridSize.y > 1)
        {
            gridLayout.spacing = new Vector2(
                Mathf.Max(10f, gridLayout.spacing.x),
                Mathf.Max(10f, remainingHeight / (gridSize.y - 1))
            );
        }

        // Настраиваем отступы контейнера
        gridLayout.padding = new RectOffset(
            (int)containerPadding.x,
            (int)containerPadding.z,
            (int)containerPadding.y,
            (int)containerPadding.w
        );
    }
    public InventoryItemUI[] GetAllItems()
    {
        HashSet<InventoryItemUI> items = new HashSet<InventoryItemUI>();

        foreach (var cell in cells)
        {
            if (cell.currentItem != null)
                items.Add(cell.currentItem);
        }

        return new List<InventoryItemUI>(items).ToArray();
    }
    public IEnumerator SpawnItemscFromSave()
    {
        if (jsonManager.gameData.inventoryItems == null || jsonManager.gameData.inventoryItems.Length == 0)
        {
            Debug.Log("Сохранённые предметы не найдены.");
            yield break;
        }

        foreach (var saveData in jsonManager.gameData.inventoryItems)
        {
            if (!jsonManager.TryGetItem(saveData.itemGuid, out var itemData))
            {
                Debug.LogWarning($"GUID {saveData.itemGuid} не найден в ItemData. Пропускаем.");
                continue;
            }

            if (itemData.worldPrefab == null)
            {
                Debug.LogWarning($"Предмет {itemData.itemName} по GUID {itemData.guid} не имеет worldPrefab, пропускаем.");
                continue;
            }

            // Создаем временный объект в мире, чтобы получить UI
            GameObject spawned = GameObject.Instantiate(itemData.worldPrefab, Vector3.zero, Quaternion.identity);
            spawned.name = $"{itemData.itemName}_Spawned";

            Items_Behavior_2D newItem = spawned.GetComponent<Items_Behavior_2D>();
            newItem.itemData = itemData;
            newItem.ViewUpgradeModuleAndOtherComponent();
            newItem.StartCoroutine(newItem.CaptureRotatedObject());
            yield return StartCoroutine(WaitForCaptureComplete(newItem));

            // Активируем UI копию
            newItem.targetUI.SetActive(true);

            // Получаем InventoryItemUI на targetUI
            InventoryItemUI inventoryItemUI = newItem.targetUI.GetComponent<InventoryItemUI>();
            if (inventoryItemUI == null)
            {
                Debug.LogWarning($"InventoryItemUI не найден на targetUI предмета {itemData.itemName}");
                Destroy(spawned);
                continue;
            }

            // Прямое восстановление occupiedCells без DetermineTopLeft
            inventoryItemUI.occupiedCells = new InventoryCell[itemData.occupiedCells.Length];
            for (int i = 0; i < itemData.occupiedCells.Length; i++)
            {
                int x = saveData.topLeftCell.x + itemData.occupiedCells[i].x;
                int y = saveData.topLeftCell.y + itemData.occupiedCells[i].y;

                if (x >= 0 && y >= 0 && x < inventoryItemUI.BackpackGridBuilder.cells.GetLength(0)
                                 && y < inventoryItemUI.BackpackGridBuilder.cells.GetLength(1))
                {
                    InventoryCell cell = inventoryItemUI.BackpackGridBuilder.cells[x, y];
                    inventoryItemUI.occupiedCells[i] = cell;
                    cell.currentItem = inventoryItemUI;
                }
                else
                {
                    Debug.LogWarning($"Клетка ({x},{y}) для предмета {itemData.itemName} выходит за границы инвентаря.");
                }
            }

            // Центрируем визуально предмет по его ячейкам
            Vector3 centerPos = Vector3.zero;
            foreach (var cell in inventoryItemUI.occupiedCells)
                centerPos += cell.transform.position;
            centerPos /= inventoryItemUI.occupiedCells.Length;

            inventoryItemUI.transform.position = centerPos;

            // Восстанавливаем поворот
            inventoryItemUI.transform.localRotation = saveData.isRotated
                ? Quaternion.Euler(0, 0, -90f)
                : Quaternion.identity;

            // Удаляем временный объект в мире
            Destroy(spawned);

            Debug.Log($"Создан предмет: {itemData.itemName} по GUID: {itemData.guid}");
            yield return null; // Не блокируем фрейм
        }
    }
    private IEnumerator WaitForCaptureComplete(Items_Behavior_2D item)
    {
        while (!item.isCaptured)
        {
            yield return null; // ждем следующего кадра
        }
        Debug.Log("Корутина CaptureRotatedObject завершилась");
        // Тут можно продолжать работу после завершения
    }
}