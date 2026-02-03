using UnityEngine;
using System.Collections;

public class BossBehave : MonoBehaviour
{
    [Header("--- CẤU HÌNH BOSS ---")]
    public float maxHealth = 500f;
    public float currentHealth;
    public float tocDoDi = 2f;
    public float tamPhatHien = 10f; 
    public float tamDanh = 2.5f;   

    [Header("--- PHASE 2: HÓA ĐIÊN ---")]
    public float tocDoDien = 5f;
    public Color mauHoaDien = Color.red;
    private bool isEnraged = false;

    [Header("--- TẤN CÔNG ---")]
    public float satThuong = 20f;
    public float tocDoDanh = 1.5f;  
    public Transform diemChem;      // Kéo cái AttackPoint vào đây
    public float banKinhChem = 1f;
    public LayerMask layerPlayer;   

    [Header("--- HIỆU ỨNG ---")]
    public GameObject hieuUngNo;    

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRen;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRen = GetComponent<SpriteRenderer>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Logic AI
        if (dist <= tamDanh)
        {
            TanCong();
        }
        else if (dist < tamPhatHien)
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
        if (Time.time < nextAttackTime) return; // Đang hồi chiêu thì không đi

        // Quay mặt theo Player
        if (player.position.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(7,7,7);

        float speed = isEnraged ? tocDoDien : tocDoDi;

        // Nếu ông có Animation chạy thì set Run = true, không thì nó lướt bằng Idle
        if (anim) anim.SetBool("Run", true); 

        // Di chuyển tịnh tiến
        Vector2 target = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void DungIm()
    {
        if (anim) anim.SetBool("Run", false);
    }

    void TanCong()
    {
        DungIm(); 

        if (Time.time >= nextAttackTime)
        {
            float cooldown = isEnraged ? tocDoDanh / 1.5f : tocDoDanh;
            nextAttackTime = Time.time + cooldown;

            // Gọi đúng tên Trigger "Attack" trong Animator của ông
            if (anim) anim.SetTrigger("Attack");

            // Gây dame sau 0.2s (để khớp với lúc vung kiếm)
            StartCoroutine(GaySatThuongTre(0.2f)); 
        }
    }

    IEnumerator GaySatThuongTre(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (diemChem != null)
        {
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(diemChem.position, banKinhChem, layerPlayer);
            foreach (Collider2D p in hitPlayers)
            {
                // Gọi hàm trừ máu bên Player (nếu có)
                // p.GetComponent<PlayerHealth>().TakeDamage(satThuong);
                Debug.Log("Chém trúng Player!");
            }
        }
    }

    // --- HÀM BỊ ĐÁNH (Gắn vào vũ khí Player gọi sang) ---
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        // Gọi Trigger "Hurt"
        if (anim) anim.SetTrigger("Hurt");

        // Hóa điên nếu máu < 50%
        if (!isEnraged && currentHealth <= maxHealth * 0.5f)
        {
            isEnraged = true;
            if (spriteRen) spriteRen.color = mauHoaDien;
            if (hieuUngNo) Instantiate(hieuUngNo, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Chet();
        }
    }

    void Chet()
    {
        isDead = true;
        // Gọi Bool "isDead"
        if (anim) anim.SetBool("isDead", true);
        
        GetComponent<Collider2D>().enabled = false; 
        this.enabled = false; 
        rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (diemChem != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(diemChem.position, banKinhChem);
        }
    }
}