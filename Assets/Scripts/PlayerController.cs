using UI;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObject;
    [SerializeField] private GameObject videoPlayerPrefab;
    
    private void Start()
    {
        if (!IsOwner) return;
        var vpp = Instantiate(videoPlayerPrefab);
        vpp.GetComponent<VideoPlayerID>().id = OwnerClientId;
    }

    private void Update()
    {
        if(!IsOwner) return;

        DoMovement();

        if (Input.GetKeyDown(KeyCode.T))
        {
            var spawnedObjectTransform = Instantiate(spawnedObject);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DisconnectClientServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    [ServerRpc]
    private void DisconnectClientServerRpc(ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID);
        print("Successfully disconnected client from server.");
    }

    private void DoMovement()
    {
        var moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = 1f;
        if (Input.GetKey(KeyCode.W)) moveDir.y = 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;


        const float moveSpeed = 5f;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
    }
}
