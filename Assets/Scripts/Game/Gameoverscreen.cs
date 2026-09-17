using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text wavesCompletedText;
    public Button returnToMenuButton;

    [Header("Scenes")]
    public string menuSceneName = "MainMenu";

    [Header("Options")]
    public bool pauseOnGameOver = true;

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (playerState == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerState = p.GetComponent<PlayerState>();
        }

        if (playerState != null)
            playerState.OnDeath += HandleDeath;

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);
    }

    void OnDestroy()
    {
        if (playerState != null)
            playerState.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        int wavesCompleted = 0;
        if (WaveManager.Instance != null)
            wavesCompleted = Mathf.Max(0, WaveManager.Instance.CurrentWave - 1);

        if (wavesCompletedText != null)
            wavesCompletedText.text = $"Waves Completed: {wavesCompleted}";

        if (panel != null) panel.SetActive(true);
        if (pauseOnGameOver) Time.timeScale = 0f;

        if (UICursor.Instance != null)
            UICursor.Instance.SetCustomCursor(false);
    }

    public void OnReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}