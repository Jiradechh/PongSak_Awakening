using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystemGold : MonoBehaviour
{
    #region Public Variables
    public GameObject shopUI;
    public GameObject[] itemButtons;
    public Transform[] spawnPoints;
    public int[] goldPrices;
    #endregion

    #region Private Variables
    private bool isShopOpen = false;
    private bool playerIsNear = false;
    private int selectedIndex = 0;
    private List<GameObject> spawnedItems = new List<GameObject>();
    private Dictionary<GameObject, int> itemPriceMap = new Dictionary<GameObject, int>();
    private HashSet<string> purchasedItems = new HashSet<string>();
    private bool shopInitialized = false;
    private bool canPurchase = true;
    private float inputCooldown = 0.2f;
    private float purchaseCooldown = 0.3f;
    private bool canMove = true;

     public Animator npcAnimator; 
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        shopUI.SetActive(false);
        InitializeShop();
    }

    private void Update()
    {
        if (playerIsNear && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button5)))
        {
            ToggleShop(!isShopOpen);
        }

        if (isShopOpen)
        {
            HandleInput();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("IsHi");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            ToggleShop(false);
        }
    }
    #endregion

    #region Shop Logic
   private void ToggleShop(bool open)
{
    if (open)
    {
        isShopOpen = true;
        shopUI.SetActive(true);
        GameManager.Instance.PauseGame();
    }
    else
    {
        CloseShopImmediately();
    }
}



    private void CloseShopImmediately()
    {
        shopUI.SetActive(false);
        isShopOpen = false;
        GameManager.Instance.ResumeGame();
    }
    #endregion

    private void InitializeShop()
    {
        if (shopInitialized) return;

        spawnedItems.Clear();
        itemPriceMap.Clear();

        List<GameObject> availableItems = itemButtons.ToList();
        for (int i = 0; i < spawnPoints.Length && availableItems.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableItems.Count);
            GameObject selectedItem = availableItems[randomIndex];

            GameObject newItem = Instantiate(selectedItem, spawnPoints[i]);
            newItem.transform.localPosition = Vector3.zero;
            newItem.name = selectedItem.name;

            spawnedItems.Add(newItem);
            itemPriceMap[newItem] = goldPrices[Array.IndexOf(itemButtons, selectedItem)];
            availableItems.RemoveAt(randomIndex);
        }

        shopInitialized = true;
        selectedIndex = 0;
        UpdateSelection();
    }

    #region Item Selection Logic
    private void HandleInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool selectPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button0);

        if (horizontalInput == 0) canMove = true;

        if (canMove && spawnedItems.Count > 0)
        {
            if (horizontalInput > 0.5f)
            {
                selectedIndex = (selectedIndex + 1) % spawnedItems.Count;
                UpdateSelection();
                canMove = false;
                Invoke(nameof(ResetMove), inputCooldown);
            }
            else if (horizontalInput < -0.5f)
            {
                selectedIndex = (selectedIndex - 1 + spawnedItems.Count) % spawnedItems.Count;
                UpdateSelection();
                canMove = false;
                Invoke(nameof(ResetMove), inputCooldown);
            }
        }

        if (selectPressed && canPurchase)
        {
            StartCoroutine(PurchaseItem());
            canPurchase = false;
        }
    }

    private void ResetMove() => canMove = true;

    private void UpdateSelection()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            Image itemImage = spawnedItems[i].GetComponent<Image>();
            itemImage.color = (i == selectedIndex) ? Color.red : Color.yellow;
        }
    }
    #endregion

    #region Purchase Logic
   private IEnumerator PurchaseItem()
    {
        if (spawnedItems.Count == 0 || selectedIndex < 0 || selectedIndex >= spawnedItems.Count)
        {
            canPurchase = true;
            yield break;
        }

        GameObject selectedItem = spawnedItems[selectedIndex];

        if (!itemPriceMap.ContainsKey(selectedItem))
        {
            canPurchase = true;
            yield break;
        }

        int goldPrice = itemPriceMap[selectedItem];

        if (!GameManager.Instance.SpendGold(goldPrice))
        {
            canPurchase = true;
            yield break;
        }

        ApplyItemEffect(selectedItem.name);
        purchasedItems.Add(selectedItem.name);
        Destroy(spawnedItems[selectedIndex]);
        spawnedItems.RemoveAt(selectedIndex);

        Debug.Log($"Item Purchased: {selectedItem.name}");

        yield return new WaitForSecondsRealtime(0.1f);

        CloseShopImmediately(); 
    }

    #endregion

    #region Item Effect Logic
    private void ApplyItemEffect(string itemName)
    {
        switch (itemName)
        {
            case "MaxDash":
                PlayerController.Instance.IncreaseMaxDashes();
                break;

            case "DropGoldFromEnvironment":
                PlayerController.Instance.EnableEnvironmentGoldDrop();
                break;

            case "RegenHp":
                PlayerController.Instance.StartRegenHp();
                break;

            case "ArmorAbsorbHit":
                PlayerController.Instance.EnableArmorAbsorption();
                break;

            case "ReviveOnce":
                PlayerController.Instance.EnableOneTimeRevive();
                break;

            case "AOEProjectile":
                PlayerController.Instance.EnableAOEProjectile();
                break;
        }
    }
    #endregion
}