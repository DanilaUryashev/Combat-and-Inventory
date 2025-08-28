using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class AutoGridFitter : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 minCellSize = new Vector2(100, 100); // минимальный размер €чейки
    public bool square = false; // делать €чейки квадратными?

    private GridLayoutGroup grid;

    void OnEnable()
    {
        grid = GetComponent<GridLayoutGroup>();
        Resize();
    }

    void Update()
    {
        if (!Application.isPlaying)
            Resize();
    }

    void OnRectTransformDimensionsChange()
    {
        Resize();
    }

    void Resize()
    {
        if (grid == null) return;

        RectTransform rt = GetComponent<RectTransform>();
        float width = rt.rect.width - (grid.padding.left + grid.padding.right);
        float height = rt.rect.height - (grid.padding.top + grid.padding.bottom);

        int columns = Mathf.FloorToInt((width + grid.spacing.x) / (minCellSize.x + grid.spacing.x));
        int rows = Mathf.FloorToInt((height + grid.spacing.y) / (minCellSize.y + grid.spacing.y));

        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        float cellWidth = (width - (grid.spacing.x * (columns - 1))) / columns;
        float cellHeight = (height - (grid.spacing.y * (rows - 1))) / rows;

        // если square = true берЄм минимальное значение, чтобы клетки стали квадратными
        if (square)
        {
            float size = Mathf.Min(cellWidth, cellHeight);
            cellWidth = cellHeight = size;
        }

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }
}
