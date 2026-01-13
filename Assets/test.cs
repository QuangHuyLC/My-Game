using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Playermovement : MonoBehaviour
{
    [Header("Stats")]
    public float Speed = 5f;
    public float jump = 8f;
    public float rollSpeed = 12f;

    [Header("Components")]
    public Rigidbody2D rigidbody2D;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Combat & Roll")]
    // QUAN TRỌNG: Kéo object AttackPoint vào ô này trong Inspector
    public Transform attackPoint; 
    public float attackCooldown = 0.5f;
    public float attackLockTime = 0.25f;
    public float rollDuration = 0.3f;
    public float rollCooldown = 0.8f;

    [Header("VFX Effects")]
    public GameObject slashEffectGO; 
    public float effectDuration = 0.1f; 

    // State variables
    bool isAttacking = false;
    bool isRolling = false;
    float nextAttackTime = 0f;
    float nextRollTime = 0f;

    void Start()
    {
        if (rigidbody2D == null) rigidbody2D = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        if (slashEffectGO != null) slashEffectGO.SetActive(false);
    }

    void Update()
    {
        if (isRolling) return;

        Moving();
        Jumping();
        Attacking();
        Rolling();
    }

    void Moving()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float horizontal = 0f;
        if (kb.aKey.isPressed) horizontal = -1f;
        if (kb.dKey.isPressed) horizontal = 1f;

        if (isAttacking) horizontal = 0f;

        rigidbody2D.linearVelocity = new Vector2(horizontal * Speed, rigidbody2D.linearVelocity.y);

        // Chỉnh lại animation speed, bỏ thời gian delay để nhân vật dừng dứt khoát
        animator.SetFloat("speed", Mathf.Abs(horizontal));

        // --- XỬ LÝ XOAY HƯỚNG (SỬA LỖI VỆT CHÉM) ---
        if (horizontal > 0) // Đi phải
        {
            spriteRenderer.flipX = false;
            // Xoay AttackPoint về hướng 0 độ (phải)
            if (attackPoint != null) 
                attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (horizontal < 0) // Đi trái
        {
            spriteRenderer.flipX = true;
            // Xoay AttackPoint về hướng 180 độ (trái)
            if (attackPoint != null) 
                attackPoint.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void Jumping()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (isAttacking) return;

        if (kb.spaceKey.wasPressedThisFrame && Mathf.Abs(rigidbody2D.linearVelocity.y) < 0.1f)
        {
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jump);
            animator.SetBool("jump", true);
        }

        if (Mathf.Abs(rigidbody2D.linearVelocity.y) < 0.1f)
        {
            animator.SetBool("jump", false);
        }
    }

    void Rolling()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftShiftKey.wasPressedThisFrame && Time.time >= nextRollTime)
        {
            StartCoroutine(RollRoutine());
        }
    }

    IEnumerator RollRoutine()
    {
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;
        animator.SetTrigger("roll"); 
        
        // Xác định hướng lộn dựa trên hướng sprite đang quay
        float rollDirection = spriteRenderer.flipX ? -1f : 1f;
        
        rigidbody2D.linearVelocity = new Vector2(rollDirection * rollSpeed, rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(rollDuration);
        
        isRolling = false;
        rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
    }

    void Attacking()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.jKey.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            animator.SetTrigger("attack");
            StartCoroutine(ShowSlashEffect());
            StartCoroutine(AttackLock());
        }
    }

    IEnumerator ShowSlashEffect()
    {
        if (slashEffectGO == null) yield break; 

        // Chỉ cần bật lên, hướng đã được hàm Moving xử lý xoay AttackPoint rồi
        slashEffectGO.SetActive(true);

        yield return new WaitForSeconds(effectDuration);

        slashEffectGO.SetActive(false);
    }

    System.Collections.IEnumerator AttackLock()
    {
        isAttacking = true;
        rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(attackLockTime);
        isAttacking = false;
    }
}