using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject normalMode;
    [SerializeField] private GameObject ghostMode;
    [SerializeField] private GameObject shootingMode;
    [SerializeField] private GameObject gameOverMode;
    [SerializeField] private GameObject pauseMode;
    [SerializeField] private GameObject winMode;
    [SerializeField] private GameObject ghostGlasses;
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject energyCell1;
    [SerializeField] private GameObject energyCell2;
    [SerializeField] private GameObject energyCell3;

    [Header("Enemy Parents")]
    [SerializeField] private GameObject zombieParent;
    [SerializeField] private GameObject ghostParent;

    [Header("Boss Enemy")]
    [SerializeField] private GameObject bossEnemy;
    [SerializeField] private GameObject bossEnemy1;
    [SerializeField] private GameObject bossEnemy2;


    [Header("Game Settings")]
    public static int masterKey = 0;
    public static int energyCells = 0;
    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isInNormalMode = false;
    private bool isInShootingMode = false;
    public static bool isEndDoor = false;

    [Header("Ghost Mode Settings")]
    private float ghostModeDuration = 60f;
    private float ghostModeCoolDown = 30f;
    private float lastGhostModeTime;
    public static bool hasGhostGlasses = false;
    public static bool isInGhostMode = false;
    public static bool hasGhostGun = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void GhostMode()
    {
        if ( hasGhostGlasses && !isInGhostMode && isInNormalMode && Time.time - lastGhostModeTime >= ghostModeCoolDown)
        {
            isInGhostMode = true;
            isInNormalMode = false;
            normalMode.SetActive(false);
            ghostMode.SetActive(true);
            shootingMode.SetActive(false);
            UpdateEnemyVisibility();
            PlayerManager.Instance.SwitchToGhostPlayer();            
            Invoke("ExitGhostMode", ghostModeDuration);
        }
    }

    public void ExitGhostMode()
    {
        lastGhostModeTime = Time.time;
        isInGhostMode = false;
        UpdateEnemyVisibility();
        NormalMode();
    }

    public void ShootingMode()
    {
        isInNormalMode = false;
        isInGhostMode = false;
        isInShootingMode = true;
        normalMode.SetActive(false);
        ghostMode.SetActive(false);
        shootingMode.SetActive(true);
        CameraManager.Instance.SwitchToFirstPerson();
        UpdateEnemyVisibility();

    }

    public void NormalMode()
    {
        isInGhostMode = false;
        isInShootingMode = false;
        isInNormalMode = true;
        normalMode.SetActive(true);
        ghostMode.SetActive(false);
        shootingMode.SetActive(false);
        PlayerManager.Instance.SwitchToNormalPlayer();
        CameraManager.Instance.SwitchToThirdPerson();
        UpdateEnemyVisibility();
    }

    private void UpdateEnemyVisibility()
    {
        if (isInGhostMode)
        {
            ghostParent.SetActive(true);
            zombieParent.SetActive(false);
        }
        else
        {
            ghostParent.SetActive(false);
            zombieParent.SetActive(true);
        }
    }


    public void GameOver()
    {
        isGameOver = true;
        gameOverMode.SetActive(true);
        normalMode.SetActive(false);
        ghostMode.SetActive(false);
        shootingMode.SetActive(false);
        pauseMode.SetActive(false);
        Time.timeScale = 0f; // Freeze game time
    }

    public void WinGame()
    {
        winMode.SetActive(true);
        normalMode.SetActive(false);
        ghostMode.SetActive(false);
        shootingMode.SetActive(false);
        pauseMode.SetActive(false);
        Time.timeScale = 0f; // Freeze game time
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed && isPaused) 
        { 
            ResumeGame();
        }
        else
        {
            PauseGame();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnGhostMode(InputValue value)
    {

        Debug.Log("Ghost Mode input received");
        Debug.Log("isInGhostMode: " + isInGhostMode);
        Debug.Log("Time since lastGhostModeTime: " + (Time.time - lastGhostModeTime));
        Debug.Log("ghostModeCoolDown: " + ghostModeCoolDown);
        Debug.Log("isInShootingMode: " + isInShootingMode);

        if (value.isPressed && Time.time - lastGhostModeTime >= ghostModeCoolDown && !isInGhostMode && !isInShootingMode)
        {
            Debug.Log("Entering Ghost Mode");
            GhostMode();
        }
        else if (value.isPressed && isInGhostMode)
        {
            Debug.Log("Exiting Ghost Mode");
            ExitGhostMode();
        }
    }

    public void RestartGame()
    {
        PlayerManager.Instance.ResetHealth();
        energyCells = 0;
        masterKey = 0;
        NormalMode();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMode.SetActive(true);
        normalMode.SetActive(false);
        ghostMode.SetActive(false);
        shootingMode.SetActive(false);
        Time.timeScale = 0f; // Freeze game time
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMode.SetActive(false);
        if (isInGhostMode)
        {
            ghostMode.SetActive(true);
        }
        else if (isInShootingMode)
        {
            shootingMode.SetActive(true);
        }
        else
        {
            normalMode.SetActive(true);
        }
            
        Time.timeScale = 1f; // Resume game time
    }

    private IEnumerator WaitAndDisableGhosts()
    {
        yield return new WaitForSeconds(1);
        NormalMode();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && energyCells == 0 && masterKey == 1 && isEndDoor)
        {
            Debug.Log("Interact input received in GameManager");
            WinGame();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitAndDisableGhosts());

        energyCell1.SetActive(false);
        energyCell2.SetActive(false);
        energyCell3.SetActive(false);

        key.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {

        if (energyCells == 1)
        {
            energyCell1.SetActive(true);
            energyCell2.SetActive(false);
            energyCell3.SetActive(false);
        }
        else if (energyCells == 2) 
        { 
            energyCell1.SetActive(true);
            energyCell2.SetActive(true);
            energyCell3.SetActive(false);
        }
        else if (energyCells == 3)
        {
            energyCell1.SetActive(true);
            energyCell2.SetActive(true);
            energyCell3.SetActive(true);
        }
        else
        {
            energyCell1.SetActive(false);
            energyCell2.SetActive(false);
            energyCell3.SetActive(false);
        }

        if (masterKey == 1)
        {
            key.SetActive(true);
        }
        else
        {
            key.SetActive(false);
        }


    }
}
