using System;
using SceneShit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerControl : NetworkBehaviour
{
    [Header("Weapon Settings")]
    public GameObject bulletPrefab;

    [Header("Movement Settings")]
    [SerializeField]
    private float mMoveSpeed = 3.5f;

    [Header("Player Settings")]
    [SerializeField]
    private NetworkVariable<int> mLives = new(3);

    private bool _mHasGameStarted;

    private bool _mIsAlive = true;

    private readonly NetworkVariable<int> _mMoveX = new();

    private GameObject _mMyBullet;
    private ClientRpcParams _mOwnerRPCParams;

    [SerializeField]
    private SpriteRenderer mPlayerVisual;
    private readonly NetworkVariable<int> _mScore = new();

    public bool IsAlive => mLives.Value > 0;

    private void Awake()
    {
        _mHasGameStarted = false;
    }

    private void Update()
    {
        if (SceneTransitionHandler.sceneTransitionHandler.GetCurrentSceneState() == SceneTransitionHandler.SceneStates.Ingame)
        {
            InGameUpdate();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsClient)
        {
            mLives.OnValueChanged -= OnLivesChanged;
            _mScore.OnValueChanged -= OnScoreChanged;
        }

        if (InvadersGame.Singleton)
        {
            InvadersGame.Singleton.IsGameOver.OnValueChanged -= OnGameStartedChanged;
            InvadersGame.Singleton.HasTheGameStarted.OnValueChanged -= OnGameStartedChanged;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Bind to OnValueChanged to display in log the remaining lives of this player
        // And to update InvadersGame singleton client-side
        mLives.OnValueChanged += OnLivesChanged;
        _mScore.OnValueChanged += OnScoreChanged;

        if (IsServer) _mOwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

        if (!InvadersGame.Singleton)
            InvadersGame.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
        else
            SubscribeToDelegatesAndUpdateValues();
    }

    private void SubscribeToDelegatesAndUpdateValues()
    {
        InvadersGame.Singleton.HasTheGameStarted.OnValueChanged += OnGameStartedChanged;
        InvadersGame.Singleton.IsGameOver.OnValueChanged += OnGameStartedChanged;

        if (IsClient && IsOwner)
        {
            InvadersGame.Singleton.SetScore(_mScore.Value);
            InvadersGame.Singleton.SetLives(mLives.Value);
        }

        _mHasGameStarted = InvadersGame.Singleton.HasTheGameStarted.Value;
    }

    public void IncreasePlayerScore(int amount)
    {
        Assert.IsTrue(IsServer, "IncreasePlayerScore should be called server-side only");
        _mScore.Value += amount;
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        _mHasGameStarted = newValue;
    }

    private void OnLivesChanged(int previousAmount, int currentAmount)
    {
        // Hide graphics client side upon death
        if (currentAmount <= 0 && IsClient)
            mPlayerVisual.enabled = false;

        if (!IsOwner) return;
        Debug.LogFormat("Lives {0} ", currentAmount);
        if (InvadersGame.Singleton != null) InvadersGame.Singleton.SetLives(mLives.Value);

        if (mLives.Value <= 0)
        {
            _mIsAlive = false;
        }
    }

    private void OnScoreChanged(int previousAmount, int currentAmount)
    {
        if (!IsOwner) return;
        Debug.LogFormat("Score {0} ", currentAmount);
        if (InvadersGame.Singleton != null) InvadersGame.Singleton.SetScore(_mScore.Value);
    } // ReSharper disable Unity.PerformanceAnalysis

    private void InGameUpdate()
    {
        if (!IsLocalPlayer || !IsOwner || !_mHasGameStarted) return;
        if (!_mIsAlive) return;

        var deltaX = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) deltaX -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) deltaX += 1;

        if (deltaX != 0)
        {
            var newMovement = new Vector3(deltaX, 0, 0);
            var position = transform.position;
            position = Vector3.MoveTowards(position,
                position + newMovement, mMoveSpeed * Time.deltaTime);
            transform.position = position;
        }

        if (Input.GetKeyDown(KeyCode.Space)) ShootServerRPC();
    }

    [ServerRpc]
    private void ShootServerRPC()
    {
        if (!_mIsAlive)
            return;

        if (_mMyBullet == null)
        {
            _mMyBullet = Instantiate(bulletPrefab, transform.position + Vector3.up, Quaternion.identity);
            //m_MyBullet.GetComponent<PlayerBullet>().owner = this;
            _mMyBullet.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void HitByBullet()
    {
        Assert.IsTrue(IsServer, "HitByBullet must be called server-side only!");
        if (!_mIsAlive) return;

        mLives.Value -= 1;

        if (mLives.Value <= 0)
        {
            // game over!
            _mIsAlive = false;
            _mMoveX.Value = 0;
            mLives.Value = 0;
            InvadersGame.Singleton.SetGameEnd(GameOverReason.Death);
            NotifyGameOverClientRpc(GameOverReason.Death, _mOwnerRPCParams);

            // Hide graphics of this player object server-side. Note we don't want to destroy the object as it
            // may stop the RPCs from reaching on the other side, as there is only one player controlled object
            mPlayerVisual.enabled = false;
        }
    }

    [ClientRpc]
    // ReSharper disable once UnusedParameter.Local
    private void NotifyGameOverClientRpc(GameOverReason reason, ClientRpcParams clientParams)
    {
        NotifyGameOver(reason);
    }

    /// <summary>
    /// This should only be called locally, either through NotifyGameOverClientRpc or through the InvadersGame.BroadcastGameOverReason
    /// </summary>
    /// <param name="reason"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void NotifyGameOver(GameOverReason reason)
    {
        Assert.IsTrue(IsLocalPlayer);
        _mHasGameStarted = false;
        switch (reason)
        {
            case GameOverReason.None:
                InvadersGame.Singleton.DisplayGameOverText("You have lost! Unknown reason!");
                break;
            case GameOverReason.EnemiesReachedBottom:
                InvadersGame.Singleton.DisplayGameOverText("You have lost! The enemies have invaded you!");
                break;
            case GameOverReason.Death:
                InvadersGame.Singleton.DisplayGameOverText("You have lost! Your health was depleted!");
                break;
            case GameOverReason.Max:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
        }
    }
}
