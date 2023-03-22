using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetManUI : MonoBehaviour
{   
    [Header("Buttons")]
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button stopBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        hostBtn.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        clientBtn.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        stopBtn.onClick.AddListener(() => NetworkManager.Singleton.Shutdown());
    }
}
