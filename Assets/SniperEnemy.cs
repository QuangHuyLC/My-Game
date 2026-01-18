using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    [Header("Tuần tra")]
    public float moveSpeed = 2f;
    public Transform leftPoint;
    public Transform rightPoint;
    private bool movingRight = true;

    [Header("Phát hiện (Vùng nhìn)")]
    public float detectionRange = 8f;
    [Range(0, 180)] public float viewAngle = 90f;
    public LayerMask playerLayer;

    [Header("Bắn súng")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject muzzleFlash; // Kéo cái hiệu ứng vòng tròn xanh vào đây
    public float fireRate = 1.5f;
    private float nextFireTime;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private bool isPlayerInView = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (leftPoint != null) leftPoint.parent = null;
        if (rightPoint != null) rightPoint.parent = null;
        
        if (muzzleFlash != null) muzzleFlash.SetActive(false); // Mặc định tắt hiệu ứng
    }

    void Update()
    {
        CheckView();

        if (isPlayerInView)
        {
            AttackLogic();
        }
        else
        {
            PatrolLogic();
        }
    }

    void CheckView()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            Vector2 forwardDir = movingRight ? Vector2.right : Vector2.left;
            float angleBetween = Vector2.Angle(forwardDir, dirToPlayer);

            if (angleBetween <= viewAngle / 2f)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, LayerMask.GetMask("Ground"));
                if (hit.collider == null)
                {
                    isPlayerInView = true;
                    return;
                }
            }
        }
        isPlayerInView = false;
    }

    void PatrolLogic()
    {
        float currentSpeed = movingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (movingRight && transform.position.x >= rightPoint.position.x) Flip();
        else if (!movingRight && transform.position.x <= leftPoint.position.x) Flip();

        if (anim) anim.SetFloat("speed", 1f);
    }

    void AttackLogic()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim) anim.SetFloat("speed", 0f);

        if (player.position.x > transform.position.x && !movingRight) Flip();
        else if (player.position.x < transform.position.x && movingRight) Flip();

        // CHỈ KÍCH HOẠT ANIMATION, KHÔNG SINH ĐẠN Ở ĐÂY
        if (Time.time >= nextFireTime)
        {
            if (anim) anim.SetTrigger("attack"); 
            nextFireTime = Time.time + fireRate;
        }
    }

    // --- CÁC HÀM GẮN VÀO ANIMATION EVENT ---

    // Gắn vào mốc 1: Lúc bắt đầu giơ súng
    public void ShowMuzzleFlash()
    {
        if (muzzleFlash != null) 
        {
            muzzleFlash.SetActive(true);
            Invoke("HideMuzzleFlash", 0.15f); // Tự tắt sau một nháy
        }
    }

    void HideMuzzleFlash()
    {
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    // Gắn vào mốc 2: Lúc bóp cò (CreateBullet)
    public void CreateBullet() 
    {
        if (bulletPrefab && firePoint && player != null)
        {
            Vector2 shootDir = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if(bulletRb != null)
            {
                float bulletSpeed = 18f; // Đạn sniper cho bay nhanh tí
                bulletRb.linearVelocity = shootDir * bulletSpeed;
            }
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.Rotate(0, 180, 0);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 forwardDir = movingRight ? transform.right : -transform.right;
        Vector3 leftBoundary = Quaternion.AngleAxis(-viewAngle / 2f, Vector3.forward) * forwardDir;
        Vector3 rightBoundary = Quaternion.AngleAxis(viewAngle / 2f, Vector3.forward) * forwardDir;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);
    }
}