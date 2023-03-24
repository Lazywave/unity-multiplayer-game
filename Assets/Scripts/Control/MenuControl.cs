using System.Text.RegularExpressions;
using SceneShit;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField]
    private TMP_Text mHostIpInput;

    [SerializeField]
    private string mLobbySceneName = "InvadersLobby";

    public void StartLocalGame()
    {
        // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport; // TODO: Adapt to Websocket transport 
        if (utpTransport) mHostIpInput.text = "127.0.0.1";
        if (NetworkManager.Singleton.StartHost())
        {
            SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene(mLobbySceneName);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinLocalGame()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport)
        {
            utpTransport.SetConnectionData(Sanitize(mHostIpInput.text), 7777);
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
