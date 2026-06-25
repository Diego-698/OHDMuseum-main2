using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Put this on a UI RawImage. It plays the GIF frames that were exported into
// Assets/Resources/LoadingDots (loaded automatically, no manual assignment).
[RequireComponent(typeof(RawImage))]
public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] string resourceFolder = "LoadingDots";
    [SerializeField] float fps = 30f;

    RawImage img;
    Texture2D[] frames;
    float timer;
    int index;

    void Awake()
    {
        img = GetComponent<RawImage>();
        // load every frame and sort by name (dot_001, dot_002, ...) so they play in order
        frames = Resources.LoadAll<Texture2D>(resourceFolder)
                          .OrderBy(t => t.name)
                          .ToArray();
        if (frames.Length > 0) img.texture = frames[0];
    }

    void OnEnable()
    {
        timer = 0f;
        index = 0;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.unscaledDeltaTime;          // unscaled so it spins even if the game is paused
        if (timer < 1f / fps) return;

        timer -= 1f / fps;
        index = (index + 1) % frames.Length;
        img.texture = frames[index];
    }
}
