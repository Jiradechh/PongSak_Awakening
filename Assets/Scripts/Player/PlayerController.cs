using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : Singleton<PlayerController>
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool hasArmorAbsorb = false;
    private bool canReviveOnce = false; //
  
    [Header("UI Elements")]
    public Image heartImage; 
    public GameObject arrowSprite;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float dashForce = 15f;
    public float dashCooldown = 0.5f;
    private int maxDashes = 1;
    private int currentDashes;

    [Header("Attack Settings")]
    public float lightAttackDamage = 10f;
    public float heavyAttackDamage = 25f;
    public float attackRange = 1.5f;
    public float lightAttackCooldown = 0.3f;
    public float heavyAttackCooldown = 0.7f;
    private bool canLightAttack = true;
    private bool canHeavyAttack = true;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float projectileDamage = 50f;
    public float reloadTime = 3f;
    private bool canShoot = true;
    private bool canShootAOEProjectile = false;

    [Header("Firepoint Settings")]
    public Transform firepoint;
    public float firepointRotationSpeed = 10f;

    [Header("Sprite & Animation")]
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Components")]
    public Rigidbody rb;
    private Gamepad gamepad;

    private Vector3 lastMoveDirection = Vector3.forward;

    private Vector2 moveInput;
    private bool canDash = true;
    private bool canMove = true;
    private bool isPlayingAnimation = false;
    private bool isAttackAnimation = false;
    private bool isGrounded;
    private Vector3 lastShootDirection;
    private bool isInvulnerable = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        gamepad = Gamepad.current;
        currentHealth = maxHealth;
        lastShootDirection = Vector3.forward;
        currentDashes = maxDashes;
         currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        HandleGroundCheck();

        if (currentHealth > 0)
        {
            if (!isPlayingAnimation || !isAttackAnimation)
            {
                HandleMovement();
                HandleFirepointRotation();
            }
            HandleInput();
        }
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }
void HandleMovement()
{
    if (!canMove || gamepad == null || isAttackAnimation) return;

    moveInput = gamepad.leftStick.ReadValue();
    float speed = Mathf.Lerp(walkSpeed, runSpeed, moveInput.magnitude);
    Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * speed;
    rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

    if (moveInput.magnitude > 0.1f)
    {
        lastMoveDirection = movement.normalized;
    }

    if (arrowSprite != null)
    {
        float angle = Mathf.Atan2(lastMoveDirection.x, lastMoveDirection.z) * Mathf.Rad2Deg;
        arrowSprite.transform.rotation = Quaternion.Euler(90f, angle, 0f); 
    }

    if (moveInput.x != 0)
        spriteRenderer.flipX = moveInput.x < 0;

    if (moveInput.magnitude > 0.1f)
    {
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            PlayAnimation("P_SideWalk");
        }
        else
        {
            PlayAnimation("P_Walk"); 
        }
    }
    else
    {
        PlayAnimation("P_Idle");
    }
}

    void HandleFirepointRotation()
    {
        if (gamepad == null || firepoint == null) return;

        Vector2 input = gamepad.leftStick.ReadValue();
        if (input.magnitude > 0.1f)
        {
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
            lastShootDirection = direction;
            firepoint.position = transform.position + direction * 1.0f;
        }
    }

  void HandleInput()
{
    if (gamepad == null) return;

    if (gamepad.buttonSouth.wasPressedThisFrame && canDash && isGrounded && currentDashes > 0)
        StartCoroutine(DashCoroutine());

    if (gamepad.buttonWest.wasPressedThisFrame && canLightAttack)
        StartCoroutine(AttackRoutine(lightAttackDamage, "P_LAttack", true));

    if (gamepad.buttonNorth.wasPressedThisFrame && canHeavyAttack)
        StartCoroutine(AttackRoutine(heavyAttackDamage, "P_HAttack", false));

    if (gamepad.buttonEast.wasPressedThisFrame)
        StartCoroutine(ShootProjectileRoutine());
}


    IEnumerator ShootProjectileRoutine()
    {
        if (!canShoot || projectilePrefab == null || firepoint == null) yield break;
        
        if (this == null) yield break;  

        canShoot = false;
        canMove = false;
        isPlayingAnimation = true;
        PlayAnimation("P_CastSpell", true);

        yield return new WaitForSeconds(0.2f);

        if (this != null)
            SpawnProjectile();

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - 0.2f);

        isPlayingAnimation = false;
        canMove = true;
        yield return new WaitForSeconds(reloadTime);
        canShoot = true;
    }


    void SpawnProjectile()
{
    if (firepoint == null || projectilePrefab == null) return;  

    GameObject projectile = Instantiate(projectilePrefab, firepoint.position, Quaternion.identity);
    if (projectile == null) return; 

    SpellProjectile projectileScript = projectile.GetComponent<SpellProjectile>();

    if (projectileScript != null)
    {
        Vector3 direction = lastShootDirection.magnitude > 0.1f ? lastShootDirection : Vector3.forward;
        projectileScript.Launch(direction, projectileSpeed, (int)projectileDamage, canShootAOEProjectile);
    }
}

