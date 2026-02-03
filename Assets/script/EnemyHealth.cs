using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("--- CÀI ĐẶT MÁU ---")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("--- CÀI ĐẶT CHẾT (QUAN TRỌNG) ---")]
    [Tooltip("Tích vào = Chạy Die -> Nhấp nháy -> Mất. Bỏ tích = Chạy Hurt -> Nhấp nháy -> Mất")]
    public bool coAnimationDie = false; 

    [Header("--- THỜI GIAN ---")]
    public float thoiGianChoHurt = 0.5f;   // Độ dài Anim Hurt (dùng cho con Spider)
    public float deathAnimDuration = 1.0f; // Độ dài Anim Die (dùng cho con Sniper)
    
    [Header("--- HIỆU ỨNG NHẤP NHÁY ---")]
    public float thoiGianFlicker = 1.0f;   // Nhấp nháy trong bao lâu?
    public float tocDoFlicker = 0.1f;      // Tốc độ chớp tắt

    [Header("--- TỰ ĐỘNG TÌM SCRIPT AI ---")]
    public MeleeEnemy meleeAI;   
    public SniperEnemy sniperAI; 

    [Header("--- VISUAL ---")]
    public GameObject bloodEffect; 
    public AudioClip hitSound; 
    public float knockbackForce = 5f; 

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

        if (meleeAI == null) meleeAI = GetComponent<MeleeEnemy>();
        if (sniperAI == null) sniperAI = GetComponent<SniperEnemy>();
    }

    public void TakeDamage(float damage, Vector2 attackerPos)
    {
        if (isDead) return; 

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // CÒN SỐNG: HURT + ĐẨY LÙI
            if (animator != null) 
            {
                animator.ResetTrigger("attack"); 
                animator.SetTrigger("Hurt");
            }

            if (rb != null) {
                ToggleAI(false);
                rb.linearVelocity = Vector2.zero; 
                Vector2 direction = (transform.position - (Vector3)attackerPos).normalized;
                direction = new Vector2(direction.x, 0.2f).normalized; 
                rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse); 
                StartCoroutine(RecoverFromKnockback());
            }

            if (bloodEffect != null) Instantiate(bloodEffect, transform.position, Quaternion.identity);
            if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f);
            if (spriteRen != null) {
                spriteRen.color = Color.red;
                Invoke("ResetColor", 0.1f);
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Quái chết -> Tắt não -> Chạy Anim -> Nhấp nháy -> Mất");

        // 1. TẮT NÃO
        if (meleeAI != null) meleeAI.StopAttackImmediately();
        if (sniperAI != null) sniperAI.StopAttackImmediately();

        // 2. KHÓA VẬT LÝ
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // 3. TẮT VA CHẠM
        if (col != null) col.enabled = false;

        // 4. XỬ LÝ ANIMATION & BẮT ĐẦU QUY TRÌNH BIẾN MẤT
        float thoiGianCho = 0f;

        if (coAnimationDie && animator != null)
        {
            // TRƯỜNG HỢP CÓ ANIM DIE (Sniper)
            animator.StopPlayback();
            animator.Play("Die"); // Chạy Die
            thoiGianCho = deathAnimDuration; // Chờ hết anim Die
        }
        else if (animator != null)
        {
            // TRƯỜNG HỢP KHÔNG CÓ ANIM DIE (Spider)
            animator.StopPlayback();
            animator.Play("Hurt"); // Chạy Hurt đỡ
            thoiGianCho = thoiGianChoHurt; // Chờ hết anim Hurt
        }

        // Gọi chung 1 hàm xử lý nhấp nháy
        StartCoroutine(QuyTrinhChet(thoiGianCho));
    }

    IEnumerator QuyTrinhChet(float thoiGianChoAnim)
    {
        // GIAI ĐOẠN 1: Chờ Animation (Die hoặc Hurt) diễn xong
        yield return new WaitForSeconds(thoiGianChoAnim);

        // GIAI ĐOẠN 2: Nhấp nháy (Flicker)
        float timer = 0;
        while (timer < thoiGianFlicker)
        {
            if (spriteRen != null) spriteRen.enabled = !spriteRen.enabled;
            yield return new WaitForSeconds(tocDoFlicker);
            timer += tocDoFlicker;
        }

        // GIAI ĐOẠN 3: Biến mất vĩnh viễn
        Destroy(gameObject);
    }

    void ToggleAI(bool trangThai)
    {
        if (meleeAI != null) meleeAI.enabled = trangThai;
        if (sniperAI != null) sniperAI.enabled = trangThai;
    }

    IEnumerator RecoverFromKnockback()
    {
        yield return new WaitForSeconds(0.2f); 
        if (!isDead) ToggleAI(true);
    }

    void ResetColor() { if (spriteRen != null) spriteRen.color = Color.white; }
}