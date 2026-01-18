using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Playermovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb; 
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public CapsuleCollider2D playerCollider;
    
    private Camera mainCam; 

    [Header("Stats")]
    public float Speed = 5f;
    public float jump = 12f;      
    public float rollSpeed = 12f;

    [Header("Ground Check & Stairs")]
    public Transform groundCheckPoint; 
    public float groundCheckRadius = 0.2f; 
    public LayerMask groundLayer;
    
    // Kéo thanh này dài hơn chiều cao 1 bậc thang
    [Range(0.1f, 2f)] public float stairCheckDistance = 1.0f;       
    
    [SerializeField] bool isGrounded;  

    [Header("Combat & Roll")]
    public Transform attackPoint;      
    public float attackCooldown = 0.5f;
    public float attackLockTime = 0.25f;
    public float rollDuration = 0.3f;
    public float rollCooldown = 0.8f;

    [Header("VFX Effects")]
    public GameObject slashEffectGO; 
    public float effectDuration = 0.1f; 

    // Biến trạng thái
    bool isAttacking = false;
    bool isRolling = false;
    bool isCrouching = false;
    float nextAttackTime = 0f;
    float nextRollTime = 0f;
    
    float lastOnGroundTime = 0f; 

    Vector2 standSize;
    Vector2 standOffset;

    void Start()
    {
        mainCam = Camera.main;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider2D>();

        if (playerCollider != null) {
            standSize = playerCollider.size;
            standOffset = playerCollider.offset;
        }
        
        if (rb != null) {
            rb.freezeRotation = true; 
            rb.gravityScale = 3f; 

            PhysicsMaterial2D noBounceMat = new PhysicsMaterial2D("NoBounce");
            noBounceMat.bounciness = 0f; 
            noBounceMat.friction = 0.4f; 
            rb.sharedMaterial = noBounceMat;
        }

        if (slashEffectGO != null) slashEffectGO.SetActive(false);
    }

    void Update()
    {
        bool currentCheck = false;
        if (groundCheckPoint != null)
            currentCheck = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        if (currentCheck)
        {
            isGrounded = true;
            lastOnGroundTime = Time.time;
        }
        else
        {
            isGrounded = false;
        }

        if (isRolling) return;

        Moving();
        Crouching(); 
        JumpingAndAnimation(); 
        Attacking();
        Rolling(); 
    }

    void Moving()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float horizontal = 0f;
        if (!isCrouching && !isAttacking)
        {
            if (kb.aKey.isPressed) horizontal = -1f;
            if (kb.dKey.isPressed) horizontal = 1f;
        }

        rb.linearVelocity = new Vector2(horizontal * Speed, rb.linearVelocity.y);
        if(animator) animator.SetFloat("speed", Mathf.Abs(horizontal));

        if (horizontal > 0) 
        {
            spriteRenderer.flipX = false;
            if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (horizontal < 0) 
        {
            spriteRenderer.flipX = true;
            if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void Crouching()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool isDownPressed = kb.sKey.isPressed || kb.downArrowKey.isPressed;

        if (isDownPressed && !isCrouching)
        {
            isCrouching = true;
            if(animator) animator.SetBool("isCrouching", true);
            ResizeCollider(true);
        }
        else if (!isDownPressed && isCrouching)
        {
            isCrouching = false;
            if(animator) animator.SetBool("isCrouching", false);
            ResizeCollider(false);
        }
    }

    void ResizeCollider(bool crouching)
    {
        if (playerCollider == null) return;
        if (crouching)
        {
            playerCollider.size = new Vector2(standSize.x, standSize.y * 0.5f);
            playerCollider.offset = new Vector2(standOffset.x, standOffset.y - (standSize.y * 0.25f));
        }
        else
        {
            playerCollider.size = standSize;
            playerCollider.offset = standOffset;
        }
    }

    // --- HÀM ĐÃ SỬA LỖI ---
    void JumpingAndAnimation()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool canJump = (Time.time - lastOnGroundTime <= 0.15f);

        if (kb.spaceKey.wasPressedThisFrame && canJump && !isAttacking && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            lastOnGroundTime = -10f; 
            isGrounded = false;
        }

        if(animator) 
        {
            if (isCrouching) 
            {
                animator.SetBool("jump", false);
            }
            else 
            {
                bool isJumpingUp = rb.linearVelocity.y > 0.1f;

                // Kiểm tra xem đất có ở gần không (Bắn tia từ chân xuống)
                bool isGroundBelow = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, stairCheckDistance, groundLayer);

                // SỬA LỖI Ở DÒNG NÀY: Dùng đúng tên biến isGroundBelow
                bool isFallingForReal = (rb.linearVelocity.y < 0) && !isGroundBelow;

                animator.SetBool("jump", isJumpingUp || isFallingForReal);
            }
        }
    }

    void Attacking()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        bool attackInput = kb.jKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (attackInput && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            
            // --- LOGIC MỚI: QUAY MẶT THEO CHUỘT KHI CHÉM ---
            if (mouse != null)
            {
                // Lấy vị trí chuột trong thế giới game
                Vector2 mousePos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
                
                // So sánh toạ độ X của chuột và người
                // Nếu chuột nằm bên phải (> 0) thì quay phải, ngược lại quay trái
                float directionToMouse = mousePos.x - transform.position.x;
                
                if (directionToMouse > 0)
                {
                    spriteRenderer.flipX = false;
                    if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    spriteRenderer.flipX = true;
                    if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 180, 0);
                }
            }
            // ------------------------------------------------

            if(animator) animator.SetTrigger("attack");
            StartCoroutine(ShowSlashEffect());
            StartCoroutine(AttackLock());
        }
    }
    IEnumerator ShowSlashEffect()
    {
        if (slashEffectGO == null) yield break; 
        slashEffectGO.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        slashEffectGO.SetActive(false);
    }

    System.Collections.IEnumerator AttackLock()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
        yield return new WaitForSeconds(attackLockTime);
        isAttacking = false;
    }

    void Rolling()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;
        
        if (kb.leftShiftKey.wasPressedThisFrame && Time.time >= nextRollTime)
        {
            float direction = 0f;
            if (kb.aKey.isPressed) direction = -1f;
            else if (kb.dKey.isPressed) direction = 1f;

            if (direction == 0f && mouse != null)
            {
                Vector2 mousePos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
                direction = mousePos.x > transform.position.x ? 1f : -1f;
            }

            if (direction == 0f) direction = spriteRenderer.flipX ? -1f : 1f;

            StartCoroutine(RollRoutine(direction));
        }
    }

    IEnumerator RollRoutine(float dir)
    {
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;

        if(animator) 
        {
            animator.SetBool("isCrouching", false);
            animator.SetTrigger("roll");
        }

        if (dir > 0) spriteRenderer.flipX = false;
        else spriteRenderer.flipX = true;

        rb.linearVelocity = new Vector2(dir * rollSpeed, 0f); 
        
        yield return new WaitForSeconds(rollDuration);
        
        isRolling = false;
        rb.linearVelocity = Vector2.zero; 

        var kb = Keyboard.current;
        bool isHoldingDown = kb.sKey.isPressed || kb.downArrowKey.isPressed;

        if (isHoldingDown)
        {
            isCrouching = true;
            if(animator) animator.SetBool("isCrouching", true);
            ResizeCollider(true); 
        }
        else 
        {
            isCrouching = false; 
            ResizeCollider(false); 
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * stairCheckDistance); 
    }
}