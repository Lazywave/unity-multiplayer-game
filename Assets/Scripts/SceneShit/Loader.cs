using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneShit
{
    public class Loader : MonoBehaviour
    {
        public delegate void MainSceneLoadedDelegateHandler();

        public static event MainSceneLoadedDelegateHandler OnMainSceneLoaded;
    
        [SerializeField] private string sceneName = "SecScene";
    
        private void OnTriggerEnter2D(Collider2D col)
        {
            LoadScene(sceneName);
        }

        public static void LoadScene(string loadedSceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(loadedSceneName, LoadSceneMode.Single);
        
            if (loadedSceneName == "MainScene")
            {
                OnMainSceneLoaded?.Invoke();
            }
        }
    }
}
