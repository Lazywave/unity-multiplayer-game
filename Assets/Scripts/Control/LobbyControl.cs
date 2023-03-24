using SceneManagement;
using Unity.Netcode;
using UnityEngine;

public class LobbyControl : NetworkBehaviour
{
    [SerializeField] private string inGameSceneName = "InGame";

    public override void OnNetworkSpawn()
    {
        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
    }

    private void OnlyImportantFunction()
    {
        SceneTransitionHandler.SwitchScene(inGameSceneName);
    }
}
