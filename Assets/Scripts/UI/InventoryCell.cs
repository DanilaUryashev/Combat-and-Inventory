using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryCell : MonoBehaviour
{
    public Vector2Int gridPosition;       // координата в сетке
    public InventoryItemUI currentItem;   // какой предмет занимает эту ячейку

    private Image image;
    private Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;
    }

    public void Highlight(Color color)
    {
        if (image != null)
            image.color = color;
    }

    public void ResetColor()
    {
        if (image != null)
            image.color = originalColor;
    }
}