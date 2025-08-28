using UnityEngine;
using System;

public enum BackpackGrade { Grade1, Grade2, Grade3 }

[CreateAssetMenu(fileName = "New Backpack", menuName = "Inventory/Backpack Data")]
public class BackpackData : ScriptableObject
{
    [Header("Basic Settings")]
    public string backpackName;
    public BackpackGrade grade;
    public GameObject backpackPrefab; // Префаб рюкзака (визуал)
    public Sprite backpackSprite;
    public GameObject cellPrefab;     // Префаб ячейки инвентаря

    [Header("Main Inventory Zone")]
    [Tooltip("Название дочернего объекта-инвентаря")]
    public string mainInventoryZoneName = "ZoneInventory_1"; // Имя основного контейнера
    [Tooltip("Размер кармашка (ширина x высота)")]
    public Vector2Int size = Vector2Int.one;

    [Header("Additional Pockets")]
    public BackpackPocket[] additionalPockets; // Дополнительные кармашки
}

[Serializable]
public class BackpackPocket
{
    [Tooltip("Название дочернего объекта-кармашка")]
    public string pocketObjectName; // Например: "SidePocket", "FrontPocket"

    [Tooltip("Размер кармашка (ширина x высота)")]
    public Vector2Int size = Vector2Int.one;
}