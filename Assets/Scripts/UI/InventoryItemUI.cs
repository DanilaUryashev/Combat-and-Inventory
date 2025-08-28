using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData;                  // ScriptableObject с occupiedCells и inventorySize
    public InventoryCell[] occupiedCells;      // Какие ячейки сейчас заняты этим предметом
    public BackpackGridBuilder BackpackGridBuilder;
    private RectTransform rectTransform;
    public bool aboutInventory = false;
    [Header("Dragging")]
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector3 originalPosition;
    private Vector2Int originalTopLeft;
    private Vector2 dragOffset;
    private bool hasSpawned = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        StorageAllObject storageAllObject = FindFirstObjectByType<StorageAllObject>();
        BackpackGridBuilder = storageAllObject.BackpackPanel.GetComponent<BackpackGridBuilder>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>(); // определяем канвас
        canvas = BackpackGridBuilder.GetComponentInParent<Canvas>();
        SetSize();
    }
    #region Dragging

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalTopLeft = occupiedCells.Length > 0 ? occupiedCells[0].gridPosition : Vector2Int.zero;
        canvasGroup.blocksRaycasts = false;

        // Считаем смещение курсора относительно центра объекта
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint);

        dragOffset = rectTransform.anchoredPosition - localPoint;

        // Освобождаем старые ячейки
        foreach (var cell in occupiedCells)
            cell.currentItem = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Перетаскиваем копию UI
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + dragOffset;
        }

        // Параллельно подсвечиваем ячейки по курсору
        Vector2Int hoveredCell = GetClosestCellFromScreenPosition(eventData.position + new Vector2(xoffcet, yoffcet));
        HighlightCells(hoveredCell);
        DropOutsideInventory(eventData);
    }
    public float xoffcet=450;//корды для правильного отображения
    public float yoffcet=-850;// 
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2Int hoveredCell = GetClosestCellFromScreenPosition(eventData.position+ new Vector2(xoffcet, yoffcet));

        // Пытаемся поставить предмет на сетку
        if (!TrySetPosition(hoveredCell))
        {
            // Если не получилось — возвращаем на исходное место
            rectTransform.anchoredPosition = originalPosition;
            TrySetPosition(originalTopLeft);
        }
        

        ResetAllCellColors();
        canvasGroup.blocksRaycasts = true;
        SaveInventory();
    }

   


    private void DropOutsideInventory(PointerEventData eventData)
    {
        // Проверяем, находится ли курсор над инвентарём
        Vector2 screenPos = eventData.position;
        bool overInventory = false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == BackpackGridBuilder.gameObject ||
                result.gameObject.transform.IsChildOf(BackpackGridBuilder.transform))
            {
                overInventory = true;
                break;
            }
        }

        if (!overInventory)
        {
            // Создаём префаб предмета в мире
            if (itemData.worldPrefab != null)
            {
                Vector3 worldPos = Vector3.zero;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvas.transform as RectTransform,
                    eventData.position,
                    canvas.worldCamera,
                    out Vector3 worldPoint))
                {
                    worldPos = worldPoint;
                }
                if (!hasSpawned)
                {
                    GameObject pref = Instantiate(itemData.worldPrefab, worldPos, Quaternion.identity);
                    Items_Behavior_2D items_Behavior = pref.GetComponent<Items_Behavior_2D>();
                    items_Behavior.itemData = itemData;
                    items_Behavior.ViewUpgradeModuleAndOtherComponent();
                }
            }
            SaveInventory();
            // Скрываем или деактивируем UI копию
            Destroy(gameObject);
            ResetAllCellColors();
        }
    }
    #endregion

    #region SetObjectWithInventory

    public Vector2Int GetClosestCellFromScreenPosition(Vector2 screenPos)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out Vector2 localPoint);

        float closestDistance = float.MaxValue;
        InventoryCell closestCell = null;

        foreach (var cell in BackpackGridBuilder.cells)
        {
            float distance = Vector2.Distance(localPoint, cell.GetComponent<RectTransform>().anchoredPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell != null ? closestCell.gridPosition : Vector2Int.zero;
    }

    // Настройка размера предмета по ширине, сохраняя пропорции
    public void SetSize()
    {
        // Размер предмета в ячейках
        float width = BackpackGridBuilder.cellSize.x * itemData.inventorySize.x;
        float height = BackpackGridBuilder.cellSize.y * itemData.inventorySize.y;

        // Если нужно вращение (например, у автомата 1x3), меняем местами ширину и высоту
        if (NeedsRotation())
        {
            float temp = width;
            width = height;
            height = temp;
        }

        // Устанавливаем размеры напрямую
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rectTransform.localScale = new Vector2(-rectTransform.localScale.x, rectTransform.localScale.y);
        UpdateRotation();
    }

    // Определяем, нужен ли поворот
    private bool NeedsRotation()
    {
        // Всё, что "выше чем шире", поворачиваем
        return itemData.inventorySize.y > itemData.inventorySize.x;
    }

    // Определение top-left ячейки исходя из hoveredCell и формы предмета
    public Vector2Int DetermineTopLeft(Vector2Int hoveredCell)
    {
        int minX = itemData.occupiedCells.Min(c => c.x);
        int maxX = itemData.occupiedCells.Max(c => c.x);
        int minY = itemData.occupiedCells.Min(c => c.y);
        int maxY = itemData.occupiedCells.Max(c => c.y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        int topLeftX = hoveredCell.x - width / 2;
        int topLeftY = hoveredCell.y - height / 2;

        // Проверяем границы сетки сверху/снизу
        if (topLeftY < 0) topLeftY = 0;
        if (topLeftY + height > BackpackGridBuilder.cells.GetLength(1))
            topLeftY = BackpackGridBuilder.cells.GetLength(1) - height;

        // Проверяем границы слева/справа
        if (topLeftX < 0) topLeftX = 0;
        if (topLeftX + width > BackpackGridBuilder.cells.GetLength(0))
            topLeftX = BackpackGridBuilder.cells.GetLength(0) - width;

        // Дальше можно умно сдвинуть topLeft, если hoveredCell находится не в центре
        // Например, если hoveredCell ближе к низу, topLeft сдвигаем вверх и т.д.
        // Можно вычислить смещение относительно центра предмета
        int offsetX = hoveredCell.x - (topLeftX + width / 2);
        int offsetY = hoveredCell.y - (topLeftY + height / 2);

        topLeftX += offsetX;
        topLeftY += offsetY;

        // Ещё раз ограничиваем границы сетки
        topLeftX = Mathf.Clamp(topLeftX, 0, BackpackGridBuilder.cells.GetLength(0) - width);
        topLeftY = Mathf.Clamp(topLeftY, 0, BackpackGridBuilder.cells.GetLength(1) - height);

        return new Vector2Int(topLeftX, topLeftY);
    }

    // Получение ближайшей ячейки к позиции курсора
    public Vector2Int GetClosestCell(Vector3 worldPos)
    {
        float closestDistance = float.MaxValue;
        InventoryCell closestCell = null;

        foreach (var cell in BackpackGridBuilder.cells)
        {
            float distance = Vector3.Distance(worldPos, cell.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell != null ? closestCell.gridPosition : Vector2Int.zero;
    }

    // Подсветка ячеек по форме и topLeft
    public void HighlightCells(Vector2Int hoveredCell)
    {
        ResetAllCellColors();

        Vector2Int topLeft = DetermineTopLeft(hoveredCell);
        List<InventoryCell> cellsToHighlight = new List<InventoryCell>();

        foreach (var offset in itemData.occupiedCells)
        {
            int x = topLeft.x + offset.x;
            int y = topLeft.y + offset.y;

            if (x < 0 || y < 0 || x >= BackpackGridBuilder.cells.GetLength(0) || y >= BackpackGridBuilder.cells.GetLength(1))
                continue;

            InventoryCell cell = BackpackGridBuilder.cells[x, y];
            cellsToHighlight.Add(cell);

            // Покрасить зелёным если свободна, красным если занята
            if (cell.currentItem == null)
                cell.Highlight(new Color(0, 1, 0, 0.5f));
            else
                cell.Highlight(new Color(1, 0, 0, 0.5f));
        }
    }

    // Установка позиции предмета по сетке
    public bool TrySetPosition(Vector2Int hoveredCell)
    {
        Vector2Int topLeft = DetermineTopLeft(hoveredCell);
        List<InventoryCell> cellsToOccupy = new List<InventoryCell>();

        foreach (var offset in itemData.occupiedCells)
        {
            int x = topLeft.x + offset.x;
            int y = topLeft.y + offset.y;

            // Проверяем границы
            if (x < 0 || y < 0 ||
                x >= BackpackGridBuilder.cells.GetLength(0) ||
                y >= BackpackGridBuilder.cells.GetLength(1))
            {
                return false; // выходит за пределы
            }

            InventoryCell cell = BackpackGridBuilder.cells[x, y];

            // Проверяем занятость
            if (cell.currentItem != null && cell.currentItem != this)
            {
                return false; // место занято другим предметом
            }

            cellsToOccupy.Add(cell);
        }

        // Если проверка пройдена — закрепляем
        occupiedCells = cellsToOccupy.ToArray();

        foreach (var cell in cellsToOccupy)
            cell.currentItem = this;

        // Центрируем предмет визуально
        Vector3 centerPos = Vector3.zero;
        foreach (var cell in cellsToOccupy)
            centerPos += cell.transform.position;
        centerPos /= cellsToOccupy.Count;

        rectTransform.position = centerPos;

        return true;
    }


    // Сброс подсветки всех ячеек
    public void ResetAllCellColors()
    {
        StorageAllObject storageAllObject = FindFirstObjectByType<StorageAllObject>();
        BackpackGridBuilder = storageAllObject.BackpackPanel.GetComponent<BackpackGridBuilder>();
        if (BackpackGridBuilder.gameObject.activeSelf)
        {
            foreach (var cell in BackpackGridBuilder.cells)
            {
                cell.ResetColor();
            }
        }
       
    }

    // Обновление вращения предмета исходя из формы
    private void UpdateRotation()
    {
        float widthCells = itemData.inventorySize.x;
        float heightCells = itemData.inventorySize.y;

        float angle = heightCells > widthCells ? -90f : 0f;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
    #endregion
    #region Inventory Save/Load

    /// <summary>
    /// Получение данных предмета для сохранения в GameData
    /// </summary>
    public InvenrotyItems GetInventoryData()
    {
        if (occupiedCells == null || occupiedCells.Length == 0) return null;

        return new InvenrotyItems
        {
            itemGuid = itemData.guid,
            topLeftCell = occupiedCells[0].gridPosition,
            isRotated = rectTransform.localEulerAngles.z != 0
        };
    }

    /// <summary>
    /// Сохраняем текущее состояние всех предметов инвентаря
    /// </summary>
    public void SaveInventory()
    {
        StorageAllObject storage = FindFirstObjectByType<StorageAllObject>();
        List<InvenrotyItems> list = new List<InvenrotyItems>();

        foreach (var item in BackpackGridBuilder.GetAllItems())
        {
            InvenrotyItems data = item.GetInventoryData();
            if (data != null)
                list.Add(data);
        }

        storage.jsonManager.gameData.inventoryItems = list.ToArray();
        storage.jsonManager.SaveGame();
    }

    /// <summary>
    /// Восстанавливаем предмет из сохранения (вызывается при загрузке сцены)
    /// </summary>
   

    #endregion

}
