using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
[System.Serializable]
public class UISlot
{
    public string name;          // для удобства в инспекторе
    public Button button;        // сама кнопка
    public TextMeshProUGUI countText;       // текст с количеством (если есть)
    public Image cooldownFill;   // картинка с fillAmount для отката
    [HideInInspector] public float cooldownEndTime;
    [HideInInspector] public float cooldownDuration;
}
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    public float maxHP = 100f;
    public float currentHP;
    public Image hpBar;

    [Header("UI Slots")]
    public UISlot pistolSlot;
    public UISlot autoSlot;
    public UISlot bombSlot;
    public UISlot healSlot;
    public UISlot coffeeSlot;
    public UISlot toggleAutoSlot;

    [Header("Inventory")]
    public int bombCount = 3;
    public int painkillerCount = 2;
    public int coffeeCount = 1;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float fireRatePistol = 1f;
    public float fireRateAuto = 0.3f;
    public float damagePistol;
    public float damageRifle;
    [SerializeField] private float rifleAttackRate = 0.1f; // задержка между пулями автомата
    private bool autoAttackEnabled = false;
    private float nextFirePistol;
    private float nextFireAuto;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float bombRadius = 5f;
    public float bombDamage = 30f;

    [Header("Heal")]
    public float healAmount = 20f;
    public float healAmountPainkiller = 20f;
    public float healAmountCoffe = 20f;
    [Header("Loot")]
    public List<GameObject> weaponDrops; // список оружий для выпадения

    private void UpdateDataItem()
    {
        StorageAllObject storageAllObject = FindFirstObjectByType<StorageAllObject>();
        GameManager gameManager = storageAllObject.canvas.GetComponent<GameManager>();
        Debug.Log("dsadasdasasd "+ gameManager.gameObject);
        damagePistol = gameManager.currentPistol.attackDamage;
        fireRatePistol = gameManager.currentPistol.cooldown;
        damageRifle = gameManager.currentRifle.attackDamage;
        fireRateAuto = gameManager.currentRifle.cooldown;
        rifleAttackRate = gameManager.currentRifle.attackRate;
        foreach (var c in gameManager.currentBombs)
        {
            bombCount = c.count;
            bombDamage = c.item.attackDamage;
            bombPrefab = c.item.worldPrefab;
        }
           
        foreach (var c in gameManager.currentCoffees)
        {
            coffeeCount = c.count;
            healAmountCoffe = c.item.healAmount;
        }
        foreach (var c in gameManager.currentPainkillers)
        {
            painkillerCount = c.count;
            healAmountPainkiller = c.item.healAmount;
        }
    }
    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();

        // подписка на событие смерти врага
        Enemy.OnEnemyDeath += HandleEnemyDeath;

        // кнопки
        if (pistolSlot.button) pistolSlot.button.onClick.AddListener(() => {
            Debug.Log("<color=red>Pressed pistol button");
            TryShootPistol();
        });
        if (autoSlot.button) autoSlot.button.onClick.AddListener(() =>
        {
            Debug.Log("<color=red>Pressed Auto button");
            TryShootAuto();
        });
        if (bombSlot.button) bombSlot.button.onClick.AddListener(() => TryUseBomb());
        if (healSlot.button) healSlot.button.onClick.AddListener(() => TryUseHeal(ref painkillerCount, healSlot));
        if (coffeeSlot.button) coffeeSlot.button.onClick.AddListener(() => TryUseHeal(ref coffeeCount, coffeeSlot));
        if (toggleAutoSlot.button) toggleAutoSlot.button.onClick.AddListener(ToggleAutoAttack);

        Debug.Log("PlayerController initialized");
        UpdateDataItem();
    }

    void Update()
    {
        if (autoAttackEnabled)
        {
            AutoAttack();
        }

        UpdateUI();
    }
    void TryShootPistol()
    {
        if (!IsSlotReady(pistolSlot)) return;

        GameObject target = FindClosestEnemy();
        if (target == null) return;

        Shoot(target, damagePistol);   // теперь берём из ItemData
        nextFirePistol = Time.time + fireRatePistol;
        StartCooldown(pistolSlot, fireRatePistol);

        Debug.Log("Pistol shot (manual)");
    }


    void TryShootAuto()
    {
        if (!IsSlotReady(autoSlot)) return;

        GameObject target = FindClosestEnemy();
        if (target == null) return;

        StartCoroutine(ShootBurst(target));

        nextFireAuto = Time.time + fireRateAuto; // общий откат оружия
        StartCooldown(autoSlot, fireRateAuto);

        Debug.Log("Auto shot burst (manual)");
    }
    private IEnumerator ShootBurst(GameObject target)
    {
        int bulletsInBurst = 3;
        for (int i = 0; i < bulletsInBurst; i++)
        {
            Shoot(target, damageRifle);
            yield return new WaitForSeconds(rifleAttackRate); // задержка между пулями
        }
    }

    void AutoAttack()
    {
        GameObject target = FindClosestEnemy();
        if (!target) return;

        if (Time.time > nextFirePistol)
        {
            TryShootPistol();
        }

        if (Time.time > nextFireAuto)
        {
            TryShootAuto();
        }
    }

    void Shoot(GameObject enemy, float damage)
    {
        if (!enemy) return;
        Debug.Log("Shoot projectile at " + enemy.name);
        GameObject b = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        b.GetComponent<Projectile>().Init(enemy.transform, damage);
    }

    void TryUseBomb()
    {
        if (bombCount <= 0 || !IsSlotReady(bombSlot)) return;
        bombCount--;

        Debug.Log("Bomb thrown!");

        GameObject bomb = Instantiate(bombPrefab, shootPoint.position, Quaternion.identity);

        // передаем параметры в Bomb.cs
        Bomb bombScript = bomb.AddComponent<Bomb>();
        Items_Behavior_2D item = bomb.GetComponent<Items_Behavior_2D>();
        Destroy(item);
        if (bombScript != null)
        {
            bombScript.Init(bombDamage, bombRadius);
        }

        StartCooldown(bombSlot, 2f); // 2 сек откат кнопки
    }

    void TryUseHeal(ref int count, UISlot slot)
    {
        if (count <= 0 || !IsSlotReady(slot)) return;
        count--;

        currentHP = Mathf.Min(currentHP + healAmount, maxHP);
        UpdateHPBar();

        Debug.Log("Heal used, new HP: " + currentHP);

        StartCooldown(slot, 5f); // например 5 сек откат
    }

    void ToggleAutoAttack()
    {
        autoAttackEnabled = !autoAttackEnabled;
        Debug.Log("Auto attack toggled: " + autoAttackEnabled);
        StartCooldown(toggleAutoSlot, 1f);
    }

    void StartCooldown(UISlot slot, float duration)
    {
        slot.cooldownDuration = duration;
        slot.cooldownEndTime = Time.time + duration;
        Debug.Log(slot.name + " cooldown started for " + duration + " sec");
    }

    bool IsSlotReady(UISlot slot)
    {
        return Time.time >= slot.cooldownEndTime;
    }

    void UpdateCooldownFill(UISlot slot)
    {
        if (slot.cooldownFill == null) return;

        if (slot.cooldownEndTime <= Time.time)
        {
            slot.cooldownFill.fillAmount = 0f;
        }
        else
        {
            float remaining = slot.cooldownEndTime - Time.time;
            slot.cooldownFill.fillAmount = remaining / slot.cooldownDuration;
        }
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
            hpBar.fillAmount = currentHP / maxHP;
    }

    void UpdateUI()
    {
        if (bombSlot.countText) bombSlot.countText.text = bombCount.ToString();
        if (healSlot.countText) healSlot.countText.text = painkillerCount.ToString();
        if (coffeeSlot.countText) coffeeSlot.countText.text = coffeeCount.ToString();

        UpdateCooldownFill(pistolSlot);
        UpdateCooldownFill(autoSlot);
        UpdateCooldownFill(bombSlot);
        UpdateCooldownFill(healSlot);
        UpdateCooldownFill(coffeeSlot);
        UpdateCooldownFill(toggleAutoSlot);
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e;
            }
        }
        if (closest) Debug.Log("Closest enemy: " + closest.name);
        return closest;
    }

    void HandleEnemyDeath(Enemy enemy)
    {
        Debug.Log("Enemy died: " + enemy.name);

        if (weaponDrops.Count > 0)
        {
            int rand = Random.Range(0, weaponDrops.Count);
            Instantiate(weaponDrops[rand], enemy.transform.position, Quaternion.identity);
            Debug.Log("Dropped weapon: " + weaponDrops[rand].name);
        }
    }
}
