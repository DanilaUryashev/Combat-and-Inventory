using UnityEngine;

[CreateAssetMenu(fileName = "New Bomb", menuName = "Inventory/Bomb Data")]
public class BombData : ItemData
{
    [Header("Bomb Specific")]
    public float explosionRadius = 3f;
    public float fuseTime = 3f;
    public GameObject explosionEffect;

    protected virtual void OnValidate()
    {
        base.OnValidate();
        itemType = ItemType.Bomb;
        shapeType = ItemShapeType.Square2x2;
        inventorySize = new Vector2Int(2, 2);

        // Автозаполнение damage в зависимости от грейда
        attackDamage = grade switch
        {
            ItemGrade.Grade1 => 10,
            ItemGrade.Grade2 => 20,
            _ => attackDamage
        };
    }
}