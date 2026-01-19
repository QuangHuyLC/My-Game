using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Chỉ số")]
    public float maxHealth = 50f;
    float currentHealth;

    [Header("Hiệu ứng Trúng đòn")]
    public GameObject bloodEffect; // Kéo Prefab Máu vào đây
    public float knockbackForce = 5f; // Lực đẩy lùi
    public float stunDuration = 0.2f; // Thời gian bị đơ

    private Rigidbody2D rb;
    private SpriteRenderer spriteRen;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRen = GetComponent<SpriteRenderer>();
    }

    // --- HÀM CHÍNH: Nhận sát thương + Vị trí người đánh ---
    public void TakeDamage(float damage, Vector2 attackerPos)
    {
        currentHealth -= damage;

        // 1. Hiệu ứng máu
        if (bloodEffect != null)
        {
            Instantiate(bloodEffect, transform.position, Quaternion.identity);
        }

        // 2. Nháy đỏ
        if (spriteRen != null)
        {
            spriteRen.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }

        // 3. Đẩy lùi (Knockback)
        if (rb != null && currentHealth > 0)
        {
            // Tính hướng bay: Từ Người đánh -> Về phía Quái
            Vector2 direction = (transform.position - (Vector3)attackerPos).normalized;
            // Nảy lên một chút (y = 0.2) cho đẹp
            direction = new Vector2(direction.x, 0.2f).normalized;
            
            StartCoroutine(KnockbackRoutine(direction));
        }

        if (currentHealth <= 0) Die();
    }

    // --- HÀM DỰ PHÒNG (Tránh lỗi đỏ nếu file khác gọi thiếu vị trí) ---
    public void TakeDamage(float damage)
    {
        // Nếu lỡ gọi hàm này, coi như bị đánh từ bên trái sang
        TakeDamage(damage, transform.position + Vector3.left);
    }

    void ResetColor()
    {
        if (spriteRen != null) spriteRen.color = Color.white;
    }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Dừng lại (Dùng .velocity nếu Unity cũ)
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse); // Đẩy
            
            yield return new WaitForSeconds(stunDuration);
            
            rb.linearVelocity = Vector2.zero; // Hết choáng thì dừng
        }
    }

    void Die()
    {
        if (bloodEffect != null) Instantiate(bloodEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}