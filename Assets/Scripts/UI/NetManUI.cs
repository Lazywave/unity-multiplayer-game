using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NetManUI : MonoBehaviour
    {   
        [Header("Buttons")]
        [SerializeField] private Button serverBtn;
        [SerializeField] private Button hostBtn;
        [SerializeField] private Button clientBtn;
        [SerializeField] private Button stopBtn;
        [SerializeField] private Button instanceBtn;

        private void Start()
        {
            serverBtn.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
            hostBtn.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
            clientBtn.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
            stopBtn.onClick.AddListener(() => NetworkManager.Singleton.Shutdown());
            instanceBtn.onClick.AddListener(CheckInstance);
        }
    
        private static void CheckInstance()
        {
            if(!NetworkManager.Singleton.IsListening)
            {
                print("No instance is running.");
            }
            else
            {
                var myString = NetworkManager.Singleton.IsServer ?
                    NetworkManager.Singleton.IsClient ? "Host" : "Server" : "Client";
                print("This instance is a " + myString);  
            
            }
        }
    }
}
