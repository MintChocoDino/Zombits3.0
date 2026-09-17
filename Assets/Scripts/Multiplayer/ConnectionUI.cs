using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    [Header("UI")]
    public Button hostButton;
    public Button clientButton;
    public Button backButton;
    public TMP_InputField addressInput;
    public GameObject connectionPanel;
    public TMP_Text statusText;

    [Header("Scenes")]
    public string menuSceneName = "MainMenu";

    void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnHost);
        if (clientButton != null) clientButton.onClick.AddListener(OnClient);
        if (backButton != null) backButton.onClick.AddListener(OnBack);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        SetStatus("Host or join a game.");
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void OnHost()
    {
        SetAddress("127.0.0.1");
        NetworkManager.Singleton.StartHost();
        SetStatus("Hosting. Waiting for players...");
        if (connectionPanel != null) connectionPanel.SetActive(false);
    }

    private void OnClient()
    {
        string addr = addressInput != null && !string.IsNullOrEmpty(addressInput.text)
            ? addressInput.text
            : "127.0.0.1";
        SetAddress(addr);
        NetworkManager.Singleton.StartClient();
        SetStatus($"Connecting to {addr}...");
    }

    private void SetAddress(string address)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData(address, 7777);
    }

    private void HandleClientConnected(ulong clientId)
    {
        SetStatus($"Connected! (clients: {NetworkManager.Singleton.ConnectedClientsIds.Count})");
        if (connectionPanel != null) connectionPanel.SetActive(false);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        SetStatus("Disconnected.");
        if (connectionPanel != null) connectionPanel.SetActive(true);
    }

    private void OnBack()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(menuSceneName);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}