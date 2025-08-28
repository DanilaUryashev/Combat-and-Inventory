using TMPro;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;               // �������� ����
    private Vector3 direction;              // ����������� �����
    private float damage;                   // ����

    [Header("Damage Text")]
    public GameObject damageTextPrefab;     // prefab Canvas � TextMeshPro
    public float damageTextRadius = 0.5f;  // ������ ���������� �������� �����

    // ������������� ����: ���� ����������� � ������ ��������
    public void Init(Transform target, float damage)
    {
        this.damage = damage;
        if (target != null)
        {
            Collider2D col = target.GetComponent<Collider2D>();
            Vector3 targetPos = col != null ? col.bounds.center : target.position;
            direction = (targetPos - transform.position).normalized;
        }
        else
        {
            direction = Vector3.zero;
        }
    }

    void Update()
    {
        if (direction == Vector3.zero)
        {
            Destroy(gameObject);
            return;
        }

        // �������� �� ������������� ����������
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                ShowDamageText(damage, enemy.transform.position);
            }
            Destroy(gameObject);
        }
    }

    void ShowDamageText(float damage, Vector3 enemyPos)
    {
        if (damageTextPrefab == null) return;

        // ��������� �������� � �������
        Vector2 offset = Random.insideUnitCircle * damageTextRadius;
        Vector3 spawnPos = enemyPos + new Vector3(offset.x, offset.y + 1f, 0);

        // ������ Canvas prefab �� ������� �����
        GameObject dmgCanvas = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        // ������ Canvas ������ ���� TextMeshProUGUI
        TextMeshProUGUI tmp = dmgCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
        }

        Destroy(dmgCanvas, 1f); // ������� ����� 1 �������
    }
}
