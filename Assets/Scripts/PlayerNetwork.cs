using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    
    [SerializeField] private Transform spawnedObject;

    private void Update()
    {
        if(!IsOwner) return;

        DoMovement();
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            var spawnedObjectTransform = Instantiate(spawnedObject);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }
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
