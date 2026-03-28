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
    public float knockbackForce = 5f; 
    
    [Header("--- KHO ÂM THANH QUÁI ---")]
    public AudioSource audioSource;

    [Space(10)]
    public AudioClip hitSound;       // Tiếng bị chém trúng
    [Range(0f, 1f)] public float hitVolume = 1f;

    [Space(10)]
    public AudioClip deathSound;     // Tiếng gục ngã
    [Range(0f, 1f)] public float deathVolume = 1f;

    [Space(10)]
    public AudioClip[] footstepSounds; // Tiếng bước chân
    [Range(0f, 1f)] public float footstepVolume = 0.5f;

    [Space(10)]
    public AudioClip attackSound;      // Tiếng chém/bắn (hoặc ném)
    [Range(0f, 1f)] public float attackVolume = 1f;

    [Space(10)]
    [Tooltip("Dùng cho con ném Boomerang")]
    public AudioClip boomerangSound;   // Tiếng ném Boomerang (MỚI THÊM)
    [Range(0f, 1f)] public float boomerangVolume = 1f; // (MỚI THÊM)


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
        
        // Tự động tìm loa
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
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
            
            // ÂM THANH BỊ ĐÁNH
            if (hitSound != null && audioSource != null) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(hitSound, hitVolume); 
            }
            else if (hitSound != null) {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume); 
            }

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

        // 0. PHÁT ÂM THANH CHẾT TRƯỚC KHI BIẾN MẤT
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume); 
        }

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
            animator.StopPlayback();
            animator.Play("Die"); 
            thoiGianCho = deathAnimDuration; 
        }
        else if (animator != null)
        {
            animator.StopPlayback();
            animator.Play("Hurt"); 
            thoiGianCho = thoiGianChoHurt; 
        }

        StartCoroutine(QuyTrinhChet(thoiGianCho));
    }

    IEnumerator QuyTrinhChet(float thoiGianChoAnim)
    {
        yield return new WaitForSeconds(thoiGianChoAnim);

        float timer = 0;
        while (timer < thoiGianFlicker)
        {
            if (spriteRen != null) spriteRen.enabled = !spriteRen.enabled;
            yield return new WaitForSeconds(tocDoFlicker);
            timer += tocDoFlicker;
        }

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

    // =====================================================================
    // CÁC HÀM BÊN DƯỚI DÙNG ĐỂ CẮM VÀO ANIMATION EVENT
    // =====================================================================
    public void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.pitch = Random.Range(0.8f, 1.2f); 
            audioSource.PlayOneShot(footstepSounds[randomIndex], footstepVolume); 
        }
    }

    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(attackSound, attackVolume); 
        }
    }

    // HÀM MỚI THÊM CHO CON BOOMERANG
    public void PlayBoomerangSound()
    {
        if (boomerangSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(boomerangSound, boomerangVolume); 
        }
    }
}