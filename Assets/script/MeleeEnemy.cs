using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("--- Cấu hình Di Chuyển ---")]
    public float tocDoChay = 3f;
    public float tamPhatHien = 10f; 

    [Header("--- Cấu hình Tấn Công ---")]
    // LỜI KHUYÊN: Nên tăng TamDanh lên một chút (ví dụ 2f) để dễ đánh trúng hơn
    public float tamDanh = 2f; 
    public float tocDoDanh = 1f; 
    public int satThuong = 10; 
    
    [Header("--- Component ---")]
    public Animator anim; 
    
    private Transform player;
    private float thoiGianHoiChieu = 0f;
    private bool dangDanh = false;
    private Vector3 scaleGoc; 

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        scaleGoc = transform.localScale; 
    }

    void Update()
    {
        if (player == null) return;

        // Giảm hồi chiêu liên tục
        if (thoiGianHoiChieu > 0) thoiGianHoiChieu -= Time.deltaTime;

        // --- FIX QUAN TRỌNG: NẾU ĐANG ĐÁNH THÌ ĐỨNG YÊN HOÀN TOÀN ---
        // (Không tính toán khoảng cách, không quay mặt, không di chuyển)
        if (dangDanh) return; 

        float khoangCach = Vector2.Distance(transform.position, player.position);

        // 1. XỬ LÝ QUAY MẶT (Flip) - Chỉ chạy khi KHÔNG đánh
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(scaleGoc.x), scaleGoc.y, scaleGoc.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(scaleGoc.x), scaleGoc.y, scaleGoc.z);
        }

        // 2. LOGIC DI CHUYỂN & TẤN CÔNG
        if (khoangCach <= tamDanh)
        {
            TanCong();
        }
        else if (khoangCach < tamPhatHien)
        {
            DiChuyen();
        }
        else
        {
            DungIm();
        }
    }

    void DiChuyen()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, tocDoChay * Time.deltaTime);
        if (anim) anim.SetBool("isRunning", true);
    }

    void DungIm()
    {
        if (anim) anim.SetBool("isRunning", false);
    }

    void TanCong()
    {
        if (anim) anim.SetBool("isRunning", false);

        if (thoiGianHoiChieu <= 0)
        {
            dangDanh = true; // Khóa mọi hành động khác
            
            if (anim) anim.SetTrigger("Attack");

            thoiGianHoiChieu = tocDoDanh;
            
            // FIX: Tăng thời gian chờ lên 1.0f (hoặc cao hơn nếu Animation của ông dài)
            // Để đảm bảo Animation Event kịp chạy trước khi bị ngắt
            Invoke("KetThucDanh", 1.0f); 
        }
    }

    // --- HÀM NÀY ĐỂ ANIMATION EVENT GỌI ---
    public void SuKienDanhTrung()
    {
        if (player == null) return;

        // Tăng tầm kiểm tra lên chút (tamDanh + 1.5f) để bù trừ lag hoặc animation
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > tamDanh + 1.5f) return;

        PlayerHealth mauNguoiChoi = player.GetComponent<PlayerHealth>();
        
        if (mauNguoiChoi != null)
        {
            // Gọi hàm trừ máu
            mauNguoiChoi.TakeDamage(satThuong, transform.position);
        }
    }

    void KetThucDanh()
    {
        dangDanh = false; // Mở khóa cho phép di chuyển lại
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tamDanh);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tamPhatHien);
    }
    // Dán cái này vào cuối script MeleeEnemy.cs
    public void StopAttackImmediately()
    {
        // 1. Tự tắt mình đi
        this.enabled = false; 
        
        // 2. Hủy hết các lệnh đang chờ (như lệnh tấn công)
        StopAllCoroutines(); 
        CancelInvoke();      
    }
}