using System;
using SceneShit;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public enum GameOverReason : byte
{
    None = 0,
    EnemiesReachedBottom = 1,
    Death = 2,
    Max,
}

public class InvadersGame : NetworkBehaviour
{
    // The vertical offset we apply to each Enemy transform once they touch an edge
    private const float KEnemyVerticalMovementOffset = -0.8f;
    private const float KLeftOrRightBoundaryOffset = 10.0f;
    private const float KBottomBoundaryOffset = 1.25f;

    [Header("Prefab settings")]
    public GameObject enemy1Prefab;
    public GameObject enemy2Prefab;
    public GameObject enemy3Prefab;
    public GameObject superEnemyPrefab;
    public GameObject shieldPrefab;

    [Header("UI Settings")]
    public TMP_Text gameTimerText;
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text gameOverText;

    [Header("GameMode Settings")]
    public Transform saucerSpawnPoint;

    [SerializeField]
    [Tooltip("Time Remaining until the game starts")]
    private float mDelayedStartTime = 5.0f;


    //These help to simplify checking server vs client
    //[NSS]: This would also be a great place to add a state machine and use networked vars for this
    private bool _mClientGameOver;
    private bool _mClientGameStarted;
    private bool _mClientStartCountdown;

    private readonly NetworkVariable<bool> _mCountdownStarted = new();

    private float _mNextTick;

    // the timer should only be synced at the beginning
    // and then let the client to update it in a predictive manner
    private bool _mReplicatedTimeSent;
    private GameObject _mSaucer;

    public static InvadersGame Singleton { get; private set; }

    public NetworkVariable<bool> HasTheGameStarted { get; } = new();

    public NetworkVariable<bool> IsGameOver { get; } = new();

    /// <summary>
    ///     Awake
    ///     A good time to initialize server side values
    /// </summary>
    private void Awake()
    {
        Assert.IsNull(Singleton, $"Multiple instances of {nameof(InvadersGame)} detected. This should not happen.");
        Singleton = this;

        OnSingletonReady?.Invoke();

        if (!IsServer) return;
        
        HasTheGameStarted.Value = false;

        //Set for server side
        _mReplicatedTimeSent = false;
    }

    /// <summary>
    ///     Update
    ///     MonoBehaviour Update method
    /// </summary>
    /* private void Update()
    {
        //Is the game over?
        if (IsCurrentGameOver()) return;

        //Update game timer (if the game hasn't started)
        UpdateGameTimer();

        //If we are a connected client, then don't update the enemies (server side only)
        if (!IsServer) return;

        //If we are the server and the game has started, then update the enemies
        if (HasGameStarted()) UpdateEnemies();
    }
    */

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            // Clear enemies
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    internal static event Action OnSingletonReady;

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            _mClientGameOver = false;
            _mClientStartCountdown = false;
            _mClientGameStarted = false;

            _mCountdownStarted.OnValueChanged += (_, newValue) =>
            {
                _mClientStartCountdown = newValue;
                Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            };

            HasTheGameStarted.OnValueChanged += (_, newValue) =>
            {
                _mClientGameStarted = newValue;
                gameTimerText.gameObject.SetActive(!_mClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            IsGameOver.OnValueChanged += (_, newValue) =>
            {
                _mClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        //Both client and host/server will set the scene state to "in-game" which places the PlayerControl into the SceneTransitionHandler.SceneStates.INGAME
        //and in turn makes the players visible and allows for the players to be controlled.
        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Ingame);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        base.OnNetworkSpawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_mReplicatedTimeSent)
        {
            // Send the RPC only to the newly connected client
            // SetReplicatedTimeRemainingClientRPC(_mTimeRemaining, new ClientRpcParams {Send = new ClientRpcSendParams{TargetClientIds = new List<ulong> {clientId}}});
        }
    }

    /// <summary>
    ///     ShouldStartCountDown
    ///     Determines when the countdown should start
    /// </summary>
    /// <returns>true or false</returns>
    private bool ShouldStartCountDown()
    {
        //If the game has started, then don't bother with the rest of the count down checks.
        if (HasGameStarted()) return false;
        if (IsServer)
        {
            _mCountdownStarted.Value = SceneTransitionHandler.sceneTransitionHandler.AllClientsAreLoaded();

            //While we are counting down, continually set the replicated time remaining value for clients (client should only receive the update once)
            if (_mCountdownStarted.Value && !_mReplicatedTimeSent)
            {
                SetReplicatedTimeRemainingClientRPC(mDelayedStartTime);
                _mReplicatedTimeSent = true;
            }

            return _mCountdownStarted.Value;
        }

        return _mClientStartCountdown;
    }

