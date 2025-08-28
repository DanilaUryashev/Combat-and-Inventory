using UnityEngine;
using System;

public enum BackpackGrade { Grade1, Grade2, Grade3 }

[CreateAssetMenu(fileName = "New Backpack", menuName = "Inventory/Backpack Data")]
public class BackpackData : ScriptableObject
{
    [Header("Basic Settings")]
    public string backpackName;
    public BackpackGrade grade;
    public GameObject backpackPrefab; // ������ ������� (������)
    public Sprite backpackSprite;
    public GameObject cellPrefab;     // ������ ������ ���������

    [Header("Main Inventory Zone")]
    [Tooltip("�������� ��������� �������-���������")]
    public string mainInventoryZoneName = "ZoneInventory_1"; // ��� ��������� ����������
    [Tooltip("������ �������� (������ x ������)")]
    public Vector2Int size = Vector2Int.one;

    [Header("Additional Pockets")]
    public BackpackPocket[] additionalPockets; // �������������� ��������
}

[Serializable]
public class BackpackPocket
{
    [Tooltip("�������� ��������� �������-��������")]
    public string pocketObjectName; // ��������: "SidePocket", "FrontPocket"

    [Tooltip("������ �������� (������ x ������)")]
    public Vector2Int size = Vector2Int.one;
}