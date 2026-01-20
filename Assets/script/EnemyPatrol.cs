using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float moveSpeed = 2f;         // Tốc độ đi bộ
    public Transform leftPoint;         // Điểm giới hạn trái
    public Transform rightPoint;        // Điểm giới hạn phải
    
    private bool movingRight = true;    // Đang đi sang phải hay trái
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Tách 2 cái điểm giới hạn ra khỏi con Enemy để nó không bị trôi theo Enemy
        if (leftPoint != null) leftPoint.parent = null;
        if (rightPoint != null) rightPoint.parent = null;
    }

    void Update()
    {
        // 1. Tính toán hướng di chuyển
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            // Nếu đi quá điểm bên phải thì quay đầu
            if (transform.position.x >= rightPoint.position.x)
            {
                Flip();
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            // Nếu đi quá điểm bên trái thì quay đầu
            if (transform.position.x <= leftPoint.position.x)
            {
                Flip();
            }
        }

        // 2. Cập nhật Animator (Biến 'speed' mà ông vừa tạo lúc nãy)
        // Dùng Mathf.Abs để lấy giá trị dương, giúp Animator hiểu là đang chạy
        if (anim != null)
        {
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        // Xoay người lính lại 180 độ
        transform.Rotate(0, 180, 0);
    }
}