    /// <summary>
    ///     We want to send only once the Time Remaining so the clients
    ///     will deal with updating it. For that, we use a ClientRPC
    /// </summary>
    /// <param name="delayedStartTime"></param>
    [ClientRpc]
    // ReSharper disable once UnusedParameter.Local
    private void SetReplicatedTimeRemainingClientRPC(float delayedStartTime)
    {
        // // See the ShouldStartCountDown method for when the server updates the value
        // if (_mTimeRemaining == 0)
        // {
        //     Debug.LogFormat("Client side our first timer update value is {0}", delayedStartTime);
        //     _mTimeRemaining = delayedStartTime;
        // }
        // else
        // {
        //     Debug.LogFormat("Client side we got an update for a timer value of {0} when we shouldn't", delayedStartTime);
        // }
    }

    /// <summary>
    ///     IsCurrentGameOver
    ///     Returns whether the game is over or not
    /// </summary>
    /// <returns>true or false</returns>
    private bool IsCurrentGameOver()
    {
        if (IsServer)
            return IsGameOver.Value;
        return _mClientGameOver;
    }

    /// <summary>
    ///     HasGameStarted
    ///     Determine whether the game has started or not
    /// </summary>
    /// <returns>true or false</returns>
    private bool HasGameStarted()
    {
        if (IsServer)
            return HasTheGameStarted.Value;
        return _mClientGameStarted;
    }

    /// <summary>
    ///     OnGameStarted
    ///     Only invoked by the server, this hides the timer text and initializes the enemies and level
    /// </summary>
    private void OnGameStarted()
    {
        gameTimerText.gameObject.SetActive(false);
        CreateEnemies();
        CreateSuperEnemy();
    }

    public void SetScore(int score)
    {
        scoreText.SetText("0{0}", score);
    }

    public void SetLives(int lives)
    {
        livesText.SetText("0{0}", lives);
    }

    public void DisplayGameOverText(string message)
    {
        if (gameOverText)
        {
            gameOverText.SetText(message);
            gameOverText.gameObject.SetActive(true);
        }
    }

    public void SetGameEnd(GameOverReason reason)
    {
        Assert.IsTrue(IsServer, "SetGameEnd should only be called server side!");

        // We should only end the game if all the player's are dead
        if (reason != GameOverReason.Death)
        {
            IsGameOver.Value = true;
            BroadcastGameOverClientRpc(reason); // Notify our clients!
            return;
        }
        
        foreach (NetworkClient networkedClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = networkedClient.PlayerObject;
            if(playerObject == null) continue;
            
            // We should just early out if any of the player's are still alive
            if (playerObject.GetComponent<PlayerControl>().IsAlive)
                return;
        }
        
        IsGameOver.Value = true;
    }

    [ClientRpc]
    public void BroadcastGameOverClientRpc(GameOverReason reason)
    {
        var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        Assert.IsNotNull(localPlayerObject);

        if (localPlayerObject.TryGetComponent<PlayerControl>(out var playerControl))
            playerControl.NotifyGameOver(reason);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadMainMenu();
    }

    // USEFULLL: IMPORTANT SPAWNING FUNCTION
    private void CreateSuperEnemy()
    {
        Assert.IsTrue(IsServer, "Create Saucer should be called server-side only!");

        _mSaucer = Instantiate(superEnemyPrefab, saucerSpawnPoint.position, Quaternion.identity);

        // Spawn the Networked Object, this should notify the clients
        _mSaucer.GetComponent<NetworkObject>().Spawn();
    }

    private void CreateEnemy(GameObject prefab, float posX, float posY)
    {
        Assert.IsTrue(IsServer, "Create Enemy should be called server-side only!");

        var enemy = Instantiate(prefab);
        enemy.transform.position = new Vector3(posX, posY, 0.0f);

        // Spawn the Networked Object, this should notify the clients
        enemy.GetComponent<NetworkObject>().Spawn();
    }

    public void CreateEnemies()
    {
        float startX = -8;
        for (var i = 0; i < 10; i++)
        {
            CreateEnemy(enemy1Prefab, startX, 12);
            startX += 1.6f;
        }

        startX = -8;
        for (var i = 0; i < 10; i++)
        {
            CreateEnemy(enemy2Prefab, startX, 10);
            startX += 1.6f;
        }

        startX = -8;
        for (var i = 0; i < 10; i++)
        {
            CreateEnemy(enemy3Prefab, startX, 8);
            startX += 1.6f;
        }
    }
}
