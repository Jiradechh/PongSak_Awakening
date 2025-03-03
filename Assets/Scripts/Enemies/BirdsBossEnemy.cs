using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BirdsBossEnemy : MonoBehaviour
{
    #region Public Variables
    public Transform point1;
    public Transform point2;
    public Transform retreatPoint;

    public float moveSpeed = 2f;
    public float attackInterval = 22f;
    public GameObject meteorPrefab1; 
    public GameObject meteorPrefab2; 
    public GameObject warningCirclePrefab;
    public Transform[] meteorSpawnPoints;
    public float warningDuration = 2f;
    public float meteorFallSpeed = 5f;

    public Transform player;
    public float meteorTrackSpeed = 5f;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public int maxHealth = 100;
    private int currentHealth;

    public bool isDead = false;
    public bool isHurt = false;
    #endregion

    private Transform currentTarget;
    private bool isAttacking = false;
    private bool hasMovedToRetreat = false;
    private int currentDamageBuff = 0;

    #region Unity Callbacks
    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found GameObject has the tag 'Player'.");
        }

        currentHealth = maxHealth;
        currentTarget = point1;
        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        if (!isAttacking)
        {
            MoveBetweenPoints();
        }
    }
    #endregion

    #region Damage Logic
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= (damage + currentDamageBuff);
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining health: " + currentHealth);
        animator.SetTrigger("Hurt");
        isHurt = true;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke("EndHurt", 0.5f);
        }
    }

    private void Die()
    {
        Debug.Log("Boss has died!");
        isDead = true;
        animator.SetTrigger("Die");
        currentTarget = null;
        isAttacking = false;

        StopAllCoroutines();

        Destroy(gameObject, 1f);
    }

    #endregion

    #region Movement Logic
    private void MoveBetweenPoints()
    {
        if (isDead) return;

        float step = moveSpeed * Time.deltaTime;

        if (currentTarget != null)
        {
            animator.SetBool("isMoving", true);
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, step);
            FlipSprite();

            if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
            {
                currentTarget = currentTarget == point1 ? point2 : point1;
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void FlipSprite()
    {
        if (currentTarget.position.x > transform.position.x && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
        else if (currentTarget.position.x < transform.position.x && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
    }
    #endregion

    #region Attack Logic
    private IEnumerator AttackRoutine()
    {
        float timeElapsed = 0f;

        while (true)
        {
            if (timeElapsed < 20f && !hasMovedToRetreat)
            {
                yield return new WaitForSeconds(2f);
                isAttacking = true;

                int attackType = Random.Range(1, 4);
                if (attackType == 1)
                {
                    StartCoroutine(AttackType1());
                }
                else if (attackType == 2)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        StartCoroutine(AttackType2());
                        yield return new WaitForSeconds(0.3f);
                    }
                }
                else
                {
                    StartCoroutine(AttackType3());
                }

                isAttacking = false;
                timeElapsed += 3f;
            }
            else if (timeElapsed >= 20f)
            {
                currentTarget = retreatPoint;
                hasMovedToRetreat = true;

                animator.SetBool("isMoving", true);

                while (Vector3.Distance(transform.position, retreatPoint.position) > 0.1f)
                {
                    yield return null;
                }

                isAttacking = true;
                animator.SetBool("isMoving", false);
                animator.SetBool("atRetreatPoint", true);

                yield return new WaitForSeconds(4f);

                animator.SetBool("atRetreatPoint", false);
                timeElapsed = 0f;
                currentTarget = point1;
                hasMovedToRetreat = false;
                isAttacking = false;
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator AttackType1()
    {
        int[] attackPoints = new int[] { 0, 2, 6, 8 };

        DisplayWarningCircles(attackPoints, new Vector3(21, 21, 1));
        yield return new WaitForSeconds(warningDuration);

        SpawnMeteors(attackPoints, meteorPrefab1);
    }

    private IEnumerator AttackType2()
    {
        if (player != null)
        {
            Vector3 lastPlayerPosition = player.position;
            GameObject warningCircle = Instantiate(warningCirclePrefab, lastPlayerPosition, Quaternion.Euler(90, 0, 0));
            warningCircle.transform.localScale = new Vector3(14, 14, 1);

            yield return new WaitForSeconds(warningDuration);

            GameObject meteor = Instantiate(meteorPrefab2, lastPlayerPosition + new Vector3(0, 1.5f, 0), Quaternion.identity);
            MeteorProjectile meteorScript = meteor.GetComponent<MeteorProjectile>();
            meteorScript.SetTargetPosition(lastPlayerPosition, meteorFallSpeed);
            meteorScript.StartFalling();

            Destroy(warningCircle);
        }
    }

    private IEnumerator AttackType3()
    {
        int[] randomPoints = GetRandomPoints(3);

        DisplayWarningCircles(randomPoints, new Vector3(14, 14, 1));
        yield return new WaitForSeconds(warningDuration);

        SpawnMeteors(randomPoints, meteorPrefab2);
    }

    private int[] GetRandomPoints(int count)
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < meteorSpawnPoints.Length; i++)
        {
            availableIndices.Add(i);
        }

        List<int> selectedIndices = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            selectedIndices.Add(availableIndices[randomIndex]);
            availableIndices.RemoveAt(randomIndex);
        }

        return selectedIndices.ToArray();
    }

    private void DisplayWarningCircles(int[] pattern, Vector3 scale)
    {
        foreach (int index in pattern)
        {
            Vector3 spawnPoint = meteorSpawnPoints[index].position;
            GameObject warningCircle = Instantiate(warningCirclePrefab, spawnPoint, Quaternion.Euler(90, 0, 0));
            warningCircle.transform.localScale = scale;
        }
    }

    private void SpawnMeteors(int[] pattern, GameObject meteorPrefab)
    {
        foreach (int index in pattern)
        {
            Vector3 spawnPoint = meteorSpawnPoints[index].position;
            GameObject meteor = Instantiate(meteorPrefab, spawnPoint + new Vector3(0, 1.5f, 0), Quaternion.identity);
            MeteorProjectile meteorScript = meteor.GetComponent<MeteorProjectile>();
            meteorScript.SetTargetPosition(spawnPoint, meteorFallSpeed);
            meteorScript.StartFalling();
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}