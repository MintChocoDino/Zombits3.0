using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Main Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button quitToMenuButton;

    [Header("Settings Controls")]
    public Slider volumeSlider;
    public TMP_Text volumeLabel;
    public Button fullscreenToggleButton;
    public TMP_Text fullscreenButtonLabel;
    public Button settingsBackButton;

    [Header("Scenes")]
    public string menuSceneName = "MainMenu";

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(QuitToMenu);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
        if (fullscreenToggleButton != null) fullscreenToggleButton.onClick.AddListener(ToggleFullscreen);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = PlayerPrefs.GetFloat("master_volume", 1f);
            ApplyVolume(volumeSlider.value);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        UpdateFullscreenLabel();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                CloseSettings();
            else if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);

        if (UICursor.Instance != null)
            UICursor.Instance.SetCustomCursor(false);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (UICursor.Instance != null)
            UICursor.Instance.SetCustomCursor(true);
    }

    private void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("master_volume", value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.masterVolume = value;
        if (volumeLabel != null)
            volumeLabel.text = $"Volume: {Mathf.RoundToInt(value * 100)}%";
    }

    private void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateFullscreenLabel();
    }

    private void UpdateFullscreenLabel()
    {
        if (fullscreenButtonLabel != null)
            fullscreenButtonLabel.text = Screen.fullScreen ? "Fullscreen: ON" : "Fullscreen: OFF";
    }
}