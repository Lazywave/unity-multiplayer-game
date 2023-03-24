using SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button toLevelBtn;
        [SerializeField] private Button exitToLobbyBtn;
        [SerializeField] private Button exitToMainBtn;
    
        [Header("Scenes")]
        [SerializeField] private string toLevelScene;
        private void Start()
        {
            toLevelBtn.onClick.AddListener(() => Loader.LoadScene(toLevelScene));
            exitToLobbyBtn.onClick.AddListener(() => Loader.LoadScene("Lobby"));
            exitToMainBtn.onClick.AddListener(() => Loader.LoadScene("MainMenu"));
            
        }

    }
}
