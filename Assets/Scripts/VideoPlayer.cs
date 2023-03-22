using UnityEngine;
using UnityEngine.UI;

namespace MainGame.Code.Scripts.Interactables

{
    public class VideoPlayer : MonoBehaviour
    {
        [SerializeField] private string videoPath;
        [SerializeField] private string videoURL;

        private RawImage _videoImage;
        private UnityEngine.Video.VideoPlayer _videoPlayer;

        private void Start()
        {
            var videoElements = GameObject.FindGameObjectsWithTag("VideoUI");
            
            foreach (var videoElement in videoElements)
            {
                switch (videoElement.name)
                {
                    case "VideoImage":
                        _videoImage = videoElement.GetComponent<RawImage>();
                        break;
                    case "VideoPlayer":
                        _videoPlayer = videoElement.GetComponent<UnityEngine.Video.VideoPlayer>();
                        break;
                }
            }
            _videoImage.enabled = false;
            
            _videoPlayer.playOnAwake = false;
            _videoPlayer.url = videoPath == "" ? videoURL : videoPath;
            _videoPlayer.isLooping = true;
            _videoPlayer.Prepare();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _videoPlayer.Play();
            _videoImage.enabled = true;
        }
        
        private void OnTriggerExit2D(Collider2D col)
        {
            _videoImage.enabled = false;
            _videoPlayer.Stop();
            _videoPlayer.Prepare();
        }
    }
}