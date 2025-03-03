using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Warp Gate Settings")]
    public GameObject warpGatePrefab;
    private Transform warpGateSpawnPoint;

    private void Start()
    {
        UpdateCurrencyUI();
        FindSpawnPoint();
        FindWarpGatePoint();
        InvokeRepeating(nameof(CheckAndSpawnWarpGate), 5f, 5f);
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
        if (!gameInProgress)
        {
            return;
        }

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
            LoadLobby();
            return;
        }

        SceneManager.LoadScene(sceneToLoad);

        Invoke(nameof(FindSpawnPoint), 0.5f);
        Invoke(nameof(FindWarpGatePoint), 0.5f);
        Invoke(nameof(RespawnPlayer), 1.0f);
    }

    public void CheckAndSpawnWarpGate()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            if (GameObject.FindWithTag("WarpGate") == null)
            {
                if (warpGateSpawnPoint != null)
                {
                    Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
                    Instantiate(warpGatePrefab, warpGateSpawnPoint.position, rotation);
                }
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

    public void LoadLobby()
    {
        gameInProgress = false;
        currentStage = 0;
        SceneManager.LoadScene(lobbyScene);

        Invoke(nameof(FindSpawnPoint), 0.5f);
        Invoke(nameof(FindWarpGatePoint), 0.5f);
        Invoke(nameof(RespawnPlayer), 1.0f);
    }
    #endregion

    #region Respawn & Death Handling
    public void RespawnPlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        if (spawnPoint == null)
        {
            return;
        }

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void FindSpawnPoint()
    {
        GameObject spawnObject = GameObject.Find("SpawnPoints");
        if (spawnObject != null)
        {
            spawnPoint = spawnObject.transform;
        }
        else
        {
            spawnPoint = null;
        }
    }

    private void FindWarpGatePoint()
    {
        GameObject warpGatePointObject = GameObject.Find("WarpGatePoints");
        if (warpGatePointObject != null)
        {
            warpGateSpawnPoint = warpGatePointObject.transform;
        }
        else
        {
            warpGateSpawnPoint = null;
        }
    }

    public void PlayerDied()
    {
        LoadLobby();
    }
    #endregion
}
