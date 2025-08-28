using UnityEngine;

public enum ItemType { Weapon, Consumable, Bomb }
public enum ItemGrade { Grade1, Grade2, Grade3 }
public enum ItemShapeType { Single, Vertical3, LShape, Square2x2 }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Settings")]
    [HideInInspector] public string guid; // уникальный ID дл€ сохранени€
    public string itemName;
    public ItemType itemType;
    public ItemGrade grade;
    public Sprite icon;

    [Header("Inventory Settings")]
    public ItemShapeType shapeType;
    public Vector2Int inventorySize = Vector2Int.one;
    public Vector2Int[] occupiedCells;

    [Header("Stats")]
    public int attackDamage;
    public int healAmount;
    public float cooldown;
    public float duration;

    [Header("Visuals")]
    public GameObject worldPrefab;
    public ParticleSystem useEffect;
    public AudioClip useSound;

    [TextArea] public string description;

    // јвтоматически рассчитываем зан€тые €чейки на основе типа формы
    protected virtual void OnValidate()
    {
        CalculateOccupiedCells();
        if (string.IsNullOrEmpty(guid))
            guid = System.Guid.NewGuid().ToString();
    }

    private void CalculateOccupiedCells()
    {
        switch (shapeType)
        {
            case ItemShapeType.Single:
                inventorySize = Vector2Int.one;
                occupiedCells = new Vector2Int[] { Vector2Int.zero };
                break;

            case ItemShapeType.Vertical3:
                inventorySize = new Vector2Int(1, 3);
                occupiedCells = new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                };
                break;

            case ItemShapeType.LShape:
                inventorySize = new Vector2Int(2, 2);
                occupiedCells = new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 0)
                };
                break;

            case ItemShapeType.Square2x2:
                inventorySize = new Vector2Int(2, 2);
                occupiedCells = new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1)
                };
                break;
        }
    }
}