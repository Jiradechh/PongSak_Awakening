using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShopSystemGold : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopUI;
    public Button[] itemButtons;
    public Transform[] spawnPoints;
    public GameObject confirmationPopup;
    public TextMeshProUGUI confirmationText;
    public Button confirmButton;
    public Button cancelButton;
    public PlayerInput playerInput;

    [Header("Selection System")]
    private int selectedIndex = 0;
    private bool isShopOpen = false;
    private bool isPlayerNear = false;
    private bool isPopupActive = false;

    private Dictionary<int, int> itemPrices = new Dictionary<int, int>();
    private List<GameObject> availableItems;
    private List<GameObject> spawnedItems = new List<GameObject>();

    private void Start()
    {
        shopUI.SetActive(false);
        confirmationPopup.SetActive(false);

        if (playerInput != null)
        {
            playerInput.actions["OpenShop"].performed += ctx => TryToggleShop();
            playerInput.actions["Navigate"].performed += ctx => Navigate(ctx.ReadValue<Vector2>());
            playerInput.actions["Select"].performed += ctx => HandleSelect();
        }
        else
        {
            Debug.LogWarning("PlayerInput is not assigned in ShopSystemGold!");
        }

        SetupItemPrices();
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.actions["OpenShop"].performed -= ctx => TryToggleShop();
            playerInput.actions["Navigate"].performed -= ctx => Navigate(ctx.ReadValue<Vector2>());
            playerInput.actions["Select"].performed -= ctx => HandleSelect();
        }
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

    private void TryToggleShop()
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
        SpawnShopItems();
        UpdateSelection();
    }

    private void CloseShop()
    {
        if (shopUI == null || confirmationPopup == null) return;

        isShopOpen = false;
        shopUI.SetActive(false);
        confirmationPopup.SetActive(false);
        Time.timeScale = 1f;
        ClearShopItems();
    }

    private void SetupItemPrices()
    {
        itemPrices[0] = 50;  // MaxDash
        itemPrices[1] = 75;  // DropGoldFromEnvironment
        itemPrices[2] = 100; // RegenHp
        itemPrices[3] = 150; // ArmorAbsorbHit
        itemPrices[4] = 200; // ReviveOnce
        itemPrices[5] = 125; // AOEProjectile
    }

        private void SpawnShopItems()
    {
        ClearShopItems();
        List<int> indices = new List<int> { 0, 1, 2, 3, 4, 5 };
        indices.Shuffle();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int itemIndex = indices[i];
            GameObject item = Instantiate(itemButtons[itemIndex].gameObject, spawnPoints[i]);
            item.name = itemButtons[itemIndex].name;
            spawnedItems.Add(item);
        }
    }


    private void ClearShopItems()
    {
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();
    }

    private void Navigate(Vector2 direction)
    {
        if (!isShopOpen || isPopupActive) return;

        if (direction.x > 0) selectedIndex = (selectedIndex + 1) % spawnedItems.Count;
        if (direction.x < 0) selectedIndex = (selectedIndex - 1 + spawnedItems.Count) % spawnedItems.Count;

        UpdateSelection();
    }

  private void UpdateSelection()
{
    for (int i = 0; i < spawnedItems.Count; i++)
    {
        Button button = spawnedItems[i].GetComponent<Button>();

        if (i == selectedIndex)
        {
            button.Select();
        }
    }
}
        private void HandlePopupSelect()
    {
        if (!isPopupActive) return;

        if (selectedPopupButton == 0)
        {
            ConfirmPurchase();
        }
        else
        {
            CloseConfirmationPopup();
        }
    }

    private void HandleSelect()
    {
        if (isPopupActive)
        {
            ConfirmPurchase();
        }
        else if (isShopOpen)
        {
            OpenConfirmationPopup();
        }
    }
    private int selectedPopupButton = 0;
        private void OpenConfirmationPopup()
    {
        if (!isShopOpen || spawnedItems.Count == 0) return;

        int price = itemPrices.ContainsKey(selectedIndex) ? itemPrices[selectedIndex] : 0;
        confirmationText.text = $"Purchase {price} Gold?";

        confirmationPopup.SetActive(true);
        isPopupActive = true;

        selectedPopupButton = 0;
        EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);

        DisableShopButtons();
    }
    private void UpdatePopupSelection()
    {
        if (!isPopupActive) return;

        if (selectedPopupButton == 0)
        {
            confirmButton.Select();
        }
        else
        {
            cancelButton.Select(); 
        }
        }
    private void NavigatePopup(Vector2 direction)
    {
        if (!isPopupActive) return;

        if (direction.x > 0 || direction.x < 0) // ซ้าย-ขวาเปลี่ยนปุ่ม
        {
            selectedPopupButton = (selectedPopupButton == 0) ? 1 : 0;
            EventSystem.current.SetSelectedGameObject(
                selectedPopupButton == 0 ? confirmButton.gameObject : cancelButton.gameObject
            );
        }
    }
    private void CloseConfirmationPopup()
    {
        confirmationPopup.SetActive(false);
        isPopupActive = false;

        EnableShopButtons(); 
        EventSystem.current.SetSelectedGameObject(spawnedItems[selectedIndex]);
    }
    private void EnableShopButtons()
{
    foreach (var item in spawnedItems)
    {
        item.GetComponent<Button>().interactable = true;
    }
}
    private void DisableShopButtons()
    {
        foreach (var item in spawnedItems)
        {
            item.GetComponent<Button>().interactable = false;
        }
    }
    private void ConfirmPurchase()
    {
        if (GameManager.Instance == null) return;

        int price = itemPrices[selectedIndex];
        if (GameManager.Instance.SpendGold(price))
        {
            ApplyItemEffect(selectedIndex);
            CloseConfirmationPopup();
        }
        else
        {
            Debug.Log("Not enough gold!");
            CloseConfirmationPopup();
        }
    }

    private void ApplyItemEffect(int itemIndex)
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        switch (itemIndex)
        {
            case 0:
                player.IncreaseMaxDashes();
                Debug.Log("Max Dash Increased!");
                break;
            case 1:
                player.EnableEnvironmentGoldDrop();
                Debug.Log("Gold now drops from environment!");
                break;
            case 2:
                player.StartRegenHp();
                Debug.Log("HP Regen Activated!");
                break;
            case 3:
                player.EnableArmorAbsorption();
                Debug.Log("Armor Absorption Active!");
                break;
            case 4:
                player.EnableOneTimeRevive();
                Debug.Log("Revive Once Enabled!");
                break;
            case 5:
                player.EnableAOEProjectile();
                Debug.Log("AOE Projectile Unlocked!");
                break;
            default:
                Debug.LogWarning($"Unknown Item Effect: {itemIndex}");
                break;
        }
    }
}
public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
