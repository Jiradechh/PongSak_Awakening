using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [Header("Main Menu Elements")]
    public Button continueButton;
    public Button startButton;
    public Button creditsButton; 
    public Button exitButton;
    public GameObject creditsCanvas;

    private List<Button> menuButtons;
    private int selectedButtonIndex = 0;

    private float inputDelay = 0.3f;
    private float lastInputTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bool hasSaveGame = PlayerPrefs.GetInt("MaxHealth", 0) > 0;
        SetContinueButtonState(hasSaveGame);

        continueButton.onClick.AddListener(ContinueGame);
        startButton.onClick.AddListener(() => LoadScene("Lobby"));
        creditsButton.onClick.AddListener(OpenCredits); 
        exitButton.onClick.AddListener(ExitGame);

        creditsCanvas.SetActive(false);

        menuButtons = new List<Button> { continueButton, startButton, creditsButton, exitButton };
        HighlightButton(selectedButtonIndex);
    }

    private void Update()
    {
        if (Time.time - lastInputTime > inputDelay)
        {
            HandleMenuNavigation();
        }

        if (creditsCanvas.activeSelf && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Joystick1Button1)))
        {
            CloseCredits();
        }
    }

    public void SetContinueButtonState(bool isActive)
    {
        if (continueButton == null)
        {
            Debug.LogError("Continue Button is not assigned in MainMenu!");
            return;
        }

        continueButton.gameObject.SetActive(isActive);
    }

    private void HandleMenuNavigation()
    {
        if (creditsCanvas.activeSelf) return; 

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetAxis("Vertical") > 0.5f)
        {
            Navigate(-1);
            lastInputTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetAxis("Vertical") < -0.5f)
        {
            Navigate(1);
            lastInputTime = Time.time;
        }

        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
        {
            var selectedButton = menuButtons[selectedButtonIndex];
            if (selectedButton.gameObject.activeSelf)
            {
                selectedButton.onClick.Invoke();
                lastInputTime = Time.time;
            }
        }
    }

    private void Navigate(int direction)
    {
        selectedButtonIndex = (selectedButtonIndex + direction + menuButtons.Count) % menuButtons.Count;
        HighlightButton(selectedButtonIndex);
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            var button = menuButtons[i];
            var buttonImage = button.GetComponent<Image>();

            if (buttonImage == null) continue;

            if (i == index)
            {
                buttonImage.sprite = button.spriteState.selectedSprite;
            }
            else
            {
                buttonImage.sprite = button.spriteState.highlightedSprite;
            }
        }
    }

    private void OpenCredits()
    {
        creditsCanvas.SetActive(true);
    }

    private void CloseCredits()
    {
        creditsCanvas.SetActive(false);
    }

    private void ContinueGame()
    {
        SaveManager.Instance.onContinue = true;
        SceneManager.LoadScene("Lobby");
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exiting game...");
    }
}
