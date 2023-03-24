using SceneManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class InGameControl : NetworkBehaviour
{
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadMainMenu();
    }

    // Generic spawning function
    private void SpawnPrefab(GameObject prefab, float posX, float posY)
    {
        Assert.IsTrue(IsServer, "SpawnPrefab should be called server-side only!");

        var spawnedPrefab = Instantiate(prefab);
        spawnedPrefab.transform.position = new Vector3(posX, posY, 0.0f);

        spawnedPrefab.GetComponent<NetworkObject>().Spawn();
    }
}
