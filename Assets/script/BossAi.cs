using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("--- THÔNG SỐ CƠ BẢN ---")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public int currentPhase = 1;

    // 👉 [HIGHLIGHT: CÔNG TẮC KÍCH HOẠT BOSS]
    [Header("--- TRẠNG THÁI ---")]
    public bool isActivated = false; 

    [Header("--- UI THANH MÁU ---")]
    public Image healthBarFill; 

    [Header("--- DI CHUYỂN ---")]
    public Transform player;
    public float phase1Speed = 3f;  
    public float phase2Speed = 6f;  
    private float currentSpeed;
    
    [Tooltip("Khoảng cách Boss đứng lại để xài 1 trong 3 chiêu")]
    public float attackRange = 2.5f; 

    [Header("--- AI THÔNG MINH (NGHỈ NGƠI) ---")]
    public float minWaitPhase1 = 2.0f; 
    public float maxWaitPhase1 = 4.0f; 
    public float minWaitPhase2 = 1.0f; 
    public float maxWaitPhase2 = 2.0f; 
    private float currentCooldown;
    private float attackTimer = 0f;

    [Header("--- SÁT THƯƠNG ---")]
    public Transform attackPoint;   
    public float attackRadius = 1f; 
    public int attackDamage = 20;   
    public LayerMask playerLayer;   

    [Header("--- SKILL ĐẬP ĐẤT (PHASE 2) ---")]
    public float slamRadius = 3.5f; 
    public int slamDamage = 40;     

    [Header("--- ULTIMATE SKILL (PHASE 3) ---")]
    public float ultiDrawTime = 1.0f;     
    public float ultiSlashInterval = 0.2f;
    public float ultiSheatheTime = 1.0f;  
    private bool hasUsedUltimate = false; 

    [Header("--- THÀNH PHẦN ---")]
    public Animator animator;
    public Animator swordAnimator; 
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    private bool isAttacking = false;
    private bool isTransitioning = false;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentSpeed = phase1Speed;
        
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;

        currentCooldown = Random.Range(minWaitPhase1, maxWaitPhase1);
        
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return; 
        if (isDead) return;

        // 👉 [HIGHLIGHT: NẾU CHƯA KÍCH HOẠT THÌ ĐỨNG IM]
        if (!isActivated) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if(animator) animator.SetFloat("moveSpeed", 0f);
            return; 
        }

        if (isTransitioning || player == null) 
        {
            if (isTransitioning) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            return;
        }

        LookAtPlayer();

        if (!isAttacking)
        {
            MoveTowardsPlayer();
            HandleAttackTimer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("moveSpeed", 0f);
        }
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false; 
            if (attackPoint != null) 
                attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
        }
        else
        {
            spriteRenderer.flipX = true;  
            if (attackPoint != null) 
                attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
        }
    }

    void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            float direction = (player.position.x > transform.position.x) ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * currentSpeed, rb.linearVelocity.y);
            animator.SetFloat("moveSpeed", currentSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("moveSpeed", 0f); 
        }
    }

    void HandleAttackTimer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= currentCooldown)
            {
                attackTimer = 0f;
                int randomMove = Random.Range(1, 4); 
                StartCoroutine(ExecuteAttack(randomMove));

                currentCooldown = (currentPhase == 1) ? 
                    Random.Range(minWaitPhase1, maxWaitPhase1) : 
                    Random.Range(minWaitPhase2, maxWaitPhase2);
            }
        }
    }

    IEnumerator ExecuteAttack(int moveIndex)
    {
        isAttacking = true;
        animator.ResetTrigger("AttackNormal");
        animator.ResetTrigger("AttackStrong");
        animator.ResetTrigger("attack");

        string triggerName = (moveIndex == 1) ? "AttackNormal" : (moveIndex == 2) ? "AttackStrong" : "attack";
        float animWaitTime = (moveIndex == 1) ? 0.8f : (moveIndex == 2) ? 1.2f : 1.5f;

        if (currentPhase == 2) animWaitTime *= 0.7f; 

        animator.SetTrigger(triggerName); 
        yield return new WaitForSeconds(animWaitTime); 
        isAttacking = false;
    }

    public void DealDamage()
    {
        if (attackPoint == null) return;
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hitPlayer != null)
        {
            // Trừ máu trực tiếp vào PlayerHealth
            PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();
            if(health != null) health.TakeDamage(attackDamage, transform.position);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || (isTransitioning && currentPhase < 3)) return; 
        
        currentHealth -= damageAmount;
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
            return; 
        }

        animator.SetTrigger("hurt");
        spriteRenderer.color = Color.red; 
        Invoke("ResetColor", 0.1f); 

        if (currentHealth <= 200f && !hasUsedUltimate)
        {
            hasUsedUltimate = true;
            StopAllCoroutines(); 
            StartCoroutine(ExecuteUltimate()); 
            return; 
        }

        if (currentHealth <= maxHealth / 2 && currentPhase == 1 && !isTransitioning)
        {
            StartCoroutine(TransitionToPhase2());
        }
    }

    void ResetColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    IEnumerator TransitionToPhase2()
    {
        isTransitioning = true; 
        currentPhase = 2;
        isAttacking = false;
        
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("roar"); 
        yield return new WaitForSeconds(1.5f); 

        animator.ResetTrigger("roar"); 
        animator.SetTrigger("phaseTransition"); 
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(0.5f); 

        if (player != null)
        {
            transform.position = new Vector2(player.position.x, player.position.y + 6f); 
        }
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f); 
        
        yield return new WaitForSeconds(0.2f); 

        spriteRenderer.enabled = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f); 
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        
        animator.ResetTrigger("phaseTransition"); 
        animator.SetTrigger("landSmash"); 
        
        yield return new WaitForSeconds(0.2f); 

        rb.gravityScale = originalGravity; 
        rb.linearVelocity = new Vector2(0f, -25f); 
        
        float dropTimer = 0f;
        while (rb.linearVelocity.y < -0.1f && dropTimer < 2f)
        {
            dropTimer += Time.deltaTime;
            yield return null; 
        }

        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, slamRadius, playerLayer);
        if (hitPlayer != null)
        {
            PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();
            if(health != null) health.TakeDamage(slamDamage, transform.position);
        }

        currentSpeed = phase2Speed;
        animator.speed = 1.3f; 
        
        yield return new WaitForSeconds(1f); 
        
        isTransitioning = false; 
    }

    public IEnumerator ExecuteUltimate()
    {
        isTransitioning = true; 
        isAttacking = true;
        currentPhase = 3;
        rb.linearVelocity = Vector2.zero; 

        animator.SetTrigger("ultiDraw"); 
        yield return new WaitForSeconds(ultiDrawTime); 

        Playermovement playerScript = null;
        if (player != null)
        {
            playerScript = player.GetComponent<Playermovement>();
            if (playerScript != null) playerScript.LockPlayerForUltimate(true);
        }

        for (int i = 1; i <= 9; i++)
        {
            animator.SetTrigger("ultiSlash"); 
            
            if (swordAnimator != null && player != null) 
            {
                swordAnimator.transform.position = new Vector3(player.position.x, player.position.y + 0.5f, 0f);
                SpriteRenderer swordSprite = swordAnimator.GetComponent<SpriteRenderer>();
                if (swordSprite != null) swordSprite.flipX = Random.value > 0.5f; 
                swordAnimator.SetTrigger("ultiSlash"); 
            }

            float dmg = (i == 9) ? 10f : 5f; 

            if (player != null) 
            {
                PlayerHealth healthScript = player.GetComponent<PlayerHealth>();
                if (healthScript != null) healthScript.TakeDamage(dmg, transform.position);
                if (playerScript != null) playerScript.TriggerHitStop(0.05f); 
            }
            
            yield return new WaitForSeconds(ultiSlashInterval); 
        }

        animator.SetTrigger("ultiSheathe");
        if (swordAnimator != null) swordAnimator.SetTrigger("ultiSheathe");
        yield return new WaitForSeconds(ultiSheatheTime); 

        if (playerScript != null) playerScript.LockPlayerForUltimate(false);
        
        isAttacking = false;
        isTransitioning = false; 
    }

    public void DrawSwordEvent()
    {
        if (swordAnimator != null) 
        {
            swordAnimator.SetTrigger("ultiDraw");
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines(); 
        CancelInvoke("ResetColor");
        
        animator.SetTrigger("die");
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll; 
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (healthBarFill != null && healthBarFill.transform.parent != null)
        {
            healthBarFill.transform.parent.gameObject.SetActive(false);
        }
        
        Destroy(gameObject, 4f); 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange); 
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}