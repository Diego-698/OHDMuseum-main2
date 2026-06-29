using UnityEngine;
using UnityEngine.Video;

// Put this on a video prefab's quad (the object with the VideoPlayer + a MeshRenderer).
// It plays the video into a RenderTexture made at runtime and shows it on the quad with an
// unlit material. This is the reliable way to show video in URP — the built-in "Material
// Override" render mode usually shows a gray quad because URP materials use _BaseMap, not _MainTex.
//
// Works for ANY video clip, landscape or portrait: the RenderTexture is sized to the clip,
// so orientation is handled automatically. Just swap the VideoPlayer's Video Clip per video.
[RequireComponent(typeof(VideoPlayer))]
public class ARVideoSurface : MonoBehaviour
{
    [Tooltip("OFF = stretch to fill the painting (like Dasamuka). " +
             "ON = keep the video's own shape so landscape/portrait clips aren't distorted.")]
    [SerializeField] bool preserveAspect = false;

    RenderTexture rt;
    VideoPlayer vp;
    Renderer rend;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        rend = GetComponent<Renderer>();

        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.isLooping = true;

        // unlit material so the video shows full-brightness, regardless of scene lighting
        Shader sh = Shader.Find("Unlit/Texture");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        if (rend != null && sh != null) rend.material = new Material(sh);

        vp.prepareCompleted += OnPrepared;
        vp.Prepare();
    }

    void OnPrepared(VideoPlayer p)
    {
        int w = Mathf.Max(16, (int)p.width);
        int h = Mathf.Max(16, (int)p.height);

        rt = new RenderTexture(w, h, 0);
        rt.Create();
        p.targetTexture = rt;
        if (rend != null) rend.material.mainTexture = rt;

        if (preserveAspect)
        {
            // the manager has already scaled this quad to the painting size;
            // shrink one axis so the video keeps its own shape (letterboxed inside the painting)
            Vector3 s = transform.localScale;
            float paintingAspect = s.x / s.y;
            float videoAspect = (float)w / h;
            if (videoAspect > paintingAspect) s.y = s.x / videoAspect;  // video wider -> reduce height
            else                              s.x = s.y * videoAspect;  // video taller -> reduce width
            transform.localScale = s;
        }

        p.Play();
    }

    void OnDestroy()
    {
        if (rt != null) { rt.Release(); Destroy(rt); }
    }
}
