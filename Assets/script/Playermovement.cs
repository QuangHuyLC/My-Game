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

    [Header("AUDIO SYSTEM")]
    public AudioSource audioSource;
    public AudioClip slashSound;       
    [Range(0f, 1f)] public float slashVolume = 0.5f; 
    public AudioClip jumpSound;        
    [Range(0f, 1f)] public float jumpVolume = 0.5f;  
    public AudioClip[] footstepSounds; 
    [Range(0f, 1f)] public float footstepVolume = 0.4f; 
    public AudioClip teleportSound; 
    [Range(0f, 1f)] public float teleportVolume = 0.7f; 
    public AudioClip landSound;        
    [Range(0f, 1f)] public float landVolume = 0.5f;

    [Header("Reflex Mode")]
    public float slowMoFactor = 0.5f;   
    public float maxReflexTime = 5f;    
    private float currentReflexTime;
    private bool isSlowMo = false;
    private bool isHitStopping = false; 

    [Header("TELEPORT SKILL")]
    public float teleportDistance = 5f;    
    public float teleportCooldown = 1.0f;  
    public LayerMask wallLayer;            
    private float nextTeleportTime;

    [Header("GHOST EFFECT")]
    public GameObject ghostPrefab;     
    public float ghostDelay = 0.05f;   
    public float ghostLifetime = 0.5f; 
    public Color ghostColor = new Color(0f, 1f, 1f, 0.8f); 
    private float ghostTimer;

    [Header("Stats")]
    public float Speed = 5f;
    public float jump = 12f;      
    public float rollSpeed = 12f;
    public float damage = 20f;

    [Header("--- MOVEMENT FEEL (CỰC QUAN TRỌNG) ---")]
    public float coyoteTime = 0.2f; 
    public float jumpBufferTime = 0.2f;
    [Tooltip("Hệ số trọng lực khi rơi (Rơi nhanh hơn nhảy lên)")]
    public float fallGravityMult = 5f; 
    [Tooltip("Hệ số cắt nhảy (Thả nút sớm thì nhảy thấp)")]
    public float jumpCutMult = 0.5f;
    private float defaultGravityScale;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("--- WALL MECHANICS ---")]
    public Transform wallCheckPoint; 
    public float wallCheckRadius = 0.2f;
    public LayerMask wallClimbLayer; 
    public float wallSlideSpeed = 2f; 
    public float wallJumpForce = 10f; 
    public Vector2 wallJumpDirection = new Vector2(1f, 1f); 
    
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallGrabbing; 
    private bool canMove = true; 

    [Header("Ground Check")]
    public Transform groundCheckPoint; 
    public float groundCheckRadius = 0.2f; 
    public LayerMask groundLayer;
    [Range(0.1f, 2f)] public float stairCheckDistance = 1.0f;       
    [SerializeField] bool isGrounded;  

    [Header("Combat & Roll")]
    public Transform attackPoint;      
    public float attackRange = 1.0f; 
    public float attackCooldown = 0.2f; 
    public float attackLockTime = 0.2f; 
    public float attackLungeForce = 15f; 
    public float rollDuration = 0.3f;
    public float rollCooldown = 0.8f;
    
    public LayerMask bulletLayer;
    public LayerMask enemyLayer; 

    [Header("VFX")]
    public GameObject slashEffectGO; 
    public float effectDuration = 0.1f; 
    public GameObject runDustPrefab;   
    public GameObject jumpDustPrefab;  
    public GameObject landDustPrefab; 

    public bool isAttacking = false;
    public bool isRolling = false;    
    public bool isCrouching = false;  
    public bool isHurting = false; 
    
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
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (playerCollider != null) {
            standSize = playerCollider.size;
            standOffset = playerCollider.offset;
        }
        
        if (rb != null) {
            rb.freezeRotation = true; 
            defaultGravityScale = 3f; // Lưu trọng lực mặc định
            rb.gravityScale = defaultGravityScale; 
            PhysicsMaterial2D noBounceMat = new PhysicsMaterial2D("NoBounce");
            noBounceMat.bounciness = 0f; 
            noBounceMat.friction = 0.4f; 
            rb.sharedMaterial = noBounceMat;
        }

        if (slashEffectGO != null) slashEffectGO.SetActive(false);
        currentReflexTime = maxReflexTime;
    }

    void Update()
    {
        if (Time.timeScale == 0) return; 

        // 1. CHECK GROUND
        bool currentCheck = false;
        if (groundCheckPoint != null)
            currentCheck = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        if (currentCheck && !isGrounded && rb.linearVelocity.y < 0.1f)
        {
            if (audioSource != null && landSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(landSound, landVolume);
            }
            CreateDust(landDustPrefab != null ? landDustPrefab : jumpDustPrefab, 0); 
        }

        if (currentCheck) { isGrounded = true; lastOnGroundTime = Time.time; }
        else { isGrounded = false; }

        // 2. TIMERS & INPUT
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        var kb = Keyboard.current;
        if (kb != null) {
            if (kb.spaceKey.wasPressedThisFrame) jumpBufferCounter = jumpBufferTime;
            else jumpBufferCounter -= Time.deltaTime;
        }

        if (isHurting) return; 
        if (isRolling) return;

        CheckWallLogic();
        UpdateAnimator(); 
        
        // --- GỌI HÀM XỬ LÝ TRỌNG LỰC MỚI ---
        ApplyGravityLogic();
        // ------------------------------------

        if (canMove) Moving();
        
        Crouching(); 
        BetterJumping();
        Attacking();
        Rolling(); 
        TeleportSkill(); 
        HandleSlowMotion();
    }

    // --- LOGIC TRỌNG LỰC & NHẢY MỚI (TẠO CẢM GIÁC BAY NHẢY) ---
    void ApplyGravityLogic()
    {
        // Nếu đang bám tường hoặc lướt -> Không can thiệp trọng lực (để logic tường tự lo)
        if (isWallGrabbing || isWallSliding || isRolling) return;

        // 1. Rơi nhanh (Fast Fall): Khi vận tốc Y < 0, tăng trọng lực lên
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravityScale * fallGravityMult;
        }
        // 2. Nhảy thấp (Jump Cut): Nếu đang bay lên (Y > 0) mà thả nút Space -> Cắt bớt lực nhảy
        else if (rb.linearVelocity.y > 0 && Keyboard.current != null && !Keyboard.current.spaceKey.isPressed)
        {
            rb.gravityScale = defaultGravityScale * (1f / jumpCutMult); // Tăng trọng lực đột ngột để hãm lại
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }
    // -----------------------------------------------------------

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("isWallSliding", isWallSliding);
        animator.SetBool("isWallGrabbing", isWallGrabbing);
        animator.SetBool("isGrounded", isGrounded);

        if (!isWallSliding && !isWallGrabbing && !isCrouching)
        {
            bool isJumpingUp = rb.linearVelocity.y > 0.1f;
            bool isFallingForReal = (rb.linearVelocity.y < 0) && !isGrounded;
            animator.SetBool("jump", isJumpingUp || isFallingForReal);
        }
        else
        {
            animator.SetBool("jump", false);
        }
    }

    void CheckWallLogic()
    {
        if (wallCheckPoint == null) return;
        isTouchingWall = Physics2D.OverlapCircle(wallCheckPoint.position, wallCheckRadius, wallClimbLayer);
        isWallSliding = false;
        isWallGrabbing = false;

        if (isTouchingWall && !isGrounded)
        {
            var kb = Keyboard.current;
            float horizontalInput = 0f;
            if (kb != null) {
                if (kb.aKey.isPressed) horizontalInput = -1f;
                if (kb.dKey.isPressed) horizontalInput = 1f;
            }

            float facingDirection = spriteRenderer.flipX ? -1f : 1f;
            bool isPushingAgainstWall = (horizontalInput != 0 && horizontalInput == facingDirection);

            if (isPushingAgainstWall) 
            {
                isWallGrabbing = true;
                rb.linearVelocity = Vector2.zero; 
                rb.gravityScale = 0; 
            }
            else 
            {
                isWallSliding = true;
                rb.gravityScale = defaultGravityScale; // Reset về mặc định khi trượt
                if (rb.linearVelocity.y < -wallSlideSpeed)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else
        {
            // Trọng lực sẽ được hàm ApplyGravityLogic xử lý ở Update
        }
    }

    void BetterJumping()
    {
        if (jumpBufferCounter > 0 && (isWallSliding || isWallGrabbing))
        {
            WallJump();
            return; 
        }
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !isAttacking && !isCrouching)
        {
            PerformJump(jump);
        }
    }

    void PerformJump(float force)
    {
        if (audioSource != null && jumpSound != null) {
            audioSource.pitch = 1f; 
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
        float facingDir = spriteRenderer.flipX ? -1f : 1f;
        CreateDust(jumpDustPrefab, facingDir); 
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpBufferCounter = 0; 
        coyoteTimeCounter = 0; 
        isGrounded = false;
    }

    void WallJump()
    {
        rb.gravityScale = defaultGravityScale; 
        float direction = spriteRenderer.flipX ? 1f : -1f; 
        Vector2 force = new Vector2(wallJumpDirection.x * direction * wallJumpForce, wallJumpDirection.y * wallJumpForce);
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(force, ForceMode2D.Impulse);
        CreateDust(jumpDustPrefab, direction);
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound, jumpVolume);
        jumpBufferCounter = 0;
        
        spriteRenderer.flipX = (direction < 0);
        if (direction > 0) wallCheckPoint.localPosition = new Vector3(Mathf.Abs(wallCheckPoint.localPosition.x), wallCheckPoint.localPosition.y, 0);
        else wallCheckPoint.localPosition = new Vector3(-Mathf.Abs(wallCheckPoint.localPosition.x), wallCheckPoint.localPosition.y, 0);

        StartCoroutine(DisableMove(0.2f));
    }

    IEnumerator DisableMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void Moving() 
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (isAttacking) return; 

        float horizontal = 0f;
        if (!isCrouching) {
            if (kb.aKey.isPressed) horizontal = -1f;
            if (kb.dKey.isPressed) horizontal = 1f;
        }

        rb.linearVelocity = new Vector2(horizontal * Speed, rb.linearVelocity.y);

        if(animator) {
            if (isWallGrabbing) animator.SetFloat("speed", 0f); 
            else animator.SetFloat("speed", Mathf.Abs(horizontal));
        }

        if (horizontal > 0) {
            if (spriteRenderer.flipX == true) CreateDust(runDustPrefab, 1f); 
            spriteRenderer.flipX = false;
            if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
            if (wallCheckPoint != null) wallCheckPoint.localPosition = new Vector3(Mathf.Abs(wallCheckPoint.localPosition.x), wallCheckPoint.localPosition.y, 0);
        }
        else if (horizontal < 0) {
            if (spriteRenderer.flipX == false) CreateDust(runDustPrefab, -1f);
            spriteRenderer.flipX = true;
            if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 180, 0);
            if (wallCheckPoint != null) wallCheckPoint.localPosition = new Vector3(-Mathf.Abs(wallCheckPoint.localPosition.x), wallCheckPoint.localPosition.y, 0);
        }
    }

    // --- GIỮ NGUYÊN CÁC HÀM CÒN LẠI (Teleport, VFX, Attack...) ---
    void TeleportSkill() {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.rightButton.wasPressedThisFrame && Time.time >= nextTeleportTime) {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
            Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
            float dist = teleportDistance;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dir, teleportDistance, wallLayer);
            if (wallHit.collider != null) dist = wallHit.distance - 0.5f; 
            Vector2 targetPos = (Vector2)transform.position + (dir * dist);
            RaycastHit2D[] enemiesHit = Physics2D.RaycastAll(transform.position, dir, dist, enemyLayer);
            foreach (RaycastHit2D hit in enemiesHit) {
                EnemyHealth eHealth = hit.collider.GetComponent<EnemyHealth>();
                if (eHealth != null) {
                    eHealth.TakeDamage(damage * 2, transform.position); 
                    TriggerHitStop(0.1f); 
                }
            }
            StartCoroutine(SpawnGhostsAlongPath(transform.position, targetPos));
            transform.position = targetPos;
            rb.linearVelocity = Vector2.zero; 
            if (audioSource != null && teleportSound != null) audioSource.PlayOneShot(teleportSound, teleportVolume);
            nextTeleportTime = Time.time + teleportCooldown;
            if (dir.x > 0) spriteRenderer.flipX = false;
            else if (dir.x < 0) spriteRenderer.flipX = true;
        }
    }
    IEnumerator SpawnGhostsAlongPath(Vector2 start, Vector2 end) {
        float distance = Vector2.Distance(start, end);
        int ghostCount = 5; 
        for (int i = 0; i < ghostCount; i++) {
            Vector2 pos = Vector2.Lerp(start, end, (float)i / ghostCount);
            if (ghostPrefab != null) {
                GameObject ghost = Instantiate(ghostPrefab, pos, transform.rotation);
                GhostSprite script = ghost.GetComponent<GhostSprite>();
                if (script != null) script.Setup(spriteRenderer, new Color(1f, 0f, 0f, 0.5f), 0.3f); 
            }
        }
        yield return null;
    }
    public void PlayFootstep() {
        if (isGrounded && footstepSounds.Length > 0 && audioSource != null) {
            int randIndex = Random.Range(0, footstepSounds.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(footstepSounds[randIndex], footstepVolume);
        }
    }
    void MakeGhost() {
        if (ghostPrefab == null) return;
        if (ghostTimer > 0) ghostTimer -= Time.deltaTime; 
        else {
            GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
            GhostSprite script = ghost.GetComponent<GhostSprite>();
            if (script != null) script.Setup(spriteRenderer, ghostColor, ghostLifetime);
            ghostTimer = ghostDelay;
        }
    }
    void HandleSlowMotion() {
        if (isHitStopping) return; 
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.eKey.isPressed && currentReflexTime > 0) {
            isSlowMo = true;
            Time.timeScale = slowMoFactor;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; 
            currentReflexTime -= Time.unscaledDeltaTime;
            if(spriteRenderer) spriteRenderer.color = new Color(0.7f, 1f, 1f); 
            MakeGhost(); 
        } else {
            isSlowMo = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            if (currentReflexTime < maxReflexTime) currentReflexTime += Time.unscaledDeltaTime * 0.5f;
            if(spriteRenderer) spriteRenderer.color = Color.white;
        }
        currentReflexTime = Mathf.Clamp(currentReflexTime, 0, maxReflexTime);
    }
    public void TriggerHitStop(float duration) {
        if (isHitStopping) return;
        StartCoroutine(HitStopRoutine(duration));
    }
    IEnumerator HitStopRoutine(float duration) {
        isHitStopping = true;
        Time.timeScale = 0f; 
        yield return new WaitForSecondsRealtime(duration); 
        isHitStopping = false; 
        Time.timeScale = 1f; 
    }
    void CreateDust(GameObject dustPrefab, float direction) {
        if (dustPrefab != null && groundCheckPoint != null) {
            GameObject dust = Instantiate(dustPrefab, groundCheckPoint.position, Quaternion.identity);
            Vector3 theScale = dust.transform.localScale;
            if (direction != 0) {
                if (direction > 0) theScale.x = -Mathf.Abs(theScale.x); 
                else theScale.x = Mathf.Abs(theScale.x); 
            }
            dust.transform.localScale = theScale;
        }
    }
    void Crouching() {
        var kb = Keyboard.current;
        if (kb == null) return;
        bool isDownPressed = kb.sKey.isPressed || kb.downArrowKey.isPressed;
        if (isDownPressed && !isCrouching) {
            isCrouching = true;
            if(animator) animator.SetBool("isCrouching", true);
            ResizeCollider(true);
        } else if (!isDownPressed && isCrouching) {
            isCrouching = false;
            if(animator) animator.SetBool("isCrouching", false);
            ResizeCollider(false);
        }
    }
    void ResizeCollider(bool crouching) {
        if (playerCollider == null) return;
        if (crouching) {
            playerCollider.size = new Vector2(standSize.x, standSize.y * 0.5f);
            playerCollider.offset = new Vector2(standOffset.x, standOffset.y - (standSize.y * 0.25f));
        } else {
            playerCollider.size = standSize;
            playerCollider.offset = standOffset;
        }
    }
    void Rolling() {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;
        if (kb.leftShiftKey.wasPressedThisFrame && Time.time >= nextRollTime) {
            float direction = 0f;
            if (kb.aKey.isPressed) direction = -1f;
            else if (kb.dKey.isPressed) direction = 1f;
            if (direction == 0f && mouse != null) {
                Vector2 mousePos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
                direction = mousePos.x > transform.position.x ? 1f : -1f;
            }
            if (direction == 0f) direction = spriteRenderer.flipX ? -1f : 1f;
            StartCoroutine(RollRoutine(direction));
        }
    }
    IEnumerator RollRoutine(float dir) {
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;
        int originalLayer = gameObject.layer; 
        int rollLayer = LayerMask.NameToLayer("PlayerRoll");
        if (rollLayer != -1) gameObject.layer = rollLayer; 
        if(animator) {
            animator.SetBool("isCrouching", false);
            animator.SetTrigger("roll");
        }
        if (dir > 0) spriteRenderer.flipX = false;
        else spriteRenderer.flipX = true;
        rb.linearVelocity = new Vector2(dir * rollSpeed, 0f); 
        float elapsedTime = 0f;
        while(elapsedTime < rollDuration) {
            MakeGhost(); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        gameObject.layer = originalLayer;
        isRolling = false;
        rb.linearVelocity = Vector2.zero; 
        var kb = Keyboard.current;
        bool isHoldingDown = kb.sKey.isPressed || kb.downArrowKey.isPressed;
        if (isHoldingDown) { isCrouching = true; if(animator) animator.SetBool("isCrouching", true); ResizeCollider(true); }
        else { isCrouching = false; ResizeCollider(false); }
    }
    void OnDrawGizmosSelected() {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * stairCheckDistance); 
        if (attackPoint != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (wallCheckPoint != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckPoint.position, wallCheckRadius);
        }
    }
    IEnumerator ShowSlashEffect() {
        if (slashEffectGO == null) yield break; 
        slashEffectGO.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        slashEffectGO.SetActive(false);
    }
    IEnumerator AttackLock() { isAttacking = true; yield return new WaitForSeconds(attackLockTime); isAttacking = false; rb.linearVelocity = Vector2.zero; }
    void Attacking() {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;
        bool attackInput = kb.jKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        if (attackInput && Time.time >= nextAttackTime) {
            nextAttackTime = Time.time + attackCooldown;
            if (mouse != null) {
                Vector2 mousePos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
                float directionToMouse = mousePos.x - transform.position.x;
                if (directionToMouse > 0) {
                    spriteRenderer.flipX = false;
                    if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
                } else {
                    spriteRenderer.flipX = true;
                    if (attackPoint != null) attackPoint.localRotation = Quaternion.Euler(0, 180, 0);
                }
            }
            if(animator) animator.SetTrigger("attack");
            if (audioSource != null && slashSound != null) {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(slashSound, slashVolume); 
            }
            StartCoroutine(ShowSlashEffect());
            StartCoroutine(AttackLock()); 
            float lungeDir = spriteRenderer.flipX ? -1f : 1f;
            rb.linearVelocity = new Vector2(lungeDir * attackLungeForce, 0f); 
            Collider2D[] bullets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, bulletLayer);
            foreach (Collider2D b in bullets) {
                EnemyBullet bulletScript = b.GetComponent<EnemyBullet>();
                if (bulletScript != null) {
                    bulletScript.Deflect(); 
                    TriggerHitStop(0.08f); 
                }
            }
            Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
            foreach (Collider2D e in enemies) {
                EnemyHealth eHealth = e.GetComponent<EnemyHealth>();
                if (eHealth != null) {
                    eHealth.TakeDamage(damage, transform.position); 
                    if (CameraShake.instance != null) StartCoroutine(CameraShake.instance.Shake(0.15f, 0.2f));
                    TriggerHitStop(0.15f); 
                }
            }
        }
    }
}