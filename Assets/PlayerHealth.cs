using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện để load lại màn chơi

public class PlayerHealth : MonoBehaviour
{
    [Header("Cài đặt máu")]
    public float maxHealth = 100f;     // Máu tối đa
    public float currentHealth;        // Máu hiện tại

    [Header("Hiệu ứng")]
    public SpriteRenderer spriteRenderer; // Để làm hiệu ứng nháy đỏ khi trúng đòn

    void Start()
    {
        // Khi bắt đầu game, đặt máu về 100%
        currentHealth = maxHealth;
        
        // Tự động tìm SpriteRenderer nếu chưa kéo vào
        if (spriteRenderer == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Hàm này sẽ được viên đạn (Bullet) gọi khi chạm vào người ông
    public void TakeDamage(float damageAmount)
    {
        // Trừ máu
        currentHealth -= damageAmount;
        Debug.Log("Máu hiện tại: " + currentHealth + "%");

        // Chạy hiệu ứng nháy đỏ
        StartCoroutine(FlashRed());

        // Kiểm tra nếu hết máu
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player đã tử trận!");
        
        // Vô hiệu hóa nhân vật để không điều khiển được nữa
        // GetComponent<PlayerMovement>().enabled = false; 

        // Load lại màn chơi sau 1 giây (đúng chất Katana Zero)
        Invoke("RestartLevel", 1f);
    }

    void RestartLevel()
    {
        // Lấy tên màn chơi hiện tại và load lại từ đầu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Coroutine làm nhân vật nháy đỏ khi trúng đòn cho dễ nhận biết
    System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}