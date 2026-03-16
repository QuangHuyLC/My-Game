using UnityEngine;

public class BoomerangFly : MonoBehaviour
{
    public float speed = 10f;       // Tốc độ bay
    public float flyTime = 0.5f;    // Thời gian bay ra trước khi quay đầu
    public float damage = 15f;

    private Rigidbody2D rb;
    private float timer;
    private bool isReturning = false;
    private Vector2 flyDirection;
    private Transform thrower;      // LƯU LẠI NGƯỜI NÉM ĐỂ TÌM ĐƯỜNG VỀ

    // Nhận hướng ném VÀ người ném
    public void Setup(Vector2 direction, Transform nguoiNem)
    {
        rb = GetComponent<Rigidbody2D>();
        flyDirection = direction;
        thrower = nguoiNem; 
        
        // Phóng đi
        rb.linearVelocity = flyDirection * speed; 
    }

    void Update()
    {
        // Hiệu ứng lốc xoáy
        transform.Rotate(0, 0, -1500 * Time.deltaTime);

        if (!isReturning)
        {
            // 1. GIAI ĐOẠN BAY RA
            timer += Time.deltaTime;
            if (timer >= flyTime)
            {
                isReturning = true; // Hết giờ bay ra, bắt đầu quay đầu
            }
        }
        else
        {
            // 2. GIAI ĐOẠN BAY VỀ (TÌM CHỦ)
            if (thrower != null) // Nếu chủ vẫn còn sống
            {
                // Dò hướng từ vị trí hiện tại về tay người ném
                Vector2 returnDir = (thrower.position - transform.position).normalized;
                rb.linearVelocity = returnDir * speed;

                // Nếu bay sát vào người ném (< 0.5 mét) -> Bắt được -> Tự hủy
                if (Vector2.Distance(transform.position, thrower.position) < 0.5f)
                {
                    Destroy(gameObject);
                }
            }
            else 
            {
                // Nếu quái ném xong bị sếp chém chết mất xác -> Bay lùi về theo đường thẳng rồi tự hủy
                rb.linearVelocity = -flyDirection * speed;
                Destroy(gameObject, 1f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // ĐÃ SỬA: Gọi đúng script PlayerHealth để trừ máu
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null) 
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
    }
}