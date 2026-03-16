using UnityEngine;

public class MeleeSlash : MonoBehaviour
{
    public float damage = 20f;
    public float timeToLive = 0.3f; // Sống 0.3 giây rồi biến mất

    void Start()
    {
        // Lệnh này bắt buộc nó tự hủy cái game object này sau đúng timeToLive giây
        Destroy(gameObject, timeToLive);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // ĐÃ SỬA: Tìm đúng script PlayerHealth để gọi hàm trừ máu
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null) 
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
    }
}