using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShopGold : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopUI;
    public Transform[] spawnPoints;
    public GameObject[] itemPrefabs;
    public GameObject confirmationPopup;
    public TextMeshProUGUI confirmationText;//
    public Button confirmButton;
    public Button cancelButton;
    public PlayerInput playerInput;

    [Header("Selection System")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    private int selectedIndex = 0;

    private bool isShopOpen = false;
    private bool isPlayerNear = false;
    private bool isPopupActive = false;
    private GameObject[] displayedItems;
    private Dictionary<GameObject, int> itemPrices = new Dictionary<GameObject, int>();
    private List<GameObject> availableItems;

    private void Start()
    {
        shopUI.SetActive(false);
        confirmationPopup.SetActive(false);
        displayedItems = new GameObject[spawnPoints.Length];

        if (playerInput != null)
        {
            playerInput.actions["OpenShop"].performed += ctx => TryOpenShop();
            playerInput.actions["CloseShop"].performed += ctx => CloseShop();
            playerInput.actions["Navigate"].performed += ctx => Navigate(ctx.ReadValue<Vector2>());
            playerInput.actions["Select"].performed += ctx => OpenConfirmationPopup();
        }
        else
        {
            Debug.LogWarning("PlayerInput is not assigned in ShopGold!");
        }

        availableItems = new List<GameObject>(itemPrefabs);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            CloseShop();
        }
    }

    private void TryOpenShop()
    {
        if (isPlayerNear)
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        isShopOpen = true;
        shopUI.SetActive(true);
        Time.timeScale = 0f;  
        SelectRandomItems();
        UpdateSelection();
    }

    private void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        confirmationPopup.SetActive(false);
        Time.timeScale = 1f;
        DestroyShopItems();
    }

    private void SelectRandomItems()
    {
        List<GameObject> itemsToChoose = new List<GameObject>(availableItems);
        itemPrices.Clear();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (itemsToChoose.Count == 0) break;

            int randomIndex = Random.Range(0, itemsToChoose.Count);
            GameObject selectedItem = itemsToChoose[randomIndex];

            displayedItems[i] = Instantiate(selectedItem, spawnPoints[i]);
            displayedItems[i].name = selectedItem.name;

            itemPrices[displayedItems[i]] = GetItemPrice(selectedItem.name);

            itemsToChoose.RemoveAt(randomIndex);
        }
    }

    private int GetItemPrice(string itemName)
    {
        switch (itemName)
        {
            case "MaxDash": return 50;
            case "DropGoldFromEnvironment": return 75;
            case "RegenHp": return 100;
            case "ArmorAbsorbHit": return 150;
            case "ReviveOnce": return 200;
            case "AOEProjectile": return 125;
            default: return 50;
        }
    }

    private void Navigate(Vector2 direction)
    {
        if (!isShopOpen || isPopupActive) return;

        if (direction.x > 0) selectedIndex = (selectedIndex + 1) % displayedItems.Length;
        if (direction.x < 0) selectedIndex = (selectedIndex - 1 + displayedItems.Length) % displayedItems.Length;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < displayedItems.Length; i++)
        {
            if (displayedItems[i] != null)
            {
                displayedItems[i].GetComponent<Image>().color = (i == selectedIndex) ? highlightColor : normalColor;
            }
        }
    }

    private void OpenConfirmationPopup()
    {
        if (!isShopOpen || displayedItems[selectedIndex] == null) return;

        int price = itemPrices[displayedItems[selectedIndex]];
        confirmationText.text = $"Confirm Purchase: {price} Gold?";

        confirmationPopup.SetActive(true);
        isPopupActive = true;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => ConfirmPurchase());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => CloseConfirmationPopup());
    }

    private void CloseConfirmationPopup()
    {
        confirmationPopup.SetActive(false);
        isPopupActive = false;
    }

    private void ConfirmPurchase()
    {
        int price = itemPrices[displayedItems[selectedIndex]];
        if (GameManager.Instance.SpendGold(price))
        {
            ApplyItemEffect(displayedItems[selectedIndex].name);
            Destroy(displayedItems[selectedIndex]);
            displayedItems[selectedIndex] = null;
            CloseConfirmationPopup();
        }
        else
        {
            Debug.Log("Not enough gold!");
            CloseConfirmationPopup();
        }
    }

    private void ApplyItemEffect(string itemName)
    {
        PlayerController player = FindObjectOfType<PlayerController>();

        switch (itemName)
        {
            case "MaxDash":
                player.IncreaseMaxDashes();
                Debug.Log("Max Dash Increased!");
                break;
            case "DropGoldFromEnvironment":
                player.EnableEnvironmentGoldDrop();
                Debug.Log("Gold now drops from environment!");
                break;
            case "RegenHp":
                player.StartRegenHp();
                Debug.Log("HP Regen Activated!");
                break;
            case "ArmorAbsorbHit":
                player.EnableArmorAbsorption();
                Debug.Log("Armor Absorption Active!");
                break;
            case "ReviveOnce":
                player.EnableOneTimeRevive();
                Debug.Log("Revive Once Enabled!");
                break;
            case "AOEProjectile":
                player.EnableAOEProjectile();
                Debug.Log("AOE Projectile Unlocked!");
                break;
            default:
                Debug.LogWarning($"Unknown Item Effect: {itemName}");
                break;
        }
    }

    private void DestroyShopItems()
    {
        foreach (var item in displayedItems)
        {
            if (item != null) Destroy(item);
        }
    }
}
