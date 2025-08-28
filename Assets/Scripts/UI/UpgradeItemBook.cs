using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;



public enum SlotType
{
    FirstItemForUpgrade,
    SecondItemForUpgrade,
    ReceivedItem
}
[System.Serializable]
public class GradeItem
{
    public SlotType SlotTypeUpgrade;   // ��� �����
    public GameObject itemSlot;        // ���� UI, ���� ��������� �������
    public TextMeshProUGUI textLvl;    // ������� ����������
    [HideInInspector] public ItemData itemData;      // ������ �������� � �����
     public GameObject itemForUpgrade; // ��� ������� � ���� (������/�������)
}
public class UpgradeItemBook : MonoBehaviour
{
    public GradeItem firstSlot;
    public GradeItem secondSlot;
    public GradeItem receivedSlot;
    private bool hasSpawned = false;
    public Button buttonUpgrade;
    public Button buttonClose;
    public Button buttonInformation;
    private GameObject upgradeItem;
    [SerializeField] private List<Transform> instructionText;
    private GameObject backpack;
    private GameObject viewInfoPanel;

    public void ClosePanel()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            backpack.SetActive(false);
            viewInfoPanel.SetActive(false);

        }

    }
    private void Viewinstruction()
    {
        foreach (Transform t in instructionText)
        {
            if(t.gameObject.activeSelf) t.gameObject.SetActive(false);
            else t.gameObject.SetActive(true);
        }
    }

    // �������� ��� ���������� �������� �� ����
    public void TryAssignItemToSlot(Items_Behavior_2D item)
    {
        Debug.Log($"Assigned {item.itemData.name} to slot ");
        if (item == null) return;

        // ���������, ���� ����� �������
        if (IsOverSlot(item, firstSlot.itemSlot))
        {
            AssignItemToSlot(item, firstSlot);
        }
        else if (IsOverSlot(item, secondSlot.itemSlot))
        {
            AssignItemToSlot(item, secondSlot);
        }

        StartCoroutine(CheckForUpgrade());
    }

    private bool IsOverSlot(Items_Behavior_2D item, GameObject slot)
    {
        if (slot == null) return false;

        // ������� �������� ����������, ����� �������� �� RectTransformUtility.RectangleContainsScreenPoint
        Vector2 itemPos = Camera.main.WorldToScreenPoint(item.transform.position);
        Vector2 slotPos = Camera.main.WorldToScreenPoint(slot.transform.position);
        float radius = 50f; // ���������� ������ ���������
        return Vector2.Distance(itemPos, slotPos) <= radius;
    }
    private void Start()
    {
        if (buttonUpgrade) buttonUpgrade.onClick.AddListener(() => ButtonGrade());
        if (buttonClose) buttonClose.onClick.AddListener(() => ClosePanel());
        if (buttonInformation) buttonInformation.onClick.AddListener(() => Viewinstruction());
        StorageAllObject storageAllObject = FindFirstObjectByType<StorageAllObject>();
        backpack = storageAllObject.BackpackPanel;
        viewInfoPanel = storageAllObject.infoItemPanel;
    }
    private void AssignItemToSlot(Items_Behavior_2D item, GradeItem slot)
    {
        slot.itemData = item.itemData;
        if (slot.itemForUpgrade == null)
        { // ��� �� �������
            slot.itemForUpgrade = item.targetUI;
        }
        if (slot.textLvl != null)
            slot.textLvl.text = $"Lvl {(int)item.itemData.grade + 1}";

        Debug.Log($"Assigned {item.itemData.name} to slot {slot.itemSlot.name}");
        SetSizeAndPosition(slot.itemForUpgrade, slot.itemSlot);
        // ����� ������ ��� ���������� ������� � ����, ���� �����
        Destroy(item.gameObject);
    }

    private void SetSizeAndPosition(GameObject targetUI, GameObject slot)
    {

        if (targetUI == null || slot == null) return;

        RectTransform rectTarget = targetUI.GetComponent<RectTransform>();
        RectTransform rectSlot = slot.GetComponent<RectTransform>();

        if (rectTarget == null || rectSlot == null) return;
        targetUI.SetActive(true);
        // ������������� ������ ��� ����
        rectTarget.sizeDelta = rectSlot.sizeDelta;

        // ������ ������� �� ������ �����
        rectTarget.position = rectSlot.position;

        // ���������� ��������� �������, ����� �� ����������
        rectTarget.localScale = Vector3.one;

        // ����� �������� ��������� �������� ���������, ���� �����
        // targetUI.transform.localScale = Vector3.zero;
        // targetUI.transform.LeanScale(Vector3.one, 0.3f); // ���� ���� LeanTween


    }

    private IEnumerator CheckForUpgrade()
    {
        // ��������� ��� �����
        if (firstSlot.itemData != null && secondSlot.itemData != null)
        {
            // ���� ��������� ItemType
            if (firstSlot.itemData.itemType == secondSlot.itemData.itemType && !hasSpawned)
            {
                Debug.Log("Items match! Upgrading...");
                Debug.Log($"<color=red>Items/{firstSlot.itemData.itemName}");
                Vector3 worldPos = (firstSlot.itemSlot.transform.position + secondSlot.itemSlot.transform.position) / 2f;


                ItemData itemDataNewItem = null;
                switch (firstSlot.itemData.itemType) 
                {
                    case ItemType.Weapon:
                        WeaponData[] weapons = Resources.LoadAll<WeaponData>($"Items/{firstSlot.itemData.itemName}");
                        foreach (var w in weapons)
                        {
                            // ���������� ��� � �����
                            if (w.grade == firstSlot.itemData.grade + 1)
                            {
                                itemDataNewItem = w;
                                break;
                            }
                        }
                        break;
                    case ItemType.Bomb:
                        BombData[] bomb = Resources.LoadAll<BombData>($"Items/{firstSlot.itemData.itemName}");
                        foreach (var w in bomb)
                        {
                            // ���������� ��� � �����
                            if (w.grade == firstSlot.itemData.grade + 1)
                            {
                                itemDataNewItem = w;
                                break;
                            }
                        }
                        break;
                    case ItemType.Consumable:
                        ConsumableData[] consumable = Resources.LoadAll<ConsumableData>($"Items/{firstSlot.itemData.itemName}");
                        foreach (var w in consumable)
                        {
                            // ���������� ��� � �����
                            if (w.grade == firstSlot.itemData.grade + 1)
                            {
                                itemDataNewItem = w;
                                break;
                            }
                        }
                        break;
                    
                }
                // ������ �������
                Debug.Log($"<color=red>��� ��������: {itemDataNewItem.name}, ������� �������� {itemDataNewItem.grade}, � ��� �������: {firstSlot.itemData.grade + 1}");
                GameObject newObj = Instantiate(itemDataNewItem.worldPrefab, worldPos, Quaternion.identity);
                Items_Behavior_2D newItem = newObj.GetComponent<Items_Behavior_2D>();
                newItem.itemData = itemDataNewItem;
                newItem.ViewUpgradeModuleAndOtherComponent();
                newItem.StartCoroutine(newItem.CaptureRotatedObject());
                StartCoroutine(WaitForCaptureComplete(newItem));
                yield return null;
                Debug.Log("<color=red> dfsfdsfdsfdsfdfSdfdsfdsfdsfsdfdsfdfsfdsfdsfdsfdfSdfdsfdsfdsfsdfdsf");
                newItem.targetUI.SetActive(true);
                receivedSlot.itemForUpgrade = newItem.targetUI;
                SetSizeAndPosition(newItem.targetUI, receivedSlot.itemSlot);
                receivedSlot.textLvl.text = $"Lvl {(int)newItem.itemData.grade + 1}";
                // ���� grade = 2 ��� 3, �������� prefab �� Resources
                //if (newItem.itemData.grade == ItemGrade.Grade2)
                //{
                //    GameObject upgradedPrefab = Resources.Load<ItemData>($"UpgradedItems/{newItem.itemData.name}");
                //    if (upgradedPrefab != null)
                //    {
                //        GameObject temp = Instantiate(upgradedPrefab, worldPos, Quaternion.identity);
                //        temp.name = "piska";
                //        Destroy(newObj);
                //        newObj = temp;
                //        newItem = temp.GetComponent<Items_Behavior_2D>();
                //        newItem.itemData = firstSlot.itemData;
                //        newItem.ViewUpgradeModuleAndOtherComponent();
                //    }
                //}

                //firstSlot.itemForUpgrade = newObj;
                //secondSlot.itemForUpgrade = newObj;
                upgradeItem = newObj;
                hasSpawned = true;
                newObj.SetActive(false);
                Debug.Log($"Upgraded item spawned at {worldPos}");
                
            }
        }
    }
    private void ButtonGrade()
    {
        upgradeItem.transform.position = receivedSlot.itemSlot.transform.position;
        upgradeItem.SetActive(true);
        //������� ������
        Destroy(firstSlot.itemForUpgrade);
        Destroy(secondSlot.itemForUpgrade);
        Destroy(receivedSlot.itemForUpgrade);
    }
    private IEnumerator WaitForCaptureComplete(Items_Behavior_2D item)
    {
        while (!item.isCaptured)
        {
            yield return null; // ���� ���������� �����
        }
        Debug.Log("�������� CaptureRotatedObject �����������");
        // ��� ����� ���������� ������ ����� ����������
    }
}
