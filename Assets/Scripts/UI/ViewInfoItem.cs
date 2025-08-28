using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

public class ViewInfoItem : MonoBehaviour
{
    [SerializeField] private GameObject viewItemImage;
    [SerializeField] private TextMeshProUGUI textlVL;
    [SerializeField] private GameObject currentImageItem;
    [SerializeField] private TextMeshProUGUI textDamageOrHealAmount_title;
    [SerializeField] private TextMeshProUGUI textDamageOrHealAmount_value;

    [SerializeField] private TextMeshProUGUI textCooldown_title;
    [SerializeField] private TextMeshProUGUI textCooldown_value;

    [SerializeField] private TextMeshProUGUI textFireRate_title;
    [SerializeField] private TextMeshProUGUI textFireRate_value;

    public void ViewAndUpdateInfoItem(InventoryItemUI itemUI)
    {
        ItemData data = itemUI.itemData;
        if (currentImageItem != null)
        {
            currentImageItem.SetActive(false);
            
        }
        textlVL.text = $"Lvl {(int)data.grade + 1}";
        currentImageItem = itemUI.gameObject;
        SetSizeAndPosition(currentImageItem, viewItemImage);

        switch (data.itemType)
        {
            case ItemType.Bomb:
                data = data as BombData;
                textDamageOrHealAmount_title.text = "Урон";
                textDamageOrHealAmount_value.text = data.attackDamage.ToString();
                break;

            case ItemType.Consumable:
                data = data as ConsumableData;
                textDamageOrHealAmount_title.text = "Хил";
                textFireRate_title.gameObject.SetActive(false);
                textFireRate_value.gameObject.SetActive(false);
                if (data.itemName.Contains("Painkiller")) // или другой маркер  
                {
                  
                    textDamageOrHealAmount_value.text = data.healAmount.ToString();
                }
                else if (data.itemName.Contains("Coffee"))
                {

                    textDamageOrHealAmount_value.text = data.healAmount.ToString();
                }
               
                
                break;

            case ItemType.Weapon:
                data = data as WeaponData;
                textDamageOrHealAmount_title.text = "Урон";
                if (data.itemName.Contains("M4"))
                {
                    textDamageOrHealAmount_value.text = data.attackDamage.ToString();
                }
                
                else if (data.itemName.Contains("G17"))
                {
                    textDamageOrHealAmount_value.text = data.attackDamage.ToString();
                }
                break;
        }
    }
    private void SetSizeAndPosition(GameObject targetUI, GameObject slot)
    {
        if (targetUI == null || slot == null) return;

        RectTransform rectTarget = targetUI.GetComponent<RectTransform>();
        RectTransform rectSlot = slot.GetComponent<RectTransform>();
        targetUI.SetActive(true);
        // Размер предмета в ячейках
        float width = rectSlot.rect.width;
        float height = rectSlot.rect.height;

        // Если нужно вращение (например, у автомата 1x3), меняем местами ширину и высоту
        rectTarget.rotation = Quaternion.identity;

        // Устанавливаем размеры напрямую
        rectTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
     
        // Ставим по центру слота
        rectTarget.position = rectSlot.position;
    }
}
