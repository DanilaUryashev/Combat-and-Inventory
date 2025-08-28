using System;
using UnityEngine;


public class Camp : MonoBehaviour
{
    [Header("Available Backpacks")]
    public BackpackData[] backpackOptions; // ��� ��������� �������
    public int selectedBackpackIndex = 0;  // ������ ���������� �������

    [Header("UI References")]
    public Transform backpackPanel;      // �������� ��� ������� � �������

    private GameObject currentBackpackInstance;
    [Header("Grid Builder")]
    [SerializeField] private BackpackGridBuilder gridBuilder;
    private void Start()
    {
        // ������������� ��������� ��� BackpackData �� ����� Resources
        backpackOptions = Resources.LoadAll<BackpackData>("Items/Backpack");
        gridBuilder = GetComponent<BackpackGridBuilder>();
        // ���������� ��������� ������ ��� ������
        ShowSelectedBackpack();
    }
    private void OnValidate()
    {
        ShowSelectedBackpack();
    }

    #region ShowSelectedBackpack
    // ����� ��� ����������� ���������� �������
    public void ShowSelectedBackpack()
    {
        // ��������� ���� �� ������� ��� ������
        if (backpackOptions == null || backpackOptions.Length == 0)
        {
            Debug.LogWarning("��� ��������� ��������!");
            return;
        }

        // ������������ ������ � �������� �������
        selectedBackpackIndex = Mathf.Clamp(selectedBackpackIndex, 0, backpackOptions.Length - 1);

        // �������� ��������� ������
        BackpackData selectedBackpack = backpackOptions[selectedBackpackIndex];

        // ������� � ���������� ������
        CreateBackpackUI(selectedBackpack);
    }

    // �������� UI �������
    private void CreateBackpackUI(BackpackData backpackData)
    {
        // ������� ���������� ������ ���� ����
        if (currentBackpackInstance != null)
        {
            Destroy(currentBackpackInstance);
        }

        // ��������� ���� �� ������
        if (backpackData.backpackPrefab == null)
        {
            Debug.LogError($"� ������� {backpackData.backpackName} ��� UI �������!");
            return;
        }

        // ������� ��������� ������� � �������
        currentBackpackInstance = Instantiate(backpackData.backpackPrefab, backpackPanel);
        currentBackpackInstance.name = backpackData.backpackName;

        Debug.Log($"������ ������: {backpackData.backpackName}");
        FindAndBuildInventoryZones(currentBackpackInstance, backpackData);
    }

    // ����� ��� ������������ �������� (����� �������� �� ����������)
    public void SelectBackpack(int index)
    {
        selectedBackpackIndex = index;
        ShowSelectedBackpack();
    }

    // ����� ��� ���������� �������
    public void SelectNextBackpack()
    {
        selectedBackpackIndex = (selectedBackpackIndex + 1) % backpackOptions.Length;
        ShowSelectedBackpack();
    }

    // ����� ��� ����������� �������
    public void SelectPreviousBackpack()
    {
        selectedBackpackIndex = (selectedBackpackIndex - 1 + backpackOptions.Length) % backpackOptions.Length;
        ShowSelectedBackpack();
    }
    #endregion
    #region
    private void FindAndBuildInventoryZones(GameObject backpackInstance, BackpackData backpackData)
    {
        // ���� �������� ���� ���������
        Transform mainZone = FindChildRecursive(backpackInstance.transform, backpackData.mainInventoryZoneName);
        if (mainZone != null && gridBuilder != null)
        {
            gridBuilder.BuildInventoryGrid(mainZone, backpackData.size, "�������� ���������", backpackData.cellPrefab);
        }

        // ���� �������������� ��������
        if (backpackData.additionalPockets != null)
        {
            foreach (var pocket in backpackData.additionalPockets)
            {
                Transform pocketZone = FindChildRecursive(backpackInstance.transform, pocket.pocketObjectName);
                if (pocketZone != null && gridBuilder != null)
                {
                    gridBuilder.BuildInventoryGrid(pocketZone, pocket.size, pocket.pocketObjectName, backpackData.cellPrefab);
                }
            }
        }
    }

    // ��������������� ������� ��� ������ �������� ��������
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
    #endregion
}