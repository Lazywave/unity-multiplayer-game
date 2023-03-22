using UnityEngine;
using UnityEngine.Video;

public class VideoPreLoader : MonoBehaviour
{
    [SerializeField] private string videoPath;
    [SerializeField] private string videoURL;
    
    private string _url;
    
    private void Start()
    {
        _url = videoPath == "" ? videoURL : videoPath;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!VideoFurniture.IDsIdentical(col, out var videoPlayerPrefab)) return;
        
        var videoPlayer = videoPlayerPrefab.GetComponentInChildren<VideoPlayer>();
        videoPlayer.url = _url;
        videoPlayer.Prepare();
    }
}
