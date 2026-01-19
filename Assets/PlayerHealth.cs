using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // BẮT BUỘC CÓ ĐỂ DÙNG ẢNH UI
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt Máu")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Thanh Máu Pixel (MỚI THÊM)")]
    public Image healthBarImage;   // Kéo cái HealthBarImage (UI) vào đây
    public Sprite[] healthSprites; // Kéo 6 cái ảnh cắt ra vào đây (Element 0 -> 5)

    [Header("Hiệu ứng Trúng đạn")]
    public float knockbackForce = 10f; 
    public float stunDuration = 0.2f;  

    [Header("UI & Effect")]
    public CanvasGroup gameOverCanvasGroup;
    public GameObject bloodEffect;

    // Các thành phần cần thiết
    private Animator anim;
    private Playermovement playerMove;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        playerMove = GetComponent<Playermovement>();
        rb = GetComponent<Rigidbody2D>();

        // Cập nhật thanh máu ngay khi vào game cho đúng
        UpdateHealthUI();

        // Ẩn bảng Game Over lúc đầu
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }
    }

    // --- HÀM CẬP NHẬT ẢNH THANH MÁU ---
    void UpdateHealthUI()
    {
        // Kiểm tra xem đã gán đủ đồ chưa, chưa gán thì thôi
        if (healthBarImage == null || healthSprites.Length == 0) return;

        // 1. Tính phần trăm máu (Từ 0 đến 1)
        float percent = currentHealth / maxHealth; 
        
        // 2. Quy đổi ra số thứ tự ảnh (Index)
        // Máu 100% -> Ảnh 0
        // Máu 0% -> Ảnh cuối cùng
        int index = Mathf.FloorToInt((1f - percent) * (healthSprites.Length - 1));

        // Kẹp lại cho chắc ăn (không bị lỗi index quá đà)
        index = Mathf.Clamp(index, 0, healthSprites.Length - 1);

        // 3. Thay ảnh
        healthBarImage.sprite = healthSprites[index];
    }
    // -----------------------------------

    public void TakeDamage(float damage, Vector2 damageSourcePos)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Không cho âm máu

        // CẬP NHẬT LẠI ẢNH THANH MÁU NGAY
        UpdateHealthUI();
        
        // 1. HIỆU ỨNG NHÁY ĐỎ
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("ResetColor", 0.1f);

        // 2. LOGIC GIẬT LÙI & CHOÁNG (Nếu chưa chết)
        if (currentHealth > 0)
        {
            Vector2 knockbackDir = (transform.position - (Vector3)damageSourcePos).normalized;
            knockbackDir = new Vector2(knockbackDir.x, 0.2f).normalized; 
            StartCoroutine(KnockbackRoutine(knockbackDir));
        }

        if (currentHealth <= 0) Die();
    }

    void ResetColor() { GetComponent<SpriteRenderer>().color = Color.white; }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        if (playerMove != null && rb != null)
        {
            playerMove.isHurting = true; 
            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(stunDuration);
            playerMove.isHurting = false; 
        }
    }

    void Die()
    {
        isDead = true;
        if (anim != null) anim.SetTrigger("die");
        if (playerMove != null) {
            playerMove.enabled = false;
            playerMove.rb.linearVelocity = Vector2.zero;
        }
        GetComponent<CapsuleCollider2D>().enabled = false;
        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        if (bloodEffect != null) Instantiate(bloodEffect, transform.position, Quaternion.identity);
        StartCoroutine(FadeInGameOver(1.5f));
    }
    
    IEnumerator FadeInGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameOverCanvasGroup != null)
        {
            float duration = 1f;
            float currentTime = 0f;
            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
                yield return null;
            }
            gameOverCanvasGroup.alpha = 1f;
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
            Cursor.visible = true;
        }
    }
    
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}