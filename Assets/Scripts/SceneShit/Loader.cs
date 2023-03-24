using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneShit
{
    public class Loader : MonoBehaviour
    {
        public delegate void MainMenuLoadedDelegateHandler();

        public static event MainMenuLoadedDelegateHandler OnMainMenuLoaded;
    
        [SerializeField] private string sceneName = "SecScene";
    
        private void OnTriggerEnter2D(Collider2D col)
        {
            LoadScene(sceneName);
        }

        public static void LoadScene(string loadedSceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(loadedSceneName, LoadSceneMode.Single);
        
            if (loadedSceneName == "MainMenu")
            {
                OnMainMenuLoaded?.Invoke();
            }
        }
    }
}
