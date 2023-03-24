using System.Collections.Generic;
using SceneShit;
using Unity.Netcode;
using UnityEngine;

public class LobbyControl : NetworkBehaviour
{
    [SerializeField]
    private string mInGameSceneName = "InGame";
    
    // Minimum player count required to transition to next level
    [SerializeField]
    private int mMinimumPlayerCount = 2;
    
    private bool _mAllPlayersInLobby;

    private Dictionary<ulong, bool> _mClientsInLobby;

    public override void OnNetworkSpawn()
    {
        _mClientsInLobby = new Dictionary<ulong, bool> {
            //Always add ourselves to the list at first
            { NetworkManager.LocalClientId, false } };

        //If we are hosting, then handle the server side for detecting when clients have connected
        //and when their lobby scenes are finished loading.
        if (IsServer)
        {
            _mAllPlayersInLobby = false;

            //Server will be notified when a client connects
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += ClientLoadedScene;
        }

        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
    }

    /// <summary>
    ///     UpdateAndCheckPlayersInLobby
    ///     Checks to see if we have at least 2 or more people to start
    /// </summary>
    private void UpdateAndCheckPlayersInLobby()
    {
        _mAllPlayersInLobby = _mClientsInLobby.Count >= mMinimumPlayerCount;

        foreach (var clientLobbyStatus in _mClientsInLobby)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))

                //If some clients are still loading into the lobby scene then this is false
                _mAllPlayersInLobby = false;
        }

        CheckForAllPlayersReady();
    }

    /// <summary>
    ///     ClientLoadedScene
    ///     Invoked when a client has loaded this scene
    /// </summary>
    /// <param name="clientId"></param>
    private void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (!_mClientsInLobby.ContainsKey(clientId))
            {
                _mClientsInLobby.Add(clientId, false);
            }

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     OnClientConnectedCallback
    ///     Since we are entering a lobby and Netcode's NetworkManager is spawning the player,
    ///     the server can be configured to only listen for connected clients at this stage.
    /// </summary>
    /// <param name="clientId">client that connected</param>
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!_mClientsInLobby.ContainsKey(clientId)) _mClientsInLobby.Add(clientId, false);

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     SendClientReadyStatusUpdatesClientRpc
    ///     Sent from the server to the client when a player's status is updated.
    ///     This also populates the connected clients' (excluding host) player state in the lobby
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="isReady"></param>
    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        if (!IsServer)
        {
            if (!_mClientsInLobby.ContainsKey(clientId))
                _mClientsInLobby.Add(clientId, isReady);
            else
                _mClientsInLobby[clientId] = isReady;
        }
    }

    /// <summary>
    ///     CheckForAllPlayersReady
    ///     Checks to see if all players are ready, and if so launches the game
    /// </summary>
    private void CheckForAllPlayersReady()
    {
        if (_mAllPlayersInLobby)
        {
            var allPlayersAreReady = true;
            foreach (var clientLobbyStatus in _mClientsInLobby)
                if (!clientLobbyStatus.Value)

                    //If some clients are still loading into the lobby scene then this is false
                    allPlayersAreReady = false;

            //Only if all players are ready
            if (allPlayersAreReady)
            {
                //Remove our client connected callback
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

                //Remove our scene loaded callback
                SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene -= ClientLoadedScene;

                //Transition to the ingame scene
                SceneTransitionHandler.sceneTransitionHandler.SwitchScene(mInGameSceneName);
            }
        }
    }

    /// <summary>
    ///     PlayerIsReady
    ///     Tied to the Ready button in the InvadersLobby scene
    /// </summary>
    public void PlayerIsReady()
    {
        _mClientsInLobby[NetworkManager.Singleton.LocalClientId] = true;
        if (IsServer)
        {
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    /// <summary>
    ///     OnClientIsReadyServerRpc
    ///     Sent to the server when the player clicks the ready button
    /// </summary>
    /// <param name="clientID">clientId that is ready</param>
    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientID)
    {
        if (_mClientsInLobby.ContainsKey(clientID))
        {
            _mClientsInLobby[clientID] = true;
            UpdateAndCheckPlayersInLobby();
        }
    }
}
