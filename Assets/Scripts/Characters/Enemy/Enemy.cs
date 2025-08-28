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
    public GameObject damageTextPrefab; // ������ TextMeshPro
    public float damageTextRadius = 0.5f; // ������ ���������� �������� �����

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

        // ������ ��������� �������� � �������
        Vector2 offset = Random.insideUnitCircle * damageTextRadius;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y + 1f, 0); // +1 �� Y ����� ��� �������

        GameObject dmgText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        TextMeshPro tmp = dmgText.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
        }

        Destroy(dmgText, 1f); // ������� ����� 1 ���
    }

    void Die()
    {
        Debug.Log("Enemy died: " + gameObject.name);
        OnEnemyDeath?.Invoke(this);
        Destroy(gameObject);
    }

    // Gizmos ��� ������� ������ (������ ��� ���������)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, damageTextRadius);
    }
}
