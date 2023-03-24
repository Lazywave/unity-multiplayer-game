using System.Text.RegularExpressions;
using SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField] private TMP_Text hostIpInput;

    [SerializeField] private string lobbySceneName;

    public void StartLocalGameAsHost()
    {
        StartLocalGame(true);
    }
    
    public void StartLocalGameAsServer()
    {
        StartLocalGame(false);
    }
    
    
    private void StartLocalGame(bool asHost)
    {
        // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport; // TODO: Adapt to Websocket transport 
        if (utpTransport) hostIpInput.text = "127.0.0.1";
        if (asHost && NetworkManager.Singleton.StartHost())
        {
            SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
            SceneTransitionHandler.SwitchScene(lobbySceneName);
        }
        else if (NetworkManager.Singleton.StartServer())
        {
            SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
            SceneTransitionHandler.SwitchScene(lobbySceneName);
        }
        else
        {
            Debug.LogError("Failed to start server.");
        }
    }

    private void JoinLocalGame()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport)
        {
            utpTransport.SetConnectionData(Sanitize(hostIpInput.text), 7777);
        }
        if (!NetworkManager.Singleton.StartClient())
        {
            Debug.LogError("Failed to start client.");
        }
    }
    
    public static string Sanitize(string dirtyString)
    {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
    }
}
