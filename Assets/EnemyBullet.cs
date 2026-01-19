using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 20f;
    private Rigidbody2D rb;
    private bool isDeflected = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 3f); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Trúng người Player
        if (other.CompareTag("Player") && !isDeflected)
        {
            Playermovement pm = other.GetComponent<Playermovement>();
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            // KIỂM TRA ĐIỀU KIỆN NÉ ĐẠN
            if (pm != null)
            {
                // Nếu đang lộn (Shift) -> Bất tử, thoát luôn
                if (pm.isRolling) return;

                // Nếu đang ngồi (S) -> Bất tử, thoát luôn
                if (pm.isCrouching) return;
            }

            // Nếu không né, không lộn thì mới trừ máu
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
                Debug.Log("Player trúng đạn!");
                Destroy(gameObject); 
            }
        }
        // 2. Phản đòn
        else if (other.CompareTag("Enemy") && isDeflected)
        {
            Debug.Log("Kẻ địch bị phản đòn!");
            Destroy(other.gameObject); 
            Destroy(gameObject); 
        }
        // 3. Chạm đất
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    public void Deflect()
    {
        Debug.Log("Đã chém tan viên đạn!");
        Destroy(gameObject);
    }
}