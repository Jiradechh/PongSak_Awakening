using System.Collections;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    [Header("Trap Settings")]
    public float delayBeforeTrigger = 0.5f;
    public float activeDuration = 2f;
    public int damageAmount = 25;
    
    private bool isTriggered = false;
    private bool isActive = false;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            isTriggered = true;
            StartCoroutine(ActivateTrap());
        }
        
        if (isActive)
        {
            ApplyDamage(other);
        }
    }

    private IEnumerator ActivateTrap()
    {
        yield return new WaitForSeconds(delayBeforeTrigger);

        isActive = true;

        if (animator != null)
        {
            animator.SetTrigger("Activated");
        }

        yield return new WaitForSeconds(activeDuration);

        isActive = false; 
    }

    private void ApplyDamage(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damageAmount);
            }
        }
        else if (target.CompareTag("Enemy"))
        {
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }
        }
    }
}
