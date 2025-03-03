using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float damage = 50f;
    public float lifeTime = 10f;
    public float fallHeightOffset = 1.5f;
    public float explosionRadius = 3f;
    public GameObject explosionEffect;
    public GameObject warningCirclePrefab;

    private Vector3 targetPosition;
    private bool isFalling = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (isFalling)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
            {
                Explode();
            }
        }
    }

    public void SetTargetPosition(Vector3 targetPos, float fallSpeed)
    {
        targetPosition = targetPos;
        speed = fallSpeed;
        transform.position = new Vector3(targetPos.x, targetPos.y + fallHeightOffset, targetPos.z);
    }

    public void StartFalling()
    {
        isFalling = true;
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosionInstance, 1f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            PlayerController playerHealth = nearbyObject.GetComponent<PlayerController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)damage);
                Debug.Log("Meteor hit Player for " + damage + " damage.");
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}