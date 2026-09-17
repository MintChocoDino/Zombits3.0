using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private const string HighestWaveKey = "highest_wave";

    [Header("Scenes")]
    public string singleplayerSceneName = "Game";
    public string multiplayerSceneName = "";
    public string settingsSceneName = "";

    [Header("UI References")]
    public Button singleplayerButton;
    public Button multiplayerButton;
    public Button settingsButton;
    public Button quitButton;
    public TMP_Text highestWaveText;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    void Start()
    {
        RefreshHighestWave();

        if (singleplayerButton != null)
            singleplayerButton.onClick.AddListener(OnSingleplayer);
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(OnMultiplayer);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void RefreshHighestWave()
    {
        int highest = PlayerPrefs.GetInt(HighestWaveKey, 0);
        if (highestWaveText != null)
            highestWaveText.text = highest > 0 ? $"Highest Wave: {highest}" : "Highest Wave: —";
    }

    public void OnSingleplayer()
    {
        if (!string.IsNullOrEmpty(singleplayerSceneName))
            SceneManager.LoadScene(singleplayerSceneName);
    }

    public void OnMultiplayer()
    {
        if (!string.IsNullOrEmpty(multiplayerSceneName))
            SceneManager.LoadScene(multiplayerSceneName);
    }

    public void OnSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        else if (!string.IsNullOrEmpty(settingsSceneName))
            SceneManager.LoadScene(settingsSceneName);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void RecordHighestWave(int wave)
    {
        int current = PlayerPrefs.GetInt(HighestWaveKey, 0);
        if (wave > current)
        {
            PlayerPrefs.SetInt(HighestWaveKey, wave);
            PlayerPrefs.Save();
        }
    }

    public static int GetHighestWave() => PlayerPrefs.GetInt(HighestWaveKey, 0);
}