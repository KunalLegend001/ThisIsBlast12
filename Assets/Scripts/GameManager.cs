using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum GameState
{
    Loading,
    Playing,
    Paused,
    Win
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private float loadingDuration = 5.5f;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;

    [Header("Win")]
    [SerializeField] private GameObject winPanel;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private int initialCubeCount;

    public GameState CurrentState { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        winPanel.SetActive(false);
        pausePanel.SetActive(false);

        StartCoroutine(LoadingRoutine());
    }

    IEnumerator LoadingRoutine()
    {
        CurrentState = GameState.Loading;
        loadingPanel.SetActive(true);

        float timer = 0f;
        loadingSlider.value = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            loadingSlider.value = timer / loadingDuration;
            yield return null;
        }

        loadingPanel.SetActive(false);
        StartGame();
    }

    void StartGame()
    {
        CurrentState = GameState.Playing;

        initialCubeCount = gridManager.GetTotalCubeCount();
        progressSlider.value = 0f;

        AudioManager.Instance.PlayBGM();
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        UpdateProgressBar();

        if (gridManager.GetTotalCubeCount() <= 0)
        {
            WinGame();
        }
    }

    // ==============================
    // PROGRESS BAR LOGIC
    // ==============================
    void UpdateProgressBar()
    {
        int remaining = gridManager.GetTotalCubeCount();

        float progress =
            1f - (remaining / (float)initialCubeCount);

        progressSlider.value = progress;
    }

    // ==============================
    // WIN
    // ==============================
    void WinGame()
    {
        CurrentState = GameState.Win;
        winPanel.SetActive(true);

        AudioManager.Instance.PlayWinSound();
        Time.timeScale = 1f;
    }

    // ==============================
    // PAUSE / RESUME
    // ==============================
    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Paused;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        AudioManager.Instance.PlayUIClick();
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        AudioManager.Instance.PlayUIClick();
    }

    // ==============================
    // RESTART
    // ==============================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.PlayUIClick();

        UnityEngine.SceneManagement.SceneManager
            .LoadScene(
                UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().buildIndex
            );
    }

    // ==============================
    // UI CLICK SOUND
    // ==============================
    public void OnUIButtonClick()
    {
        AudioManager.Instance.PlayUIClick();
    }
    public void QuitGame()
    { 
    
    Application.Quit();
    }
}
