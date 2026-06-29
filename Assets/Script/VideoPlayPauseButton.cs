using UnityEngine;
using UnityEngine.UI;

// Put this on ONE UI Button. Drag the AR scene's DisplayedPanelManager into "Manager".
// It self-wires its click, toggles the video, swaps the play/pause icon, and fades the
// button out after a couple of seconds. It stays clickable even while faded, so a tap
// always works and brings it back to full opacity.
[RequireComponent(typeof(Button))]
public class VideoPlayPauseButton : MonoBehaviour
{
    [SerializeField] DisplayedPanelManager manager;
    [SerializeField] Image icon;            // the button's image (auto-found if left empty)
    [SerializeField] Sprite playSprite;     // optional: shown while the video is paused
    [SerializeField] Sprite pauseSprite;    // optional: shown while the video is playing

    [Header("Fade")]
    [SerializeField] float visibleSeconds = 2f;   // stay fully visible this long after a tap
    [SerializeField] float fadeSpeed = 6f;        // higher = snappier fade
    [SerializeField] float fadedAlpha = 0f;       // 0 = fully invisible; set ~0.2 to leave a faint hint

    Button button;
    CanvasGroup group;
    float hideAt;
    bool hadVideo;

    void Awake()
    {
        button = GetComponent<Button>();
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        button.onClick.AddListener(OnClick);   // self-wires the click
        if (icon == null) icon = GetComponent<Image>();
    }

    void OnClick()
    {
        if (manager != null) manager.ToggleVideo();
        Show();   // any tap on it keeps it up
    }

    public void Show() => hideAt = Time.unscaledTime + visibleSeconds;

    void Update()
    {
        if (manager == null) return;
        bool hasVideo = manager.HasVideo;

        if (hasVideo && !hadVideo) Show();   // a video just appeared -> show the button
        hadVideo = hasVideo;

        float target = !hasVideo ? 0f
                     : (Time.unscaledTime < hideAt) ? 1f
                     : fadedAlpha;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);

        // ALWAYS clickable while a video is on screen, even while faded -> a tap always works
        group.interactable = hasVideo;
        group.blocksRaycasts = hasVideo;

        if (icon != null && playSprite != null && pauseSprite != null)
            icon.sprite = manager.IsVideoPlaying ? pauseSprite : playSprite;
    }
}
