using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Attach to the banner UI Image. Cycles through painting sprites every few seconds.
//
// Cover = true  -> fills the whole banner and crops the overflow (keeps full size, no stretch).
//                  The script auto-creates the crop mask + display child at runtime, so you
//                  DON'T have to set anything up in the editor.
// Cover = false -> fits the painting inside the banner (no crop, may leave gaps).
[RequireComponent(typeof(Image))]
public class ImageSlideshow : MonoBehaviour
{
    [SerializeField] Sprite[] images;          // paintings to cycle through
    [SerializeField] float interval = 5f;      // seconds each painting stays up
    [SerializeField] bool fade = true;         // cross-fade between paintings
    [SerializeField] float fadeDuration = 0.4f;
    [SerializeField] bool cover = true;        // true = fill + crop, false = fit inside

    Image img;            // the image the painting is shown on
    RectTransform area;   // the banner rect we fill (this object)
    RectTransform rt;     // the display image's transform (== area when not cover)
    int index;

    void Awake()
    {
        var banner = GetComponent<Image>();
        area = (RectTransform)transform;

        if (cover)
        {
            // crop anything that spills outside the banner, and hide the banner's own image
            banner.enabled = false;
            if (GetComponent<RectMask2D>() == null) gameObject.AddComponent<RectMask2D>();

            // a child image we oversize to fill the banner; the mask trims the excess
            var go = new GameObject("PaintingDisplay", typeof(RectTransform), typeof(Image));
            rt = (RectTransform)go.transform;
            rt.SetParent(area, false);
            rt.SetAsFirstSibling();            // sit behind any text on the banner
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = false;        // we size it manually to cover
        }
        else
        {
            img = banner;
            rt = area;
            img.preserveAspect = true;         // fit inside, no crop
        }
    }

    void OnEnable()
    {
        if (images == null || images.Length == 0) return;
        index = 0;
        StartCoroutine(Play());
    }

    void OnDisable() => StopAllCoroutines();

    IEnumerator Play()
    {
        yield return null;            // wait one frame so the banner size is known
        Apply(images[0]);

        while (images.Length > 1)
        {
            yield return new WaitForSeconds(interval);
            index = (index + 1) % images.Length;

            if (fade)
            {
                yield return Fade(1f, 0f);
                Apply(images[index]);
                yield return Fade(0f, 1f);
            }
            else
            {
                Apply(images[index]);
            }
        }
    }

    void Apply(Sprite s)
    {
        img.sprite = s;
        if (!cover || s == null) return;

        // scale so the painting's smaller side fills the banner; the rest overflows and is cropped
        float vw = area.rect.width, vh = area.rect.height;
        float sw = s.rect.width,    sh = s.rect.height;
        if (sw <= 0 || sh <= 0) return;

        float scale = Mathf.Max(vw / sw, vh / sh);
        rt.sizeDelta = new Vector2(sw * scale, sh * scale);
        rt.anchoredPosition = Vector2.zero;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = img.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }
}
