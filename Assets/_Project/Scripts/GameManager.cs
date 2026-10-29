using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Owns the level flow, collectible count, and the four in-game UI states.</summary>
public sealed class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Paused, Dead, Won }
    private const string AutoStartRunKey = "Cloudstep.AutoStartRun";

    public static GameManager Instance { get; private set; }

    [Header("Level")]
    private int totalCoins;
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Coin tracker")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Image progressFill;
    [SerializeField] private RectTransform countPulseTarget;
    [SerializeField, Min(1f)] private float countPulseScale = 1.12f;
    [SerializeField, Min(0.01f)] private float countPulseDuration = 0.16f;
    [SerializeField, Min(0f)] private float progressFillSpeed = 1.4f;

    [Header("Screens")]
    [SerializeField] private CanvasGroup gameplayHud;
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup pauseMenu;
    [SerializeField] private CanvasGroup deathMenu;
    [SerializeField] private CanvasGroup winMenu;
    [SerializeField] private TMP_Text mainMenuTotalText;
    [SerializeField] private TMP_Text deathScoreText;
    [SerializeField] private TMP_Text winScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private Button hudPauseButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button deathMenuButton;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button winMenuButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button mainSettingsButton;
    [SerializeField] private AudioSettingsUI audioSettingsPanel;
    [SerializeField] private string nextSceneName = "Level03_NeonSpireway";

    private int score;
    private float targetProgress;
    private Vector3 pulseBaseScale = Vector3.one;
    private float pulseTime;
    private bool isPulsing;
    private bool startAfterSceneLoad;
    private GameState state;

    public int Score => score;
    public int TotalCoins => totalCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        totalCoins = CountLevelCoinPickups();
        startAfterSceneLoad = PlayerPrefs.GetInt(AutoStartRunKey, 0) == 1;
        PlayerPrefs.DeleteKey(AutoStartRunKey);
        if (startAfterSceneLoad) PlayerPrefs.Save();
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (cameraFollow == null && Camera.main != null) cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (countPulseTarget != null) pulseBaseScale = countPulseTarget.localScale;

        score = 0;
        targetProgress = 0f;
        state = GameState.MainMenu;
        Time.timeScale = 1f;
        Application.runInBackground = true;
        SetPlayerEnabled(false);
        SetLookEnabled(false);
        SetCursorForMenu();
        RefreshScoreDisplay();
        BindButtons();
        ShowState(GameState.MainMenu);
    }

    private void OnDestroy()
    {
        UnbindButtons();
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Start()
    {
        totalCoins = CountLevelCoinPickups();
        RefreshScoreDisplay();
        ShowState(state);
        PlaySceneAudio();
        if (startAfterSceneLoad)
        {
            startAfterSceneLoad = false;
            StartGame();
        }
    }

    private void PlaySceneAudio()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level03"))
        {
            AudioManager.Instance?.PlayBGM("BGM_Level03_Cosmic", 1.2f);
            AudioManager.Instance?.PlayAmbience("Ambience_CosmicDrone", 2.0f);
        }
        else if (sceneName.Contains("Level02"))
        {
            AudioManager.Instance?.PlayBGM("BGM_Level02_Sunset", 1.2f);
            AudioManager.Instance?.PlayAmbience("Ambience_OceanBreeze", 2.0f);
        }
        else
        {
            AudioManager.Instance?.PlayBGM("BGM_Level01_Daytime", 1.2f);
            AudioManager.Instance?.PlayAmbience("Ambience_MeadowWind", 2.0f);
        }
    }

    private void BindButtons()
    {
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(RestartRun);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (hudPauseButton != null) hudPauseButton.onClick.AddListener(PauseGame);
        if (retryButton != null) retryButton.onClick.AddListener(RestartRun);
        if (deathMenuButton != null) deathMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(RestartRun);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (winMenuButton != null) winMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (pauseSettingsButton != null) pauseSettingsButton.onClick.AddListener(OpenAudioSettings);
        if (mainSettingsButton != null) mainSettingsButton.onClick.AddListener(OpenAudioSettings);
    }

    private void UnbindButtons()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartGame);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
        if (pauseRestartButton != null) pauseRestartButton.onClick.RemoveListener(RestartRun);
        if (pauseMenuButton != null) pauseMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        if (hudPauseButton != null) hudPauseButton.onClick.RemoveListener(PauseGame);
        if (retryButton != null) retryButton.onClick.RemoveListener(RestartRun);
        if (deathMenuButton != null) deathMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        if (playAgainButton != null) playAgainButton.onClick.RemoveListener(RestartRun);
        if (nextLevelButton != null) nextLevelButton.onClick.RemoveListener(LoadNextLevel);
        if (winMenuButton != null) winMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        if (pauseSettingsButton != null) pauseSettingsButton.onClick.RemoveListener(OpenAudioSettings);
        if (mainSettingsButton != null) mainSettingsButton.onClick.RemoveListener(OpenAudioSettings);
    }

    public void OpenAudioSettings()
    {
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.OpenSettings();
        }
    }

    public void StartGame()
    {
        if (state != GameState.MainMenu) return;
        StartCoroutine(StartGameRoutine());
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        score = 0;
        if (totalCoins <= 0) totalCoins = CountLevelCoinPickups();
        targetProgress = 0f;
        RefreshScoreDisplay();

        RectTransform cardRect = mainMenu != null ? mainMenu.transform.Find("Card") as RectTransform : null;
        yield return FadeCanvasGroup(mainMenu, 1f, 0f, 0.25f, cardRect, 1f, 1.04f);

        state = GameState.Playing;
        Time.timeScale = 1f;
        SetPlayerEnabled(true);
        if (cameraFollow != null) cameraFollow.ResetOrientation();
        SetLookEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yield return FadeCanvasGroup(gameplayHud, 0f, 1f, 0.22f);
    }

    public void PauseGame()
    {
        if (state != GameState.Playing) return;

        state = GameState.Paused;
        AudioManager.Instance?.PlaySound("UI_Pause");
        SetPlayerEnabled(false);
        SetLookEnabled(false);
        SetCursorForMenu();
        StartCoroutine(FadeCanvasGroup(gameplayHud, 1f, 0f, 0.15f));

        RectTransform cardRect = pauseMenu != null ? pauseMenu.transform.Find("Card") as RectTransform : null;
        StartCoroutine(FadeCanvasGroup(pauseMenu, 0f, 1f, 0.22f, cardRect, 0.95f, 1f, () =>
        {
            Time.timeScale = 0f;
            if (resumeButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }));
    }

    public void ResumeGame()
    {
        if (state != GameState.Paused) return;
        AudioManager.Instance?.PlaySound("UI_Resume");
        StartCoroutine(ResumeGameRoutine());
    }

    private System.Collections.IEnumerator ResumeGameRoutine()
    {
        RectTransform cardRect = pauseMenu != null ? pauseMenu.transform.Find("Card") as RectTransform : null;
        yield return FadeCanvasGroup(pauseMenu, 1f, 0f, 0.18f, cardRect, 1f, 0.95f);

        state = GameState.Playing;
        Time.timeScale = 1f;
        SetPlayerEnabled(true);
        SetLookEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yield return FadeCanvasGroup(gameplayHud, 0f, 1f, 0.18f);
    }

    public void TogglePause()
    {
        if (state == GameState.Playing) PauseGame();
        else if (state == GameState.Paused) ResumeGame();
    }

    public void AddCoins(int amount)
    {
        if (state != GameState.Playing || amount <= 0) return;

        score = Mathf.Min(score + amount, totalCoins);
        RefreshScoreDisplay();
        PlayCountPulse();
    }

    public void PlayerDied(PlayerController dyingPlayer)
    {
        if (state != GameState.Playing || dyingPlayer == null) return;

        state = GameState.Dead;
        AudioManager.Instance?.PlaySound("SFX_Player_Death");
        StartCoroutine(PlayerDiedRoutine());
    }

    private System.Collections.IEnumerator PlayerDiedRoutine()
    {
        StartCoroutine(FadeCanvasGroup(gameplayHud, 1f, 0f, 0.25f));

        // Let the player tumble through the sky into the cloud void for 0.75s in realtime
        yield return new WaitForSecondsRealtime(0.75f);

        SetPlayerEnabled(false);
        SetLookEnabled(false);
        SetCursorForMenu();
        if (deathScoreText != null) deathScoreText.text = $"{score:00} / {totalCoins:00}";

        RectTransform cardRect = deathMenu != null ? deathMenu.transform.Find("Card") as RectTransform : null;
        yield return FadeCanvasGroup(deathMenu, 0f, 1f, 0.38f, cardRect, 0.92f, 1f);

        Time.timeScale = 0f;
        if (retryButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }

    public void LevelCompleted()
    {
        if (state != GameState.Playing) return;

        state = GameState.Won;
        AudioManager.Instance?.PlaySound("SFX_Level_Victory");
        StartCoroutine(LevelCompletedRoutine());
    }

    private System.Collections.IEnumerator LevelCompletedRoutine()
    {
        // Disable controls while standing victoriously before the Sun Portal
        SetPlayerEnabled(false);
        SetLookEnabled(false);
        StartCoroutine(FadeCanvasGroup(gameplayHud, 1f, 0f, 0.15f));

        // Short snappy 0.15s beat so the portal arrival registers immediately
        yield return new WaitForSecondsRealtime(0.15f);

        SetCursorForMenu();
        if (winScoreText != null) winScoreText.text = $"{score:00} / {totalCoins:00}";

        RectTransform cardRect = winMenu != null ? winMenu.transform.Find("Card") as RectTransform : null;
        yield return FadeCanvasGroup(winMenu, 0f, 1f, 0.22f, cardRect, 0.92f, 1f);

        Time.timeScale = 0f;
        if (playAgainButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration, RectTransform scaleTarget = null, float startScale = 1f, float endScale = 1f, System.Action onComplete = null)
    {
        if (group == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        group.gameObject.SetActive(true);
        group.blocksRaycasts = endAlpha > 0.5f;
        group.interactable = endAlpha > 0.5f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = Mathf.Sin(t * Mathf.PI * 0.5f);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, easeT);
            if (scaleTarget != null)
            {
                scaleTarget.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, easeT);
            }
            yield return null;
        }

        group.alpha = endAlpha;
        if (scaleTarget != null) scaleTarget.localScale = Vector3.one * endScale;
        if (endAlpha <= 0.001f)
        {
            group.gameObject.SetActive(false);
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        onComplete?.Invoke();
    }

    public void RestartLevel()
    {
        RestartRun();
    }

    public void RestartRun()
    {
        // Persist through a scene reload, including Editor configurations that reload the domain.
        PlayerPrefs.SetInt(AutoStartRunKey, 1);
        PlayerPrefs.Save();
        ReloadActiveScene();
    }

    private void ReloadActiveScene()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
            SceneManager.LoadScene(activeScene.buildIndex);
        else
            SceneManager.LoadScene(activeScene.name);
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.DeleteKey(AutoStartRunKey);
        if (SceneManager.GetActiveScene().name == "Level03_NeonSpireway")
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level01");
        }
        else
        {
            ReloadActiveScene();
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            PlayerPrefs.DeleteKey(AutoStartRunKey);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void Update()
    {
        if (state == GameState.Playing)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                PauseGame();
                return;
            }
        }
        else if (state == GameState.Paused)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                ResumeGame();
                return;
            }
        }
        else if ((state == GameState.Dead || state == GameState.Won) &&
            Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartRun();
            return;
        }

        if (progressFill != null)
            progressFill.fillAmount = Mathf.MoveTowards(progressFill.fillAmount, targetProgress,
                progressFillSpeed * Time.unscaledDeltaTime);

        if (isPulsing && countPulseTarget != null)
        {
            pulseTime += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(pulseTime / countPulseDuration);
            countPulseTarget.localScale = pulseBaseScale * Mathf.Lerp(1f, countPulseScale,
                Mathf.Sin(normalized * Mathf.PI));
            if (normalized >= 1f)
            {
                countPulseTarget.localScale = pulseBaseScale;
                isPulsing = false;
            }
        }
    }

    private void RefreshScoreDisplay()
    {
        if (scoreText != null) scoreText.text = score.ToString("00");
        if (totalText != null) totalText.text = $"/ {totalCoins:00}";
        targetProgress = totalCoins > 0 ? Mathf.Clamp01((float)score / totalCoins) : 0f;
        if (mainMenuTotalText != null) mainMenuTotalText.text = $"{totalCoins:00} COINS HIDDEN THROUGHOUT THE ISLES";
    }

    /// <summary>Counts enabled pickups in this level before any are collected, so HUD totals stay accurate.</summary>
    private int CountLevelCoinPickups()
    {
        CoinPickup[] pickups = FindObjectsByType<CoinPickup>(FindObjectsInactive.Exclude);
        int count = 0;
        for (int i = 0; i < pickups.Length; i++)
        {
            CoinPickup pickup = pickups[i];
            if (pickup != null && pickup.isActiveAndEnabled && pickup.gameObject.scene == gameObject.scene)
                count++;
        }

        return count;
    }

    private void PlayCountPulse()
    {
        if (countPulseTarget == null) return;
        isPulsing = true;
        pulseTime = 0f;
    }

    private void SetPlayerEnabled(bool enabled)
    {
        if (player != null) player.enabled = enabled;
    }

    private void SetLookEnabled(bool enabled)
    {
        if (cameraFollow != null) cameraFollow.SetLookInputEnabled(enabled);
    }

    private void SetCursorForMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowState(GameState visibleState)
    {
        // Keep the small counter visible during gameplay, and also on results if desired
        SetVisible(gameplayHud, visibleState == GameState.Playing);
        SetVisible(mainMenu, visibleState == GameState.MainMenu);
        SetVisible(pauseMenu, visibleState == GameState.Paused);
        SetVisible(deathMenu, visibleState == GameState.Dead);
        SetVisible(winMenu, visibleState == GameState.Won);

        Button firstButton = visibleState switch
        {
            GameState.MainMenu => startButton,
            GameState.Paused => resumeButton,
            GameState.Dead => retryButton,
            GameState.Won => playAgainButton,
            _ => null
        };
        if (firstButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    private static void SetVisible(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.gameObject.SetActive(visible);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private void OnValidate()
    {
        countPulseScale = Mathf.Max(1f, countPulseScale);
        countPulseDuration = Mathf.Max(0.01f, countPulseDuration);
        progressFillSpeed = Mathf.Max(0f, progressFillSpeed);
    }
}
