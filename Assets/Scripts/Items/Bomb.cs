using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private float damage;
    private float radius;
    private Vector3 direction;
    private float speed = 10f;

    public void Init(float dmg, float rad)
    {
        damage = dmg;
        radius = rad;

        // ����������� = ����� �� ������ (��� ������)
        direction = Camera.main.transform.forward;

        // ��������� ������
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // ��������� ��������
        StartCoroutine(ExplodeAfterDelay(2f));
    }

    void Update()
    {
        // ������� ����� ������ ����
        transform.position += direction * speed * Time.deltaTime;
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        //    Debug.Log("Bomb exploded!");

        //    Collider2D[] hits = Physics.OverlapSphere(transform.position, radius);
        //    foreach (Collider2D hit in hits)
        //    {
        //        if (hit.CompareTag("Enemy"))
        //        {
        //            hit.GetComponent<Enemy>().TakeDamage(damage);
        //            Debug.Log("Bomb hit enemy: " + hit.name);
        //        }
        //    }

        //    // ������ ������ ��� (�������, ����)
        //    Destroy(gameObject);
    }
}