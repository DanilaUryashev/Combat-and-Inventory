using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Items_Behavior_2D : MonoBehaviour
{
    [SerializeField] public ItemData itemData;
    [Header("Drag Settings")]
    public float dragSpeed = 15f;
    public float liftHeight = 0.5f;
    public LayerMask groundLayer = 1;
    public List<PolygonCollider2D> polygons;
    private PolygonCollider2D assignedPolygon;
    private static Dictionary<GameObject, PolygonCollider2D> assignedPolygons = new Dictionary<GameObject, PolygonCollider2D>();
    [Header("Click Settings")]
    public float clickThreshold = 0.1f; // ������������ ����������, ����� ��������� ������
    private Vector3 clickStartPos;

    private Camera mainCamera;
    private bool isDragging = false;
    private Rigidbody2D rb;
    private Vector3 offset;
    private int touchId = -1;
    private Vector3 dragPosition;

    [Header("Interaction of an object with inventory")]
    [SerializeField] private int baseTextureSize = 512; // ������� ������
    [SerializeField] public GameObject targetUI;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask renderLayer;
    [SerializeField] private float padding = 0.2f; // ����� ������ �������
    [SerializeField] private GameObject parent_inventoryGameobject; // ���� ��������� ����� ������
    [SerializeField] private GameObject gradePanel; // ������ ��� ��������� ���������
    [SerializeField] private GameObject backpackPanel; // ������ ���������(�������)
    [SerializeField] private GameObject itemInfoPanel; // ������ ���� � ��������

    [SerializeField] private GameObject firstLevel;
    [SerializeField] private GameObject secondLevel;
    [SerializeField] private GameObject thirdLevel;
    void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"������ ��������������� ��� {gameObject.name}");
        
    }
    private void Start()
    {
        ViewUpgradeModuleAndOtherComponent();
    }
    public void ViewUpgradeModuleAndOtherComponent()
    {
        StorageAllObject storageAllObject = FindFirstObjectByType<StorageAllObject>();
        parent_inventoryGameobject = storageAllObject.CopyUIItems;
        gradePanel = storageAllObject.CreatePanel;
        backpackPanel = storageAllObject.BackpackPanel;
        itemInfoPanel = storageAllObject.infoItemPanel;

        GameObject assignedPolygonsParent = storageAllObject.polygonGround;
        polygons.Clear();
        foreach (Transform child in assignedPolygonsParent.transform)
        {
            PolygonCollider2D collider = child.GetComponent<PolygonCollider2D>();
            if (collider != null)
            {
                polygons.Add(collider);
            }
        }

        Debug.Log($"������ ��������������� ��� {gameObject.name} asd sad as dasd asd ");
        if (itemData.grade == ItemGrade.Grade1) // ������ ���������
        {
            firstLevel.SetActive(true);
            secondLevel.SetActive(false);
            if(thirdLevel != null) 
            thirdLevel.SetActive(false);
        }
        else if (itemData.grade == ItemGrade.Grade2)
        {
            firstLevel.SetActive(false);
            secondLevel.SetActive(true);
            if (thirdLevel != null)
                thirdLevel.SetActive(false);
        }
        else if (itemData.grade == ItemGrade.Grade3)
        {
            firstLevel.SetActive(false);
            secondLevel.SetActive(false);
            if (thirdLevel != null)
                thirdLevel.SetActive(true);
        }
      //  transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        GameObject groundObject = GameObject.Find("Coliders_Ground");
        if (groundObject != null)
        {
            // �������� ��� PolygonCollider2D � ��������
            PolygonCollider2D[] allColliders = groundObject.GetComponentsInChildren<PolygonCollider2D>();
            polygons.AddRange(allColliders);
        }
        else
        {
            Debug.LogWarning("������ 'Coliders_Ground' �� ������ �� �����.");
        }
    }

    void Update()
    {
        //DebugInput(); // �������� ��� ������ ��� �������
        if (Application.isMobilePlatform)
        {
            HandleMobileInput();
        }
        else
        {
            HandleDesktopInput(); //  �������� � ���������
        }

        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    // ����� ��� ������� �����
    //    private void DebugInput()
    //    {
    //#if UNITY_EDITOR
    //        // ���������� ������� ���� ���������
    //        Vector2 mousePos = Input.mousePosition;
    //        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

    //        Debug.DrawLine(worldPos, worldPos + Vector2.up * 0.2f, Color.green, 0.1f);

    //        // ���������� ���������� ��� � ������� ����� �� �������� �������
    //        if (Time.frameCount % 60 == 0)
    //        {
    //            Debug.Log($" ����: �����({mousePos.x:F1}, {mousePos.y:F1}) -> ���({worldPos.x:F2}, {worldPos.y:F2})");

    //            // ��������� ������ ����
    //            Debug.Log($" ������: LMB={Input.GetMouseButton(0)} | RMB={Input.GetMouseButton(1)}");
    //        }
    //#endif

    //#if UNITY_ANDROID || UNITY_IOS
    //        // ���������� � �����
    //        if (Input.touchCount > 0 && Time.frameCount % 60 == 0)
    //        {
    //            Debug.Log($" �����: {Input.touchCount}");
    //            foreach (Touch touch in Input.touches)
    //            {
    //                Debug.Log($"   - {touch.fingerId}: {touch.phase} at {touch.position}");
    //            }
    //        }
    //#endif
    //    }
    #region DraggingObject
    private void HandleDesktopInput()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
           
            TryStartDrag(Input.mousePosition);
           
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            
            EndDrag();
        }
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && !isDragging)
                {
                    TryStartDrag(touch.position);
                    touchId = touch.fingerId;
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) &&
                         touch.fingerId == touchId && isDragging)
                {
                    EndDrag();
                }
            }
        }
    }

    private void TryStartDrag(Vector2 inputPosition)
    {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
        Debug.Log($"�������� �������: {inputPosition}  ������� �������: {worldPos}");

        // ���������� OverlapPointAll ������ Raycast ��� ����������
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

        if (hits.Length > 0)
        {
            Debug.Log($"������� �����������: {hits.Length}");

            foreach (Collider2D col in hits)
            {
                Debug.Log($"������� {col.gameObject.name} (����: {col.gameObject.layer})");

                if (col.gameObject == gameObject)
                {
                    clickStartPos = transform.position; // ��������� ������� ��� �����

                    StartCoroutine(CaptureRotatedObject());
                    Debug.Log($"������� �� {gameObject.name} ������� ");

                    // �������� ����� ��������� ����� Raycast ��� ��������
                    RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                    if (hit.collider != null)
                    {
                        StartDrag(inputPosition, hit.point);
                        Debug.Log($"������� {hit.collider.gameObject.name}");
                    }
                    else
                    {
                        StartDrag(inputPosition, worldPos);
                        Debug.Log(" Raycast �� ����� ������");
                    }
                    return;
                }
            }

            Debug.Log($"������ {gameObject.name} �� ����� ��������");
        }
        else
        {
            Debug.Log("�� ������ �� � ���� ��������� 2D");
        }
    }


    private void StartDrag(Vector2 inputPosition, Vector2 hitPoint)
    {
        isDragging = true;

        // ��������� offset
        offset = transform.position - mainCamera.ScreenToWorldPoint(inputPosition);

        // ��������� ������
        transform.position += Vector3.up * liftHeight;

        // ��������� ������
        if (rb != null)
        {
            rb.simulated = false;
        }

        // ������������� ��������� �������
        UpdateDragTarget(inputPosition);

    }
    private void OnStartDrag()
    {
        
        Collider2D itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null) return;

        // �������� �������� �������� �� ����� ����������
        foreach (PolygonCollider2D polygon in polygons)
        {
            if (polygon != null)
            {
                Physics2D.IgnoreCollision(itemCollider, polygon, false);
            }
        }
    }

    private void UpdateDragPosition()
    {
        // ������� �����������
        if (Vector3.Distance(transform.position, dragPosition) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, dragPosition, dragSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = dragPosition;
        }
        CheckIfDroppedInInventory();
    }

    private void UpdateDragTarget(Vector2 inputPosition)
    {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);

        // ���������� Raycast ��� ����������� ������� �� groundLayer
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, groundLayer);

        if (hit.collider != null)
        {
            dragPosition = hit.point + (Vector2)offset + Vector2.up * liftHeight;
        }
        else
        {
            // ���� �� ������ �� groundLayer, ���������� ������� ����
            dragPosition = worldPos + (Vector2)offset + Vector2.up * liftHeight;
        }
    }

    private Vector2 GetInputPosition()
    {
#if UNITY_ANDROID || UNITY_IOS
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == touchId)
            {
                return touch.position;
            }
        }
        return Input.mousePosition;
