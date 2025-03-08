using System.Collections;
using UnityEngine;

public class FadeSpriteWhenObstructed : Singleton<FadeSpriteWhenObstructed>
{
    private Transform player;
    private Camera mainCamera;
    public string obstacleName = "Pillar"; 
    public float transparentAlpha = 0.4f;
    public float opaqueAlpha = 1.0f;
    private SpriteRenderer targetSpriteRenderer; 

    private Coroutine fadeCoroutine;

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
      

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found");
        }

        GameObject obstacleObject = GameObject.Find(obstacleName);
        if (obstacleObject != null)
        {
            targetSpriteRenderer = obstacleObject.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (player != null && mainCamera != null && targetSpriteRenderer != null)
        {
            HandleSpriteFade();
        }
    }

    private void HandleSpriteFade()
    {
        Vector3 direction = (mainCamera.transform.position - player.position).normalized;
        float distance = Vector3.Distance(player.position, mainCamera.transform.position);

        RaycastHit hit;
        if (Physics.Raycast(player.position, direction, out hit, distance))
        {
            if (hit.collider.gameObject.name == obstacleName)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeSprite(targetSpriteRenderer, transparentAlpha));
            }
            else
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeSprite(targetSpriteRenderer, opaqueAlpha)); 
            }
        }
        else
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeSprite(targetSpriteRenderer, opaqueAlpha));
        }
    }

    private IEnumerator FadeSprite(SpriteRenderer spriteRenderer, float targetAlpha)
    {
        if (spriteRenderer == null)
            yield break;

        float currentAlpha = spriteRenderer.color.a;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsed / duration);
            SetSpriteAlpha(spriteRenderer, newAlpha);
            yield return null;
        }

        SetSpriteAlpha(spriteRenderer, targetAlpha);
    }

    private void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
