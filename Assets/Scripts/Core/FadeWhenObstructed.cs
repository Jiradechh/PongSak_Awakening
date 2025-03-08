using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWhenObstructed : MonoBehaviour
{
    private Transform player;
    private Transform cameraTransform;
    public LayerMask obstacleLayer;
    public float transparentAlpha = 0.4f; 
    public float opaqueAlpha = 1.0f; 

    private Dictionary<Renderer, Coroutine> fadeCoroutines = new Dictionary<Renderer, Coroutine>();
    private HashSet<Renderer> currentlyFadedObjects = new HashSet<Renderer>();

    private Dictionary<Renderer, (Material, float, int)> originalMaterialSettings = new Dictionary<Renderer, (Material, float, int)>();

    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
       

        cameraTransform = Camera.main?.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("Main Camera not found");
        }
    }

    void Update()
    {
        if (player != null && cameraTransform != null)
        {
            HandleObjectFade();
        }
    }

    private void HandleObjectFade()
    {
        HashSet<Renderer> newlyFadedObjects = new HashSet<Renderer>();

        Vector3 direction = player.position - cameraTransform.position;
        float distance = Vector3.Distance(player.position, cameraTransform.position);

        RaycastHit[] hits = Physics.RaycastAll(cameraTransform.position, direction, distance, obstacleLayer);

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform.gameObject.layer == LayerMask.NameToLayer("Fade3D"))
            {
                Renderer[] renderers = hitTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    newlyFadedObjects.Add(renderer);

                    if (!currentlyFadedObjects.Contains(renderer))
                    {
                        StartSmoothFade(renderer, transparentAlpha, true);
                    }
                }
            }
        }

        foreach (Renderer renderer in currentlyFadedObjects)
        {
            if (!newlyFadedObjects.Contains(renderer))
            {
                StartSmoothFade(renderer, opaqueAlpha, false);
            }
        }

        currentlyFadedObjects = newlyFadedObjects;
    }

    private void StartSmoothFade(Renderer renderer, float targetAlpha, bool isTransparent)
    {
        if (fadeCoroutines.ContainsKey(renderer))
        {
            StopCoroutine(fadeCoroutines[renderer]);
        }

        Coroutine fadeCoroutine = StartCoroutine(FadeObject(renderer, targetAlpha, isTransparent));
        fadeCoroutines[renderer] = fadeCoroutine;
    }

    private IEnumerator FadeObject(Renderer renderer, float targetAlpha, bool isTransparent)
    {
        Material material = renderer.material;

        if (!originalMaterialSettings.ContainsKey(renderer))
        {
            originalMaterialSettings[renderer] = (new Material(material), material.GetFloat("_Surface"), (int)material.GetFloat("_Cull"));
        }

        float currentAlpha = material.color.a;
        float duration = 0.5f;
        float elapsed = 0f;

        if (isTransparent)
        {
            SetMaterialTransparent(material);
        }
        else
        {
            SetMaterialOpaque(material);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsed / duration);
            SetObjectAlpha(renderer, newAlpha);
            yield return null;
        }

        SetObjectAlpha(renderer, targetAlpha);

        if (!isTransparent)
        {
            ResetMaterial(renderer);
            fadeCoroutines.Remove(renderer);
        }
    }

    private void SetObjectAlpha(Renderer renderer, float alpha)
    {
        if (renderer == null || renderer.material == null) return;

        Material material = renderer.material;
        Color color = material.color;
        color.a = alpha;
        material.color = color;
        material.SetFloat("_BaseAlpha", alpha);
    }

    private void SetMaterialTransparent(Material material)
    {
        if (material == null) return;

        material.SetFloat("_Surface", 1);
        material.SetFloat("_Cull", 0);
        material.renderQueue = 3000;
    }

    private void SetMaterialOpaque(Material material)
    {
        if (material == null) return;

        material.SetFloat("_Surface", 0);
        material.SetFloat("_Cull", 0); 
        material.renderQueue = 2000; 
    }

    private void ResetMaterial(Renderer renderer)
    {
        if (originalMaterialSettings.TryGetValue(renderer, out var originalSettings))
        {
            Material originalMaterial = originalSettings.Item1;
            float originalSurface = originalSettings.Item2;
            int originalCullMode = originalSettings.Item3;

            renderer.material.CopyPropertiesFromMaterial(originalMaterial);
            renderer.material.SetFloat("_Surface", originalSurface);
            renderer.material.SetFloat("_Cull", originalCullMode);
            renderer.material.renderQueue = (originalSurface == 1) ? 3000 : 2000;

            originalMaterialSettings.Remove(renderer);
        }
    }
}
