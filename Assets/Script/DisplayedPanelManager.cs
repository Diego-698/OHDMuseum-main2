using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.Video;

public class DisplayedPanelManager : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager manager;
    [SerializeField] GameObject infoPanel;
    [SerializeField] TMPro.TextMeshProUGUI titleText;
    [SerializeField] TMPro.TextMeshProUGUI bodyText;
    [SerializeField] AudioSource narration;
    [SerializeField] UnityEngine.UI.Image artworkImage;   // picture shown on the panel
    [SerializeField] GameObject playButton;     // audio play icon
    [SerializeField] GameObject pauseButton;    // audio pause icon
    [SerializeField] GameObject loadingPanel;   // loading-dots overlay shown before the panel
    [SerializeField] float loadingSeconds = 1f;

    [Header("Video controls (for video paintings)")]
    [SerializeField] GameObject videoControls;   // play/pause UI shown only while a video plays
    [SerializeField] GameObject videoPlayButton;
    [SerializeField] GameObject videoPauseButton;

    // one entry per painting
    [System.Serializable]
    public class Artwork
    {
        public string imageName;   // must match Reference Image name
        public string title;
        [TextArea] public string description;
        public AudioClip audio;
        public Sprite image;       // picture shown on the panel
        public GameObject videoPrefab;   // optional: if set, this prefab is shown ON the painting (no panel)
    }
    [SerializeField] Artwork[] artworks;

    string currentName;
    GameObject currentVideoInstance;   // the prefab spawned for the current video painting
    VideoPlayer currentVideo;          // the VideoPlayer inside that prefab

    void OnEnable()  => manager.trackablesChanged.AddListener(OnChanged);
    void OnDisable() => manager.trackablesChanged.RemoveListener(OnChanged);

    void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
        if (videoControls != null) videoControls.SetActive(false);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> e)
    {
        foreach (var img in e.added)   HandleImage(img);
        foreach (var img in e.updated) HandleImage(img);
    }

    void HandleImage(ARTrackedImage img)
    {
        if (img.trackingState != TrackingState.Tracking) return;

        string name = img.referenceImage.name;
        if (name == currentName) return;   // already showing this one

        var art = System.Array.Find(artworks,
            a => string.Equals(a.imageName.Trim(), name.Trim(),
                               System.StringComparison.OrdinalIgnoreCase));
        if (art == null)
        {
            Debug.LogWarning($"No artwork entry matches reference image '{name}'");
            return;
        }

        currentName = name;
        StopAllCoroutines();

        // remove the prefab spawned for the previous painting (unsubscribe safely)
        if (currentVideoInstance != null)
        {
            var vpOld = currentVideoInstance.GetComponentInChildren<VideoPlayer>();
            if (vpOld != null)
            {
                vpOld.loopPointReached -= OnVideoEnded;
                vpOld.errorReceived -= OnVideoError;
            }
            Destroy(currentVideoInstance);
        }
        currentVideo = null;
        if (videoControls != null) videoControls.SetActive(false);

        if (art.videoPrefab != null)
        {
            // this painting has a video prefab -> spawn it pinned on the artwork, no panel
            if (infoPanel != null) infoPanel.SetActive(false);
            currentVideoInstance = Instantiate(art.videoPrefab, img.transform, false);
            currentVideoInstance.transform.localPosition = Vector3.zero;
            // make the video exactly the size of the tracked painting
            currentVideoInstance.transform.localScale = new Vector3(img.size.x, img.size.y, 1f);

            currentVideo = currentVideoInstance.GetComponentInChildren<VideoPlayer>();
            if (currentVideo != null)
            {
                Debug.Log($"DisplayedPanelManager: spawned video prefab '{currentVideoInstance.name}' and found VideoPlayer.");
                currentVideo.loopPointReached += OnVideoEnded;
                currentVideo.errorReceived += OnVideoError;
                currentVideo.Play();
            }
            else
            {
                Debug.LogWarning($"DisplayedPanelManager: spawned video prefab '{currentVideoInstance.name}' but no VideoPlayer found.");
            }
            if (videoControls != null) videoControls.SetActive(true);
            RefreshVideoButton();
        }
        else
        {
            // normal painting -> details panel
            StartCoroutine(ShowAfterLoading(art));
        }
    }

    // true while a video is spawned and playing
    public bool IsVideoPlaying => currentVideo != null && currentVideo.isPlaying;
    // true while any video is on screen (playing or paused)
    public bool HasVideo => currentVideo != null;

    // hook this to the video Play/Pause button
    public void ToggleVideo()
    {
        if (currentVideo == null) return;
        if (currentVideo.isPlaying) currentVideo.Pause();
        else                        currentVideo.Play();
        RefreshVideoButton();
    }

    void RefreshVideoButton()
    {
        bool playing = currentVideo != null && currentVideo.isPlaying;
        if (videoPlayButton  != null) videoPlayButton.SetActive(!playing);
        if (videoPauseButton != null) videoPauseButton.SetActive(playing);
    }

    void OnVideoEnded(VideoPlayer vp)
    {
        Debug.Log("DisplayedPanelManager: video ended.");
        RefreshVideoButton();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"DisplayedPanelManager: VideoPlayer error - {message}");
    }

    IEnumerator ShowAfterLoading(Artwork art)
    {
        infoPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(loadingSeconds);

        if (loadingPanel != null) loadingPanel.SetActive(false);

        titleText.text = art.title;
        bodyText.text  = art.description;
        narration.clip = art.audio;
        if (artworkImage != null && art.image != null) artworkImage.sprite = art.image;
        infoPanel.SetActive(true);

        narration.Stop();   // wait for the play button
        RefreshPlayPauseButton();
    }

    // hook to the Play/Pause button
    public void ToggleAudio()
    {
        if (narration.isPlaying) narration.Pause();
        else                     narration.Play();
        RefreshPlayPauseButton();
    }

    // hook to the Close button
    public void ClosePanel()
    {
        narration.Stop();
        infoPanel.SetActive(false);
        currentName = null;   // allow the same painting to be recognised again
        RefreshPlayPauseButton();
    }

    void Update()
    {
        RefreshPlayPauseButton();
        RefreshVideoButton();
    }

    void RefreshPlayPauseButton()
    {
        bool playing = narration != null && narration.isPlaying;
        if (playButton  != null) playButton.SetActive(!playing);
        if (pauseButton != null) pauseButton.SetActive(playing);
    }
}
