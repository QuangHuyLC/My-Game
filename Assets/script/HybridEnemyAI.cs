using UnityEngine;

public class HybridEnemyAI : MonoBehaviour
{
    [Header("--- THÔNG SỐ TẦM ĐÁNH ---")]
    public float meleeRange = 1.8f;   
    public float throwRange = 8f;     
    public float attackCooldown = 2f; 

    [Header("--- THÔNG SỐ CHIÊU GẦN (CHÉM) ---")]
    public Transform meleePoint;      
    public GameObject meleeSlashPrefab; 

    [Header("--- THÔNG SỐ CHIÊU XA (NÉM) ---")]
    public GameObject boomerangPrefab;
    public Transform firePoint;       

    private Transform player;
    private SpriteRenderer spriteRen;
    private Animator animator;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || !canAttack) return;

        // Ảnh gốc nhìn TRÁI. Nếu Player ở bên PHẢI (x lớn hơn) -> LẬT ẢNH (true)
        if (player.position.x > transform.position.x) spriteRen.flipX = true;
        else spriteRen.flipX = false;

        float distance = Vector2.Distance(transform.position, player.position);

        if (Time.time >= nextAttackTime)
        {
            if (distance <= meleeRange)
            {
                nextAttackTime = Time.time + attackCooldown;
                animator.SetTrigger("attack_melee"); // Gọi Anim Chém
            }
            else if (distance <= throwRange)
            {
                nextAttackTime = Time.time + attackCooldown;
                animator.SetTrigger("attack_range"); // Gọi Anim Ném
            }
        }
    }

    // ==========================================
    // 2 HÀM NÀY ĐỂ ANIMATION EVENT GỌI ĐÚNG NHỊP
    // ==========================================

    public void ThucHienGhiSatThuong() // Gắn vào frame vung móng vuốt
    {
        if (meleeSlashPrefab != null && meleePoint != null)
        {
            GameObject slash = Instantiate(meleeSlashPrefab, meleePoint.position, Quaternion.identity);
            
            SpriteRenderer slashSprite = slash.GetComponent<SpriteRenderer>();
            if (slashSprite != null)
            {
                slashSprite.flipX = spriteRen.flipX; 
            }
        }
    }

    public void ThucHienPhongBoomerang() // Gắn vào frame ném Boomerang
    {
        if (boomerangPrefab != null && firePoint != null)
        {
            GameObject boom = Instantiate(boomerangPrefab, firePoint.position, Quaternion.identity);
            
            // Nếu lật ảnh (đang nhìn PHẢI) -> ném sang PHẢI. Không lật (nhìn TRÁI) -> ném TRÁI
            Vector2 throwDir = spriteRen.flipX ? Vector2.right : Vector2.left;
            
            BoomerangFly boomScript = boom.GetComponent<BoomerangFly>();
            
            // ĐÃ THÊM ĐỂ QUAY VỀ: Truyền throwDir VÀ firePoint để Boomerang biết tay ai ném mà bay tới
            if (boomScript != null) boomScript.Setup(throwDir, firePoint);
        }
    }

    public void StopAttackImmediately()
    {
        canAttack = false;
        this.enabled = false; 
    }
}