using UnityEngine;
using System.Collections;

public class MeleeEnemyHealth : MonoBehaviour
{
    [Header("--- Chỉ số ---")]
    public int mauToiDa = 10; // Máu ít thôi để 1 hit là chết
    private int mauHienTai;

    [Header("--- Cấu hình ---")]
    // Chỉnh số này bằng đúng thời gian Animation Hurt (ví dụ 0.4 hoặc 0.5)
    public float thoiGianHurt = 0.5f; 

    [Header("--- KÉO SCRIPT DI CHUYỂN VÀO ĐÂY ---")]
    public MeleeEnemy scriptDiChuyen; // Kéo file MeleeEnemy vào đây

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool daChet = false;

    void Start()
    {
        mauHienTai = mauToiDa;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (scriptDiChuyen == null) scriptDiChuyen = GetComponent<MeleeEnemy>();
    }

    public void TakeDamage(float satThuong, Vector2 viTriNguoiDanh)
    {
        if (daChet) return; // Chết rồi thì thôi, không nhận damage nữa

        mauHienTai -= (int)satThuong;

        // 1. Luôn ưu tiên chạy Animation Hurt trước
        if (anim != null) 
        {
            anim.ResetTrigger("Attack"); // Hủy lệnh đánh (nếu có)
            anim.Play("Hurt"); // Lệnh Play mạnh hơn Trigger, ép chạy Hurt ngay lập tức
        }

        // 2. Kiểm tra sống chết
        if (mauHienTai <= 0)
        {
            XuLyChet1Hit(); // Gọi hàm xử lý chết ngay
        }
        else
        {
            // Nếu chưa chết thì mới tính chuyện bị đẩy lùi
            if (rb != null)
            {
                Vector2 huongDay = (transform.position - (Vector3)viTriNguoiDanh).normalized;
                StartCoroutine(BiDayLui(huongDay));
            }
        }
    }

    // Hàm xử lý riêng cho vụ "1 hit chết luôn"
    void XuLyChet1Hit()
    {
        daChet = true;
        Debug.Log("1 Hit! Chết luôn -> Đứng im -> Hurt -> Biến mất");

        // BƯỚC 1: TẮT NÃO (Ngừng đuổi theo ngay lập tức)
        if (scriptDiChuyen != null) scriptDiChuyen.enabled = false;

        // BƯỚC 2: KHÓA CỨNG VẬT LÝ (Chữa bệnh quay vòng vòng)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Dừng trượt
            rb.angularVelocity = 0f;          // Dừng quay
            rb.bodyType = RigidbodyType2D.Kinematic; // Đóng băng tại chỗ
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // Khóa toàn bộ
        }

        // BƯỚC 3: TẮT VA CHẠM (Để Player không vướng chân)
        if (col != null) col.enabled = false;

        // BƯỚC 4: HỦY DIỆT (Sau khi diễn xong Hurt)
        Destroy(gameObject, thoiGianHurt);
    }

    IEnumerator BiDayLui(Vector2 huong)
    {
        if (daChet) yield break;

        if (scriptDiChuyen != null) scriptDiChuyen.enabled = false;
        
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(huong * 5f, ForceMode2D.Impulse); 

        yield return new WaitForSeconds(0.2f); 

        if (!daChet && scriptDiChuyen != null)
        {
            scriptDiChuyen.enabled = true;
            rb.linearVelocity = Vector2.zero; 
        }
    }
}