using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 50f;
    public float currentHP;

    [Header("UI")]
    public Image hpBar;

    [Header("Damage Text")]
    public GameObject damageTextPrefab; // префаб TextMeshPro
    public float damageTextRadius = 0.5f; // радиус случайного смещения цифры

    public static System.Action<Enemy> OnEnemyDeath;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        UpdateHPBar();
        ShowDamageText(damage);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
            hpBar.fillAmount = currentHP / maxHP;
    }

    void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null) return;

        // создаём случайное смещение в радиусе
        Vector2 offset = Random.insideUnitCircle * damageTextRadius;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y + 1f, 0); // +1 по Y чтобы над головой

        GameObject dmgText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        TextMeshPro tmp = dmgText.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
        }

        Destroy(dmgText, 1f); // удаляем через 1 сек
    }

    void Die()
    {
        Debug.Log("Enemy died: " + gameObject.name);
        OnEnemyDeath?.Invoke(this);
        Destroy(gameObject);
    }

    // Gizmos для радиуса текста (только для редактора)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, damageTextRadius);
    }
}
