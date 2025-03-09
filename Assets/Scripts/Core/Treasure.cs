using UnityEngine;

public class Treasure : MonoBehaviour
{
    public int goldAmount = 5;
    public int gemAmount = 5;
    public ParticleSystem particle;
    private bool isCollected = false;

    private Animator treasureAnimator;

    public AudioSource audioSource;

    public AudioClip openTreasure;

    private void Awake()
    {
        treasureAnimator = GetComponent<Animator>();
        if (treasureAnimator == null)
        {
            Debug.LogError("❌ No Animator component found on the treasure!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            CollectTreasure();
            audioSource.PlayOneShot(openTreasure);
        }
    }

    private void CollectTreasure()
    {
        if (treasureAnimator != null)
        {
            treasureAnimator.SetTrigger("OpenChest");
        }

        GameManager.Instance.AddGold(goldAmount);
        GameManager.Instance.AddGems(gemAmount);

        Debug.Log($"🎁 Treasure Collected! +{goldAmount} Gold, +{gemAmount} Gems");

      //  Destroy(gameObject, 0.5f);
    }

    private void PlayParticle()
    {
        Debug.Log("particle");
        particle.Play();
    }
}
