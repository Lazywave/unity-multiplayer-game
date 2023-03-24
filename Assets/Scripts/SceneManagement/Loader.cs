using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class Loader : MonoBehaviour
    {
    
        [SerializeField] private string sceneName = "Lobby";
    
        private void OnTriggerEnter2D(Collider2D col)
        {
            LoadScene(sceneName);
        }

        public static void LoadScene(string loadedSceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(loadedSceneName, LoadSceneMode.Single);
        
        }
    }
}
