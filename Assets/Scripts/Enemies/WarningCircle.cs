using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningCircle : MonoBehaviour
{
    public float fadeInTime = 1.5f;
    public float shakeMagnitude = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.localPosition;
        StartCoroutine(FadeInWithShake());
    }

    public void SetWarningScale(float radius)
    {
        float scale = radius * 2f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private IEnumerator FadeInWithShake()
    {
        Color color = spriteRenderer.color;
        float elapsedTime = 0f;
        bool shakeStarted = false;

        color.a = 0f;
        spriteRenderer.color = color;

        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            spriteRenderer.color = color;

            if (elapsedTime >= fadeInTime / 2f && !shakeStarted)
            {
                shakeStarted = true;
                StartCoroutine(Shake(fadeInTime / 1.5f));
            }

            yield return null;
        }
        color.a = 1f;
        spriteRenderer.color = color;
        Destroy(gameObject);
    }

    private IEnumerator Shake(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}