using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt Máu")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Thanh Máu Pixel")]
    public Image healthBarImage;   
    public Sprite[] healthSprites; 

    [Header("Hiệu ứng Trúng đạn")]
    public float knockbackForce = 10f; 
    public float stunDuration = 0.2f;  

    [Header("UI & Effect")]
    public CanvasGroup gameOverCanvasGroup;
    public GameObject bloodEffect;
    public GameObject healEffect; 
    public GameObject gameplayUI; 

    // ==========================================
    // ---> Ổ CẮM ÂM THANH (MỚI THÊM) <---
    // ==========================================
    [Header("Âm thanh Hồi Máu")]
    public AudioSource audioSource;
    public AudioClip healSound;
    [Range(0f, 1f)] public float healVolume = 0.8f;
    // ==========================================

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
        
        // Tự động tìm cái mồm phát âm thanh trên người Ninja
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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

    void UpdateHealthUI()
    {
        if (healthBarImage == null || healthSprites.Length == 0) return;

        float percent = currentHealth / maxHealth; 
        int index = Mathf.FloorToInt((1f - percent) * (healthSprites.Length - 1));
        index = Mathf.Clamp(index, 0, healthSprites.Length - 1);
        healthBarImage.sprite = healthSprites[index];
    }

    public void TakeDamage(float damage, Vector2 damageSourcePos)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        UpdateHealthUI();
        
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("ResetColor", 0.1f);

        if (currentHealth > 0)
        {
            Vector2 knockbackDir = (transform.position - (Vector3)damageSourcePos).normalized;
            knockbackDir = new Vector2(knockbackDir.x, 0.2f).normalized; 
            StartCoroutine(KnockbackRoutine(knockbackDir));
        }

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        UpdateHealthUI();

        if (healEffect != null)
        {
            GameObject fx = Instantiate(healEffect, transform.position, Quaternion.identity, transform);
        }

        GetComponent<SpriteRenderer>().color = Color.green;
        Invoke("ResetColor", 0.15f);

        // ---> BẬT TIẾNG HỒI MÁU TẠI ĐÂY <---
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound, healVolume);
        }
    }

    void ResetColor() { GetComponent<SpriteRenderer>().color = Color.white; }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        if (playerMove != null && rb != null)
        {
            // 👉 [ĐÃ SỬA] CHỐT CHẶN Ở ĐÂY: Nếu đang bị Boss trói thì dẹp luôn vụ hất văng!
            if (playerMove.isLockedByBoss) yield break; 

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

        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (MusicManager.instance != null) MusicManager.instance.PlayNhacChet();

        Time.timeScale = 0f;

        if (anim != null) {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("die");
        }
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
        yield return new WaitForSecondsRealtime(delay); 

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
        Time.timeScale = 1f; 
        if (MusicManager.instance != null) MusicManager.instance.PlayNhacChinh();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}