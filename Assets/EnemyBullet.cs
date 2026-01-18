using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 10f; // Sát thương 10%
    private Rigidbody2D rb;
    private bool isDeflected = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Bay thẳng theo hướng trục X của đạn (transform.right)
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 3f); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Trúng người Player và đạn chưa bị chém
        if (other.CompareTag("Player") && !isDeflected)
        {
            // Lấy script Health từ Player để trừ máu
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage); 
            }

            Debug.Log("Player trúng đạn, trừ 10% máu!");
            Destroy(gameObject); // Đạn trúng người thì biến mất
        }
        // 2. Trúng kẻ địch SAU KHI đã bị phản đòn (Dành cho lúc ông muốn Deflect bay ngược lại)
        else if (other.CompareTag("Enemy") && isDeflected)
        {
            Debug.Log("Kẻ địch bị phản đòn!");
            Destroy(other.gameObject); // Tiêu diệt kẻ địch
            Destroy(gameObject); // Đạn biến mất
        }
        // 3. Chạm tường/Đất
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    // HÀM GỌI KHI CHÉM TRÚNG
    public void Deflect()
    {
        // TRƯỜNG HỢP A: Nếu ông muốn chém là đạn BIẾN MẤT LUÔN
        Debug.Log("Đã chém tan viên đạn!");
        Destroy(gameObject);

        /* // TRƯỜNG HỢP B: Nếu sau này ông muốn chém đạn BAY NGƯỢC LẠI (Katana Zero)
        // Hãy xóa dòng Destroy(gameObject) ở trên và dùng đoạn dưới này:
        
        isDeflected = true;
        rb.linearVelocity = -rb.linearVelocity * 1.5f; 
        transform.Rotate(0, 0, 180);
        GetComponent<SpriteRenderer>().color = Color.yellow;
        */
    }
}