#else
        return Input.mousePosition;
#endif
    }

    private void EndDrag()
    {
        isDragging = false;
        touchId = -1;

        // �������� ������ �������
        if (rb != null)
        {
            rb.simulated = true;
        }

        // �������� �� ����
        if (Vector3.Distance(transform.position, clickStartPos) <= clickThreshold)
        {
            OnClick();
        }

        OnEndDrag(); // ��������������� �������������
        AssignRandomPolygon(); // ��������� �������
    }
    private void OnClick()
    {
        Debug.Log($"���� �� ��������: {gameObject.name}");
        if(backpackPanel.activeSelf) backpackPanel.SetActive(false);
        itemInfoPanel.SetActive(true);
        ViewInfoItem viewInfoItem = itemInfoPanel.GetComponent<ViewInfoItem>();
        viewInfoItem.ViewAndUpdateInfoItem(targetUI.GetComponent<InventoryItemUI>());
    }
    private void OnEndDrag()
    {
        // ��������������� ���������� �������������
        IgnoreOtherPolygonsPermanently();
        // ����� ��������� ������ � ������� ����
        InventoryItemUI inventoryItemUI = targetUI.GetComponent<InventoryItemUI>();
        if (inventoryItemUI.aboutInventory)
        {
            Vector2Int hoveredCell = inventoryItemUI.GetClosestCell(targetUI.transform.position);
            inventoryItemUI.TrySetPosition(hoveredCell);
            inventoryItemUI.ResetAllCellColors();
            inventoryItemUI.SaveInventory();
            Destroy(gameObject);
            
        }
      //  gradePanel.GetComponent<UpgradeItemBook>().TryAssignItemToSlot(this);

    }

    // ��� �������� ���������� � FixedUpdate
    void FixedUpdate()
    {
        if (isDragging)
        {
            UpdateDragTarget(GetInputPosition());
        }
    }
    private void CheckIfDroppedInInventory()
    {
        Vector2 screenPos = GetInputPosition();

        // ���������, ��������� �� ������ ��� UI ��������� ���������
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        InventoryItemUI inventoryItemUI = targetUI.GetComponent<InventoryItemUI>();

        foreach (var result in results)
        {
            //      Debug.Log(result);
            // ���������, �������� �� ������ ����������
          //  Debug.Log($"<color=red>result.gameObject   {result.gameObject}");
          //  Debug.Log($"<color=blue>backpackPanel  {backpackPanel}");
            if (result.gameObject == backpackPanel.transform.GetChild(0).gameObject)
            {
               if(!inventoryItemUI.aboutInventory) inventoryItemUI.aboutInventory =true;
                MovingCopy();
                // �������������� ��������: ������ ������, �������� � ��������� � �.�.
             //   Debug.Log("inventoru");
                return;
            }
            else if(result.gameObject ==  gradePanel)
            {
               //�� ��� ���� ������

                return;
            }
        }
        if (inventoryItemUI.aboutInventory) inventoryItemUI.aboutInventory = false;
        if (targetUI.gameObject.activeSelf) targetUI.gameObject.SetActive(false);
        inventoryItemUI.ResetAllCellColors();
     //   inventoryItemUI.SaveInventory();

    }
    #endregion

    #region dropingObject
    // ���������� ��� ���������� ��������
    private void AssignRandomPolygon()
    {
        if (polygons.Count == 0) return;

        // �������� ��������� �������
        assignedPolygon = polygons[Random.Range(0, polygons.Count)];

        // ��������� � ����������� �������
        if (assignedPolygons.ContainsKey(gameObject))
        {
            assignedPolygons[gameObject] = assignedPolygon;
        }
        else
        {
            assignedPolygons.Add(gameObject, assignedPolygon);
        }

        Debug.Log($" ������� {name} �������� ��: {assignedPolygon.gameObject.name}");

        // ���������� ������ �������� ��������
        IgnoreOtherPolygonsPermanently();
    }
    private void IgnoreOtherPolygonsPermanently()
    {
        Collider2D itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null) return;

        foreach (PolygonCollider2D polygon in polygons)
        {
            if (polygon != null)
            {
                // ���������� ��� �������� ����� ������������
                bool shouldIgnore = (polygon != assignedPolygon);
                Physics2D.IgnoreCollision(itemCollider, polygon, shouldIgnore);
            }
        }
    }

    // ���������� �������� � ������� ����������
    private System.Collections.IEnumerator IgnoreOtherPolygons(PolygonCollider2D targetPolygon)
    {
        yield return null; // ���� ���� ����

        Collider2D itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null) yield break;

        // ���������� ��� �������� ����� ����������
        foreach (PolygonCollider2D polygon in polygons)
        {
            if (polygon != null && polygon != targetPolygon)
            {
                Physics2D.IgnoreCollision(itemCollider, polygon, true);
            }
        }

        // ���������� ���������
        Debug.Log($"������� {name} �������� � {targetPolygon.gameObject.name}");

           }

    // ����� ��� ��������������� ���������� ��������
    public void AssignToSpecificPolygon(PolygonCollider2D polygon)
    {
        if (polygon != null && polygons.Contains(polygon))
        {
            assignedPolygon = polygon;
            StartCoroutine(IgnoreOtherPolygons(polygon));
        }
    }

    // �������� ������� ����������� �������
    public PolygonCollider2D GetAssignedPolygon()
    {
        return assignedPolygon;
    }

    #endregion

    #region Interaction with inventory
    #region �reate a copy for inventory
    public bool isCaptured = false; //���������� ��������
    public IEnumerator CaptureRotatedObject()
    {
        // ������� ������ �������������� �� ���� itemLayer
        //yield break;
        // yield return new WaitForSeconds(1f);
        SetLayerRecursively(gameObject, (int)Mathf.Log(renderLayer.value, 2));
        yield return null;
        // ��������� ������
        GameObject camObj = new GameObject("Temp2DCaptureCamera");
        Camera tempCamera = camObj.AddComponent<Camera>();
        tempCamera.enabled = false;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = new Color(0, 0, 0, 0);
        tempCamera.orthographic = true;
        tempCamera.cullingMask = renderLayer;

        camObj.transform.position = new Vector3(camObj.transform.position.x, camObj.transform.position.y, camObj.transform.position.z - 0.3f);
        // ����
        
        GameObject lightObj = new GameObject("TempLight");
        lightObj.transform.parent = camObj.transform;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
      
        // ���������
        Collider2D col = GetComponent<Collider2D>();
        Bounds bounds = col.bounds;

        // ������ ������� �� ��������� �������
        camObj.transform.rotation = transform.rotation;

        // ����� ������ �� ������ ����������
        //Vector3 objCenter = bounds.center;
        //camObj.transform.position = objCenter - camObj.transform.forward * 10f; // ������ �����

        // ������� ������� � padding
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("��� SpriteRenderer � ������� ��� ��� �����!");
            yield break;
        }

        // ��������� ������� ������� (� �������� ���� = 1)
        Vector2 spriteSize = sr.sprite.bounds.size;

        // ��������� ������� �������
        Vector3 scale = sr.transform.lossyScale;
        float width = spriteSize.x * scale.x * (1 + padding);
        float height = spriteSize.y * scale.y * (1 + padding);

        // ����� ������ �� ������ �������
        Vector3 objCenter = sr.bounds.center;
        camObj.transform.position = objCenter - camObj.transform.forward * 10f;

        // ������������ orthoSize ������ (���������, ����� ������ ������ ������)
        float aspect = (float)Screen.width / Screen.height;
        if (width / height > aspect)
            tempCamera.orthographicSize = width / (2f * aspect);
        else
            tempCamera.orthographicSize = height / 2f;

        // ����������� RenderTexture �������������, �������� 4
        int scaleFactor = 4;
        int texWidth = Mathf.CeilToInt(baseTextureSize * width / Mathf.Max(width, height) * scaleFactor);
        int texHeight = Mathf.CeilToInt(baseTextureSize * height / Mathf.Max(width, height) * scaleFactor);

        RenderTexture rt = new RenderTexture(texWidth, texHeight, 24);
        tempCamera.targetTexture = rt;

        tempCamera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        tempCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(camObj);

        

        // ������� ������
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        if (targetUI == null)
        {
            targetUI = new GameObject($"{gameObject.name}_UICopy");
            targetUI.transform.SetParent(parent_inventoryGameobject.transform, false);
            targetUI.gameObject.SetActive(false);
        }
        
        // ���������, ���� �� ��� ��������� Image
        Image image = targetUI.GetComponent<Image>();
        if (image == null)
        {
            // ���� ���������� ���, ��������� ���
            image = targetUI.AddComponent<Image>();
        }
        image.sprite = CropTransparentEdges(sprite);
        image.preserveAspect = true;
        isCaptured = true;
        // ��������� RectTransform
        RectTransform rtUI = targetUI.GetComponent<RectTransform>();
        rtUI.sizeDelta = new Vector2(width * 100 * scaleFactor, height * 100 * scaleFactor);
        //rtUI.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        SetLayerRecursively(gameObject,(int)Mathf.Log(itemLayer.value, 2));
        InventoryItemUI inventoryItemUI = targetUI.GetComponent<InventoryItemUI>();
        if(inventoryItemUI == null)
        {
            inventoryItemUI = targetUI.AddComponent<InventoryItemUI>();
        }
        inventoryItemUI.itemData = itemData;
        inventoryItemUI.SetSize();
        

    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private Sprite CropTransparentEdges(Sprite originalSprite) // ������� ���� ��� ��� ���� ��� ������ �������� ����������� ��� (����� ����� ������� � ������� ���� � �������� ���� ��� �������� �������)
    {
        Texture2D originalTexture = originalSprite.texture;
        Color[] pixels = originalTexture.GetPixels();

        int width = originalTexture.width;
        int height = originalTexture.height;

        // ������� ������� ������������ �������
        int left = width;
        int right = 0;
        int bottom = height;
        int top = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].a > 0.01f) // ���� ������� �� ����������
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < bottom) bottom = y;
                    if (y > top) top = y;
                }
            }
        }

        // ��������� ��������� ������ (1 �������)
        left = Mathf.Max(0, left - 1);
        right = Mathf.Min(width - 1, right + 1);
        bottom = Mathf.Max(0, bottom - 1);
        top = Mathf.Min(height - 1, top + 1);

        int croppedWidth = right - left + 1;
        int croppedHeight = top - bottom + 1;

        // ������� ����� �������� � ����������� ���������
        Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight);
        Color[] croppedPixels = originalTexture.GetPixels(left, bottom, croppedWidth, croppedHeight);
        croppedTexture.SetPixels(croppedPixels);
        croppedTexture.Apply();

        // ������� ����� ������
        Sprite croppedSprite = Sprite.Create(
            croppedTexture,
            new Rect(0, 0, croppedWidth, croppedHeight),
            new Vector2(0.5f, 0.5f),
            originalSprite.pixelsPerUnit
        );

        // ���������� ��������� �������� (�����������)
        Destroy(originalTexture);

        return croppedSprite;
    }
    #endregion
    #region interantion
    private void MovingCopy()
    {
        if (!targetUI.gameObject.activeSelf)
            targetUI.gameObject.SetActive(true);

        // ������� ������ �� �������� / ��������
        targetUI.transform.position = transform.position;

        
        InventoryItemUI inventoryItemUI = targetUI.GetComponent<InventoryItemUI>();
        // �������� ��������� ������ � ������� �������
        Vector2Int hoveredCell = inventoryItemUI.GetClosestCell(targetUI.transform.position);

        // ������������ ��� ������������� ������
        inventoryItemUI.HighlightCells(hoveredCell);
    }
    #endregion
    #endregion
}


