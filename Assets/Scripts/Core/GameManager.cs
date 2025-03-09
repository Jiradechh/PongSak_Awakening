using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    [Header("Currency Settings")]
    public int gold = 0;
    public int gems = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemsText;
    public CanvasGroup currencyUIGroup;

    [Header("Stage and Map Settings")]
    public string[] allStages;
    public string shopScene = "ShopMap1";
    public string bossScene = "BossMap1";
    public string lobbyScene = "Lobby";

    private int currentStage = 0;
    private List<string> stageQueue;
    private bool gameInProgress = false;
    public bool GameInProgress => gameInProgress;

    [Header("Respawn Settings")]
    public GameObject playerPrefab;
    private GameObject currentPlayer;
    private Transform spawnPoint;

    [Header("Warp Gate & Treasure Settings")]
    public GameObject warpGatePrefab;
    private Transform warpGateSpawnPoint;
    private bool treasureSpawned = false;
    public GameObject treasurePrefab;
    private Transform treasureSpawnPoint;

    private Coroutine fadeCoroutine;

            [Header("Audio Settings")]
        public AudioClip lobbyMusic;
        public AudioClip stageMusic;
        public AudioClip bossMusic;
        private AudioSource audioSource;

         private static bool instanceExists = false; 

    private void Awake()
    {
        if (instanceExists)
        {
            Destroy(gameObject);
            return;
        }

        instanceExists = true;
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        UpdateCurrencyUI();
        FindSpawnPoint();
        FindWarpGatePoint();
        FindTreasureSpawnPoint();
        InvokeRepeating(nameof(CheckAndSpawnWarpGate), 5f, 5f);

        if (currencyUIGroup != null) currencyUIGroup.alpha = 0f;

         /* if (SaveManager.Instance.onContinue)
        {
            //Load Savegems
            gems = SaveManager.Instance.saveData.gems;
        }*/
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        PlayMusicForCurrentScene();
    }

    #region Currency Methods
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateCurrencyUI();
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateCurrencyUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI();
            return true;
        }
        return false;
    }

    public bool SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            UpdateCurrencyUI();
            return true;
        }
        return false;
    }

    private void UpdateCurrencyUI()
    {
        if (goldText != null) goldText.text = $"{gold}";
        if (gemsText != null) gemsText.text = $"{gems}";

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCurrencyUI());
    }

    private IEnumerator FadeCurrencyUI()
    {
        if (currencyUIGroup != null)
        {
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currencyUIGroup.alpha = Mathf.Lerp(currencyUIGroup.alpha, 1f, elapsed / duration);
                yield return null;
            }

            currencyUIGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(2f);

        if (currencyUIGroup != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currencyUIGroup.alpha = Mathf.Lerp(currencyUIGroup.alpha, 0f, elapsed / duration);
                yield return null;
            }

            currencyUIGroup.alpha = 0f;
        }
    }
    #endregion

    #region Game Flow Methods
    public void StartGame()
    {
        currentStage = 0;
        stageQueue = GenerateStageQueue();
        gameInProgress = true;
        LoadNextStage();
    }

    public void LoadNextStage()
{
    if (!gameInProgress) return;

    currentStage++;
    string sceneToLoad;

    if (currentStage <= 4)
    {
        sceneToLoad = stageQueue[currentStage - 1];
    }
    else if (currentStage == 5)
    {
        sceneToLoad = shopScene;
    }
    else if (currentStage == 6)
    {
        sceneToLoad = bossScene;
    }
    else
    {
        LoadLobbyAndRestart(); 
        return;
    }

    SceneManager.LoadScene(sceneToLoad);
    Invoke(nameof(PlayMusicForCurrentScene), 1f);

    Invoke(nameof(FindSpawnPoint), 0.5f);
    Invoke(nameof(FindWarpGatePoint), 0.5f);
    Invoke(nameof(FindTreasureSpawnPoint), 0.5f);
    Invoke(nameof(RespawnPlayer), 1.0f);

    treasureSpawned = false;
}

    private void LoadLobbyAndRestart()
    {
        gameInProgress = false;
        currentStage = 0;
        stageQueue = GenerateStageQueue();

        SceneManager.LoadScene(lobbyScene);
        Invoke(nameof(PlayMusicForCurrentScene), 1f);

        Invoke(nameof(FindSpawnPoint), 0.5f);
        Invoke(nameof(FindWarpGatePoint), 0.5f);
        Invoke(nameof(FindTreasureSpawnPoint), 0.5f);
        Invoke(nameof(RespawnPlayer), 1.0f);
    }

    public void CheckAndSpawnWarpGate()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            if (GameObject.FindWithTag("WarpGate") == null && warpGateSpawnPoint != null)
            {
                Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
                Instantiate(warpGatePrefab, warpGateSpawnPoint.position, rotation);
            }

            if (!treasureSpawned && treasureSpawnPoint != null)
            {
                Instantiate(treasurePrefab, treasureSpawnPoint.position, Quaternion.identity);
                treasureSpawned = true;
            }
        }
    }

    private List<string> GenerateStageQueue()
    {
        List<string> shuffledStages = new List<string>(allStages);
        for (int i = 0; i < shuffledStages.Count; i++)
        {
            int randomIndex = Random.Range(0, shuffledStages.Count);
            (shuffledStages[i], shuffledStages[randomIndex]) = (shuffledStages[randomIndex], shuffledStages[i]);
        }

        return shuffledStages.GetRange(0, 4);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindSpawnPoint();
            FindWarpGatePoint();
            FindTreasureSpawnPoint();
            RespawnPlayer();
        }
    public void LoadLobby()
    {
        gameInProgress = false;
        currentStage = 0;
        SceneManager.LoadScene(lobbyScene);

        Invoke(nameof(FindSpawnPoint), 0.5f);
        Invoke(nameof(FindWarpGatePoint), 0.5f);
        Invoke(nameof(FindTreasureSpawnPoint), 0.5f);
        Invoke(nameof(RespawnPlayer), 1.0f);
    }
    #endregion

    #region Respawn & Death Handling