IEnumerator AttackRoutine(float damage, string animationName, bool isLightAttack)
{
    if (isLightAttack && !canLightAttack) yield break;
    if (!isLightAttack && !canHeavyAttack) yield break;

    if (isLightAttack)
        canLightAttack = false;
    else
        canHeavyAttack = false;

    canMove = false;
    isPlayingAnimation = true;
    PlayAnimation(animationName, true);

    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length * 0.1f); 

    Collider[] hitEnemies = Physics.OverlapSphere(firepoint.position, attackRange, LayerMask.GetMask("Enemy", "Environment", "DamagePillar"));
    foreach (var enemy in hitEnemies)
    {
        if (enemy.TryGetComponent(out EnemyController enemyController))
        {
            enemyController.TakeDamage((int)damage);
            Debug.Log($"⚔ Hit {enemy.name} for {damage} damage!");
        }
        if (enemy.TryGetComponent(out BirdsBossEnemy birdsBossEnemy))
        {
            birdsBossEnemy.TakeDamage((int)damage);
        }
        if (enemy.TryGetComponent(out EnvironmentObject env))
        {
            env.TakeDamage((int)damage);
        }
        if (enemy.TryGetComponent(out DamagePillar damagePillar))
        {
            damagePillar.TakeDamage((int)damage);
        }
    }

    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length * 0.5f);

    isPlayingAnimation = false;
    canMove = true;

    if (isLightAttack)
        yield return new WaitForSeconds(lightAttackCooldown);
    else
        yield return new WaitForSeconds(heavyAttackCooldown);

    if (isLightAttack)
        canLightAttack = true;
    else
        canHeavyAttack = true;
}


    

   IEnumerator DashCoroutine()
{
    PlayAnimation("P_Dash", true);
    currentDashes--;

    Vector3 dashDir = moveInput.magnitude > 0.1f ? new Vector3(moveInput.x, 0, moveInput.y).normalized : lastMoveDirection;

    float dashDuration = 0.3f;
    float elapsed = 0f;

    canMove = false;
    while (elapsed < dashDuration)
    {
        rb.linearVelocity = dashDir * dashForce;
        elapsed += Time.deltaTime;
        yield return null;
    }

    rb.linearVelocity = Vector3.zero;
    canMove = true;
    Invoke(nameof(ReloadDash), dashCooldown);
}

    private void ReloadDash() => currentDashes = maxDashes;

    public void TakeDamage(float damage)
    {
        if (hasArmorAbsorb)
        {
            hasArmorAbsorb = false;
            return;
        }

        if (isInvulnerable) return; 

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        StartCoroutine(HandleHurtAnimation()); 

        if (currentHealth <= 0)
        {
            if (canReviveOnce)
            {
                canReviveOnce = false;
                currentHealth = maxHealth;
                UpdateHealthUI();
            }
            else
            {
                Die();
            }
        }
        else
        {
            StartCoroutine(BecomeInvincible()); 
        }
    }

    IEnumerator HandleHurtAnimation()
    {
        canMove = false; 
        isPlayingAnimation = true;
        anim.Play("P_Hurt"); 

        yield return new WaitForSeconds(0.4f);
        
        canMove = true;
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - 0.4f);

        isPlayingAnimation = false;
    }

    IEnumerator BecomeInvincible()
    {
        isInvulnerable = true;
        float blinkDuration = 1.5f;
        float blinkInterval = 0.15f;
        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; 
            elapsedTime += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }
    public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthUI(); 
        }

        private void UpdateHealthUI()
        {
            if (heartImage != null)
            {
                heartImage.fillAmount = currentHealth / maxHealth;
            }
        }

   void Die()
{
    anim.Play("P_Die");
    if (GameManager.Instance != null)
    {
        GameManager.Instance.PlayerDied();
    }
}
    void OnDrawGizmosSelected()
    {
        if (firepoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firepoint.position, attackRange);
        }
    }


    public void IncreaseMaxDashes() => maxDashes++;
        public void EnableEnvironmentGoldDrop()
    {
        EnvironmentObject[] environments = FindObjectsOfType<EnvironmentObject>();
        foreach (var env in environments)
        {
            env.EnableGoldDrop();
        }
    }

    public void StartRegenHp() => StartCoroutine(RegenHpCoroutine());
    private IEnumerator RegenHpCoroutine()
    {
        while (currentHealth < maxHealth)
        {
            currentHealth += 1;
            yield return new WaitForSeconds(2f);
        }
    }
    public void EnableArmorAbsorption() => hasArmorAbsorb = true;
    public void EnableOneTimeRevive() => canReviveOnce = true;
    public void EnableAOEProjectile() => canShootAOEProjectile = true;

    void PlayAnimation(string animationName, bool lockDuringAnimation = false)
    {
        if (!anim) return;
        anim.Play(animationName);
    }
}
