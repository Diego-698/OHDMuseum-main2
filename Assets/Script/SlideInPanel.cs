using UnityEngine;
using System.Collections;

// Put this on the panel's CONTENT object (a child of the Canvas, NOT the Canvas itself).
// Whenever it is shown (SetActive true), it slides up from below into place.
[RequireComponent(typeof(RectTransform))]
public class SlideInPanel : MonoBehaviour
{
    [SerializeField] float duration = 0.35f;
    [SerializeField] float distance = 0f;   // how far below to start; 0 = use the panel's own height

    RectTransform rt;
    Vector2 homePos;
    bool captured;
    bool invalid;

    void Awake()
    {
        rt = (RectTransform)transform;

        // A Screen Space Overlay/Camera Canvas has its RectTransform driven by Unity, so it can't
        // be animated. If this is on a Canvas, do nothing rather than break the panel.
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.LogWarning("SlideInPanel is on a Canvas. Move it to a CHILD content object inside " +
                             "the Canvas. Disabling so it doesn't break the panel.", this);
            invalid = true;
            return;
        }

        homePos = rt.anchoredPosition;   // resting position
        captured = true;
    }

    void OnEnable()
    {
        if (invalid) return;
        if (!captured) { homePos = rt.anchoredPosition; captured = true; }
        StopAllCoroutines();
        StartCoroutine(Slide());
    }

    IEnumerator Slide()
    {
        // wait one frame so layout/size is valid before measuring height
        yield return null;

        float d = distance > 0f ? distance : Mathf.Max(rt.rect.height, 100f);
        Vector2 start = homePos + new Vector2(0f, -d);
        rt.anchoredPosition = start;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            float k = 1f - (1f - n) * (1f - n);   // ease-out
            rt.anchoredPosition = Vector2.Lerp(start, homePos, k);
            yield return null;
        }
        rt.anchoredPosition = homePos;   // always end exactly at the resting position
    }
}
