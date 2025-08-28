using UnityEngine;

public class StorageAllObject : MonoBehaviour
{
    public GameObject BackpackPanel;
    public GameObject CreatePanel;
    public GameObject CopyUIItems;
    public GameObject polygonGround;
    public GameObject canvas;
    public GameObject infoItemPanel;
    public JsonManager jsonManager;
    //храним тут все объекты которые нужно найти скриптом, даже выключенные
    private void Start()
    {
        if (jsonManager == null)
            jsonManager = GetComponent<JsonManager>();
    }

    public void UpdateCampItemData()
    {
        jsonManager.gameData.campItems.weaponGuids.Clear();
        jsonManager.gameData.campItems.bombGuids.Clear();
        jsonManager.gameData.campItems.consumableGuids.Clear();

        Items_Behavior_2D[] allItems = Resources.FindObjectsOfTypeAll<Items_Behavior_2D>();
        foreach (Items_Behavior_2D item in allItems)
        {
            ItemData data = item.itemData;
            if (data == null) continue;

            switch (data.itemType)
            {
                case ItemType.Bomb:
                    jsonManager.gameData.campItems.bombGuids.Add(data.guid);
                    break;

                case ItemType.Consumable:
                    jsonManager.gameData.campItems.consumableGuids.Add(data.guid);
                    break;

                case ItemType.Weapon:
                    jsonManager.gameData.campItems.weaponGuids.Add(data.guid);
                    break;
            }
        }

        jsonManager.SaveGame();
    }

}
