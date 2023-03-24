using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneShit
{
    public class SceneTransitionHandler : NetworkBehaviour
    {
        // ReSharper disable once InconsistentNaming
        public static SceneTransitionHandler sceneTransitionHandler { get; internal set; }

        [SerializeField]
        public string defaultMainMenu = "MainMenu";

        public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
        public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;

        public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
        public event SceneStateChangedDelegateHandler OnSceneStateChanged;

        private int _numberOfClientsLoaded;

        // This can be extended for more complex logic
        public enum SceneStates
        {
            Init,
            First,
            Ingame,
            Lobby,
            Second
        }
    
        private SceneStates _sceneState;

        private void Awake()
        {
            if(sceneTransitionHandler != this && sceneTransitionHandler != null)
            {
                Destroy(sceneTransitionHandler.gameObject);
            }
            sceneTransitionHandler = this;
            SetSceneState(SceneStates.Init);
            DontDestroyOnLoad(this);
        }
    
        public void SetSceneState(SceneStates sceneState)
        {
            _sceneState = sceneState;
            OnSceneStateChanged?.Invoke(_sceneState);
        }

        private void Start()
        {
            if (_sceneState == SceneStates.Init)
            {
                SceneManager.LoadScene(defaultMainMenu);
            }
            
            // My Camera UI thing
            NetworkManager.Singleton.OnClientDisconnectCallback += DestroyLeftoverSceneObjects;
        }
    
        // Logic can do different things based on the current scene state
        public SceneStates GetCurrentSceneState()
        {
            return _sceneState;
        }

        // Once a host has started, we need to register the callbacks of clients trying to join
        public void RegisterCallbacks()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
    
        public void SwitchScene(string sceneName)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                _numberOfClientsLoaded = 0;
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }

        // Do something when a client has loaded a scene
        private void OnLoadComplete(ulong clientID, string sceneName, LoadSceneMode loadSceneMode)
        {
            _numberOfClientsLoaded++;
            OnClientLoadedScene?.Invoke(clientID);
        }
    
        // Client sync function, that I probably don't need
        public bool AllClientsAreLoaded()
        {
            return _numberOfClientsLoaded == NetworkManager.Singleton.ConnectedClientsList.Count;
        }

        // Ends the game and returns the client to the main menu -> IMPORTANT FUNCTIONALITY
        // If Host ends the game, clients are hard-locked
        public void ExitAndLoadMainMenu()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
            OnClientLoadedScene = null;
            SetSceneState(SceneStates.First);
            SceneManager.LoadScene(defaultMainMenu);
        }
    
        // My Camera UI thing
        private static void DestroyLeftoverSceneObjects(ulong obj)
        {
            // Currently only destroys the video UI
            // If we have more objects that need to be destroyed, we can add them here (object pooling?)
            Destroy(GameObject.FindGameObjectWithTag("VideoUI"));
        }
    }
}




















