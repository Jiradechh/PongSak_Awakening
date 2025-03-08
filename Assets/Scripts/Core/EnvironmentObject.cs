using UnityEngine;

public class EnvironmentObject : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Particle Effect")]
    public GameObject destructionEffect;

    [Header("Gold Drop Settings")]
    public bool canDropGold = false;
    public int[] goldDropAmounts = { 5, 1, 3 };

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            DestroyEnvironment();
        }
    }

    private void DestroyEnvironment()
    {
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        DropGold();
        Destroy(gameObject);
    }

    private void DropGold()
    {
        if (canDropGold && GameManager.Instance != null)
        {
            int goldAmount = goldDropAmounts[Random.Range(0, goldDropAmounts.Length)];
            if (goldAmount > 0)
            {
                GameManager.Instance.AddGold(goldAmount);
                Debug.Log($"💰 Environment dropped {goldAmount} Gold!");
            }
        }
    }


    public void EnableGoldDrop()
    {
        canDropGold = true;
    }
}
