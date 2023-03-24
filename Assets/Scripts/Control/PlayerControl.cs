using SceneManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerControl : NetworkBehaviour
{
    public GameObject toSpawnPrefab;

    // ReSharper disable once NotAccessedField.Local
    private ClientRpcParams _ownerRPCParams; // Used to send RPCs to the owner of this object
    private GameObject _myPrefab;

    private void Update()
    {
        if (SceneTransitionHandler.sceneTransitionHandler.GetCurrentSceneState() == SceneTransitionHandler.SceneStates.Ingame)
        {
            InGameUpdate();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            // unsub from client side events
        }

        if (IsServer)
        {
            // unsub from server side events
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) _ownerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };
    }

    private void InGameUpdate()
    {
        if (!IsLocalPlayer || !IsOwner) return;
        
        // movement and other input checks
        if (Input.GetKeyDown(KeyCode.Space)) SpawnServerRPC();
    }

    [ServerRpc]
    private void SpawnServerRPC()
    {
        _myPrefab = Instantiate(toSpawnPrefab, transform.position + Vector3.up, Quaternion.identity);
        _myPrefab.GetComponent<NetworkObject>().Spawn();
    }

    public void ServerCalculatedThings()
    {
        Assert.IsTrue(IsServer, "ServerCalculatedThings must be called server-side only!");
    }
}
