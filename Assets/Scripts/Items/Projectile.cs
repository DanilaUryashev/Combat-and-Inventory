using TMPro;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;               // скорость пули
    private Vector3 direction;              // направление полёта
    private float damage;                   // урон

    [Header("Damage Text")]
    public GameObject damageTextPrefab;     // prefab Canvas с TextMeshPro
    public float damageTextRadius = 0.5f;  // радиус случайного смещения цифры

    // Инициализация пули: цель фиксируется в момент выстрела
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

        // движемся по фиксированной траектории
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

        // случайное смещение в радиусе
        Vector2 offset = Random.insideUnitCircle * damageTextRadius;
        Vector3 spawnPos = enemyPos + new Vector3(offset.x, offset.y + 1f, 0);

        // создаём Canvas prefab на позиции врага
        GameObject dmgCanvas = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        // внутри Canvas должен быть TextMeshProUGUI
        TextMeshProUGUI tmp = dmgCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
        }

        Destroy(dmgCanvas, 1f); // удаляем через 1 секунду
    }
}
