using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public int damage = 50;
    public float lifetime = 5f;
    public bool isAOE = false;
    public float explosionRadius = 3.0f; 
    public int explosionDamage = 30;
    public SpriteRenderer spriteRenderer;

    private Vector3 moveDirection; 

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float projectileSpeed, int projectileDamage, bool isAOEAttack)
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        isAOE = isAOEAttack;

        spriteRenderer.flipX = moveDirection.x > 0;

        Debug.Log($"Projectile launched with direction: {moveDirection}, Speed: {speed}, Damage: {damage}, AOE: {isAOE}");
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        transform.rotation = Quaternion.identity;
    }

        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (isAOE)
            {
                ApplyAOEDamage();
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }


        private void ApplyAOEDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (isAOE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
