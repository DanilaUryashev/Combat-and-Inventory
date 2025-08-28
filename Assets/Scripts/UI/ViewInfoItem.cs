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
                textDamageOrHealAmount_title.text = "����";
                textDamageOrHealAmount_value.text = data.attackDamage.ToString();
                break;

            case ItemType.Consumable:
                data = data as ConsumableData;
                textDamageOrHealAmount_title.text = "���";
                textFireRate_title.gameObject.SetActive(false);
                textFireRate_value.gameObject.SetActive(false);
                if (data.itemName.Contains("Painkiller")) // ��� ������ ������  
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
                textDamageOrHealAmount_title.text = "����";
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
        // ������ �������� � �������
        float width = rectSlot.rect.width;
        float height = rectSlot.rect.height;

        // ���� ����� �������� (��������, � �������� 1x3), ������ ������� ������ � ������
        rectTarget.rotation = Quaternion.identity;

        // ������������� ������� ��������
        rectTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
     
        // ������ �� ������ �����
        rectTarget.position = rectSlot.position;
    }
}
