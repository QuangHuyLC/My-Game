using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Chỉ số")]
    public float maxHealth = 50f;
    float currentHealth;

    [Header("Hiệu ứng")]
    public GameObject bloodEffect; 
    public AudioClip hitSound; 
    
    [Header("Vật lý")]
    public float knockbackForce = 5f; 
    public float stunDuration = 0.2f; 

    [Header("Setup Quái")]
    public MonoBehaviour movementScript; 
    public MonoBehaviour shootingScript; 

    [Header("Cài đặt lúc chết")]
    public float deathAnimDuration = 1.0f; 
    public int flickerCount = 2;           
    public float flickerSpeed = 0.1f;      

    private Rigidbody2D rb;
    private SpriteRenderer spriteRen;
    private Animator animator;      
    private Collider2D col;         
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRen = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); 
        col = GetComponent<Collider2D>();    

        // --- TỰ ĐỘNG TÌM SCRIPT (MỚI) ---
        // Sửa lỗi ông không kéo được script vào Inspector
        // Code sẽ tự tìm xem có script "SniperEnemy" không để gắn vào
        if (shootingScript == null) shootingScript = GetComponent("SniperEnemy") as MonoBehaviour;
        if (movementScript == null) movementScript = GetComponent("SniperEnemy") as MonoBehaviour;
    }

    public void TakeDamage(float damage, Vector2 attackerPos)
    {
        if (isDead) return; 

        // --- NGẮT CHIÊU KHI BỊ ĐÁNH ---
        // Bị chém là phải ngừng bắn ngay
        if (shootingScript != null) shootingScript.StopAllCoroutines();

        currentHealth -= damage;

        if (bloodEffect != null) Instantiate(bloodEffect, transform.position, Quaternion.identity);
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f);

        if (spriteRen != null) {
            spriteRen.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }

        if (rb != null && currentHealth > 0) {
            Vector2 direction = (transform.position - (Vector3)attackerPos).normalized;
            direction = new Vector2(direction.x, 0.2f).normalized;
            StartCoroutine(KnockbackRoutine(direction));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage) { TakeDamage(damage, transform.position + Vector3.right); }
    void ResetColor() { if (spriteRen != null) spriteRen.color = Color.white; }

    IEnumerator KnockbackRoutine(Vector2 dir) {
        if (rb != null && !isDead) {
            if (movementScript != null) movementScript.enabled = false;
            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse); 
            yield return new WaitForSeconds(stunDuration);
            if (!isDead) {
                rb.linearVelocity = Vector2.zero;
                if (movementScript != null) movementScript.enabled = true;
                // Nếu hồi phục thì cho phép bắn lại (nhưng phải ngắm lại từ đầu)
                if (shootingScript != null) shootingScript.enabled = true; 
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Quái chết - Cắt mọi lệnh bắn!");

        // 1. NGẮT NGAY LẬP TỨC MỌI COROUTINE (Quan trọng)
        // Lệnh này đảm bảo dù script chưa kịp hủy thì lệnh bắn cũng bị dừng
        StopAllCoroutines(); 
        if (shootingScript != null) shootingScript.StopAllCoroutines();

        // 2. Chạy Animation
        if (animator != null) {
            animator.ResetTrigger("attack"); // Bỏ tấn công
            animator.SetTrigger("die");      // Chết
        }

        // 3. HỦY DIỆT SCRIPT BẮN SÚNG
        if (movementScript != null) Destroy(movementScript);
        if (shootingScript != null) Destroy(shootingScript);

        // 4. Tắt va chạm & Vật lý
        if (col != null) col.enabled = false;
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.simulated = false; 
        }

        // 5. Bắt đầu quy trình biến mất
        StartCoroutine(DeathSequenceRoutine());
    }

    IEnumerator DeathSequenceRoutine()
    {
        // Chờ animation chết xong
        yield return new WaitForSeconds(deathAnimDuration);

        // Nháy 2 cái
        if (spriteRen != null)
        {
            for (int i = 0; i < flickerCount; i++)
            {
                spriteRen.enabled = false; 
                yield return new WaitForSeconds(flickerSpeed);
                spriteRen.enabled = true;  
                yield return new WaitForSeconds(flickerSpeed);
            }
        }

        // Biến mất
        Destroy(gameObject);
    }
}