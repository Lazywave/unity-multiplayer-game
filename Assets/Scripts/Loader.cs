using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [SerializeField] private string sceneName = "SecScene";

    private void OnTriggerEnter2D(Collider2D col)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        col.GetComponent<Transform>().position = new Vector3(0, 0, 0);
    }
}
