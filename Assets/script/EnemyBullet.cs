using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyBullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;
    public float damage = 20f;
    public Color deflectColor = Color.yellow;
    
    [Header("Auto-Target Info")]
    public float searchRadius = 20f; // Bán kính tìm địch
    public LayerMask enemyLayer;     // Layer của kẻ địch (để biết mà tìm)

    [Range(-180, 180)]
    public float rotationOffset = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDeflected = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb.linearVelocity = transform.right * speed;
        rb.gravityScale = 0f;
        Destroy(gameObject, 5f); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDeflected)
        {
            Playermovement pm = other.GetComponent<Playermovement>();
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (pm != null && (pm.isRolling || pm.isCrouching)) return; 

            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
                Destroy(gameObject); 
            }
        }
        else if (other.CompareTag("Enemy") && isDeflected)
        {
            // --- ĐOẠN NÀY QUAN TRỌNG: GÂY SÁT THƯƠNG CHO ĐỊCH ---
            // Nếu ông có script EnemyHealth thì mở comment dòng dưới ra nhé
             EnemyHealth eHealth = other.GetComponent<EnemyHealth>();
             if(eHealth != null) eHealth.TakeDamage(damage * 5, transform.position); // Phản dame to gấp 5
            
            Debug.Log("Headshot kẻ địch!");
            Destroy(other.gameObject); // Tạm thời vẫn destroy
            Destroy(gameObject); 
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }

    // --- HÀM DEFLECT ĐÃ ĐƯỢC NÂNG CẤP AUTO-AIM ---
    public void Deflect()
    {
        if (isDeflected) return; 
        isDeflected = true;

        // 1. TÌM KẺ ĐỊCH GẦN NHẤT
        Transform target = FindNearestEnemy();
        Vector2 direction;

        if (target != null)
        {
            // Nếu tìm thấy địch -> Bay về hướng địch
            direction = (target.position - transform.position).normalized;
            Debug.Log("Đã tìm thấy mục tiêu: " + target.name);
        }
        else
        {
            // Nếu KHÔNG có địch nào gần đó -> Bay thẳng theo hướng viên đạn đang bay (hoặc bay về chuột làm dự phòng)
            // Ở đây tôi để bay thẳng tiếp cho ngầu
            direction = transform.right; 
            Debug.Log("Không thấy địch, bay thẳng!");
        }

        // 2. SETUP VẬT LÝ
        rb.gravityScale = 0f; 
        rb.linearVelocity = direction * (speed * 2f); // Tốc độ x2

        // 3. XOAY MŨI ĐẠN
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // 4. ĐỔI MÀU & LAYER
        if (sr != null) sr.color = deflectColor;
        gameObject.layer = LayerMask.NameToLayer("PlayerBullet");

        CancelInvoke();
        Destroy(gameObject, 3f); 
    }

    // --- HÀM PHỤ ĐỂ TÌM ĐỊCH GẦN NHẤT ---
    Transform FindNearestEnemy()
    {
        // Quét tất cả Collider trong bán kính searchRadius thuộc Layer Enemy
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
        
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            // Tính khoảng cách từ đạn tới từng thằng địch
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            
            // Tìm thằng gần nhất
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }
    
    // Vẽ vòng tròn tìm kiếm trong Editor để dễ chỉnh (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}