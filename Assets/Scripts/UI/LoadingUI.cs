using SceneShit;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button toLevelBtn;
        [SerializeField] private Button exitToSecBtn;
        [SerializeField] private Button exitToMainBtn;
    
        [Header("Scenes")]
        [SerializeField] private string toLevelScene;
        private void Start()
        {
            toLevelBtn.onClick.AddListener(() => Loader.LoadScene(toLevelScene));
            exitToSecBtn.onClick.AddListener(() => Loader.LoadScene("SecScene"));
            exitToMainBtn.onClick.AddListener(() => Loader.LoadScene("MainMenu"));
            
        }

    }
}
