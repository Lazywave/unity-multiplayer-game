using Unity.Netcode;
using UnityEngine;

namespace SceneShit
{
    public class NetworkSpawning : MonoBehaviour
    {
        [SerializeField] private Transform playerPrefab;

        private void Start()
        {
            Loader.OnMainSceneLoaded += SpawnPlayerServerRpc;

        }

        [ServerRpc]
        internal void SpawnPlayerServerRpc()
        {
            print("Spawning player");
            var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
