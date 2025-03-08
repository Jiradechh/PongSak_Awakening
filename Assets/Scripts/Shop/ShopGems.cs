using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems; 

public class ShopGems : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopUI;
    public Button increaseHPButton;
    public Button increaseDamageButton;

    [Header("Player Stats UI")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerDamageText;
    public TextMeshProUGUI playerSpeedText;

    [Header("Upgrade Settings")]
    public int upgradeCost = 4;
    public int hpIncreaseAmount = 25;
    public int damageIncreaseAmount = 25;

    private PlayerController player;
    private bool isShopOpen = false;
    private bool playerIsNear = false;
    private int currentIndex = 1; 
    private Button[] buttons;
    private EventSystem eventSystem;

    private void Start()
    {
        shopUI.SetActive(false);
        player = PlayerController.Instance;
        eventSystem = EventSystem.current;

        if (player == null)
        {
            return;
        }

        buttons = new Button[] { increaseHPButton, increaseDamageButton };

        UpdatePlayerStatsUI();

        increaseHPButton.onClick.AddListener(BuyIncreaseHP);
        increaseDamageButton.onClick.AddListener(BuyIncreaseDamage);

        UpdateButtonSelection();
    }

    private void Update()
    {
        if (playerIsNear && Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            OpenShop();
        }

        if (isShopOpen && Gamepad.current != null)
        {
            if (Gamepad.current.buttonEast.wasPressedThisFrame) 
            {
                CloseShop();
            }

            if (Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                UpdateButtonSelection();
            }
            else if (Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                currentIndex = Mathf.Min(buttons.Length - 1, currentIndex + 1);
                UpdateButtonSelection();
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame) 
            {
                if (currentIndex == 0) BuyIncreaseHP();
                else if (currentIndex == 1) BuyIncreaseDamage();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }

    public void OpenShop()
    {
        if (!playerIsNear) return;

        shopUI.SetActive(true);
        isShopOpen = true;
        Time.timeScale = 0f;
        UpdatePlayerStatsUI();
        UpdateButtonSelection();
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        isShopOpen = false;
        Time.timeScale = 1f;
    }

    private void UpdatePlayerStatsUI()
    {
        if (playerHPText != null)
            playerHPText.text = $"{player.maxHealth}";

        if (playerDamageText != null)
            playerDamageText.text = $"{player.lightAttackDamage}";

        if (playerSpeedText != null)
            playerSpeedText.text = $"{player.walkSpeed}";
    }

    private void BuyIncreaseHP()
    {
        if (GameManager.Instance.SpendGems(upgradeCost))
        {
            player.IncreaseMaxHP(hpIncreaseAmount);
            UpdatePlayerStatsUI();
        }
        
    }

    private void BuyIncreaseDamage()
    {
        if (GameManager.Instance.SpendGems(upgradeCost))
        {
            player.IncreaseLightAttackDamage(damageIncreaseAmount);
            UpdatePlayerStatsUI();
        }
        
    }

    private void UpdateButtonSelection()
    {
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(buttons[currentIndex].gameObject);

        for (int i = 0; i < buttons.Length; i++)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = (i == currentIndex) ? colors.selectedColor : colors.normalColor;
            buttons[i].colors = colors;
        }
    }
}
