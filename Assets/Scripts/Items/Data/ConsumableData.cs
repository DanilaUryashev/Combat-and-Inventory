using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable Data")]
public class ConsumableData : ItemData
{
    [Header("Consumable Specific")]
    public bool isInstantUse = true;
    public float effectRadius = 0f;

    protected virtual void OnValidate()
    {
        base.OnValidate();
        itemType = ItemType.Consumable;
        // Автозаполнение healAmount в зависимости от грейда
        if (itemName.Contains("Кофе") || itemName.Contains("Coffee"))
        {
            healAmount = grade switch
            {
                ItemGrade.Grade1 => 3,
                ItemGrade.Grade2 => 10,
                _ => healAmount
            };
        }
        else if (itemName.Contains("Обезболивающее") || itemName.Contains("Painkiller"))
        {
            healAmount = grade switch
            {
                ItemGrade.Grade1 => 10,
                ItemGrade.Grade2 => 20,
                _ => healAmount
            };
        }
    }
   
}