public void RespawnPlayer()
{
    if (playerPrefab == null || spawnPoint == null) return;

    currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    Debug.Log($"✅ Player Respawned at {spawnPoint.position}");

    PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
    if (playerController != null)
    {
        playerController.ResetPlayerState();
        playerController.EnablePlayerActions(); 
    }
}


private void FindSpawnPoint()
{
    GameObject spawnObject = GameObject.Find("SpawnPoints");
    if (spawnObject != null)
    {
        spawnPoint = spawnObject.transform;
    }
   
}


    private void FindWarpGatePoint()
    {
        GameObject warpGatePointObject = GameObject.Find("WarpGatePoints");
        if (warpGatePointObject != null)
        {
            warpGateSpawnPoint = warpGatePointObject.transform;
        }
    }

    private void FindTreasureSpawnPoint()
    {
        GameObject treasurePointObject = GameObject.Find("TreasureSpawnPoint");
        if (treasurePointObject != null)
        {
            treasureSpawnPoint = treasurePointObject.transform;
        }
    }
    #endregion

    #region Game Pause Methods
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    #endregion



public void PlayerDied()
{
    StartCoroutine(HandlePlayerDeath());
}

private IEnumerator HandlePlayerDeath()
{
    yield return new WaitForSeconds(1f);

    if (currentPlayer != null)
    {
        Destroy(currentPlayer);
        currentPlayer = null;
    }

    gold = 0;  
    UpdateCurrencyUI();

    SceneManager.LoadScene(lobbyScene);

    yield return new WaitForSeconds(0.5f); 

    FindSpawnPoint();

    RespawnPlayer();
}





     private void PlayMusicForCurrentScene()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = null;

        if (SceneManager.GetActiveScene().name == lobbyScene)
        {
            clipToPlay = lobbyMusic;
        }
        else if (SceneManager.GetActiveScene().name == bossScene)
        {
            clipToPlay = bossMusic;
        }
        else
        {
            clipToPlay = stageMusic;
        }

        if (clipToPlay != null && audioSource.clip != clipToPlay)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
}
