using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Put this on the panel's CANVAS (the object DisplayedPanelManager turns on/off).
// When the panel is shown, ALL of the canvas's children slide up from below together.
// Use this when the panel is made of several separate objects (no single card to slide).
public class SlideInChildren : MonoBehaviour
{
    [SerializeField] float duration = 0.35f;
    [SerializeField] float distance = 0f;   // how far below to start; 0 = the full screen height

    readonly List<RectTransform> kids = new List<RectTransform>();
    readonly List<Vector2> home = new List<Vector2>();
    bool captured;

    void Capture()
    {
        if (captured) return;
        kids.Clear();
        home.Clear();
        foreach (RectTransform c in transform)   // direct children only
        {
            kids.Add(c);
            home.Add(c.anchoredPosition);         // their resting positions
        }
        captured = true;
    }

    void OnEnable()
    {
        Capture();
        StopAllCoroutines();
        StartCoroutine(Slide());
    }

    IEnumerator Slide()
    {
        float d = distance > 0f ? distance : Screen.height;
        Vector2 off = new Vector2(0f, -d);

        // snap everything down first (no flash)
        for (int i = 0; i < kids.Count; i++)
            if (kids[i] != null) kids[i].anchoredPosition = home[i] + off;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            float k = 1f - (1f - n) * (1f - n);   // ease-out
            for (int i = 0; i < kids.Count; i++)
                if (kids[i] != null)
                    kids[i].anchoredPosition = Vector2.Lerp(home[i] + off, home[i], k);
            yield return null;
        }

        for (int i = 0; i < kids.Count; i++)
            if (kids[i] != null) kids[i].anchoredPosition = home[i];   // exact resting spot
    }
}
