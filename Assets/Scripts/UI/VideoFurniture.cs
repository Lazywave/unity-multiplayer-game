using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    public class VideoFurniture : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IDsIdentical(col, out var videoPlayerPrefab)) return;
        
            var videoPlayer = videoPlayerPrefab.GetComponentInChildren<VideoPlayer>();
            var videoImage = videoPlayerPrefab.GetComponentInChildren<RawImage>();
        
            videoPlayer.Play();
            videoImage.enabled = true;
        }
    
        private void OnTriggerExit2D(Collider2D col)
        {
            if (!IDsIdentical(col, out var videoPlayerPrefab)) return;

            var videoPlayer = videoPlayerPrefab.GetComponentInChildren<VideoPlayer>();
            var videoImage = videoPlayerPrefab.GetComponentInChildren<RawImage>();

            videoImage.enabled = false;
            videoPlayer.Stop();
            videoPlayer.Prepare();
        }

    
        // ReSharper disable once InconsistentNaming
        public static bool IDsIdentical(Component col, out GameObject videoPlayerPrefab)
        {
            try
            {
                videoPlayerPrefab = GameObject.FindGameObjectWithTag("VideoUI");

                // This checks that only the video player that belongs
                // to the player that entered the trigger is affected
                // There's probably a better way to do this...
                var vppId = videoPlayerPrefab.GetComponent<VideoPlayerID>().id;
                var colId = col.GetComponentInChildren<PlayerController>().OwnerClientId;

                return colId == vppId;
            }
            catch
            {
                videoPlayerPrefab = null;
                return false;
            }
        }
    }
}