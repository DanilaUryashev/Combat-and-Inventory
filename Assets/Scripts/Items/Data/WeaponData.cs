using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon Data")]
public class WeaponData : ItemData
{
    [Header("Weapon Specific")]
    public float attackRate = 1f;
    public float reloadTime = 2f;
    public int maxAmmo = 30;
    public bool isAutomatic = true;
    public GameObject projectilePrefab;
    public Vector3 shootOffset = new Vector3(0.5f, 0, 0);

    protected virtual void OnValidate()
    {
        base.OnValidate();
        itemType = ItemType.Weapon;
        // Автозаполнение damage в зависимости от грейда
        //attackDamage = grade switch
        //{
        //    ItemGrade.Grade1 => 3,
        //    ItemGrade.Grade2 => 10,
        //    ItemGrade.Grade3 => 15,
        //    _ => attackDamage
        //};
    }
}