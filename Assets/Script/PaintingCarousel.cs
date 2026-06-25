using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// Put this on the card (the object with the painting Image). It auto-advances on a
// timer with a sliding transition, and you can also swipe left/right. Dots below show
// the current page. The second image + clip mask are created automatically.
public class PaintingCarousel : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class Item
    {
        public Sprite image;
        public string title;
    }

    [SerializeField] Item[] items;
    [SerializeField] Image display;                       // the painting Image
    [SerializeField] TMPro.TextMeshProUGUI titleText;     // the title label
    [SerializeField] bool loop = true;

    [Header("Auto-slide")]
    [SerializeField] float autoplaySeconds = 4f;          // 0 = no auto-advance
    [SerializeField] float slideDuration = 0.45f;

    [Header("Swipe")]
    [SerializeField] float swipeThreshold = 60f;

    [Header("Dots (page indicator)")]
    [SerializeField] bool showDots = true;                // turn off for the auto-slide / featured one
    [SerializeField] RectTransform dotsContainer;         // leave empty to auto-create below the card
    [SerializeField] Sprite dotSprite;                    // leave empty for square dots
    [SerializeField] float dotSize = 16f;
    [SerializeField] Color activeColor = Color.white;
    [SerializeField] Color inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    int index;
    float startX;
    float nextAdvance;
    bool sliding;
    Vector2 homePos;                                      // the painting's real resting position
    Image secondary;                                      // created at runtime for the slide
    readonly List<Image> dots = new List<Image>();

    void Awake()
    {
        if (display != null)
        {
            display.preserveAspect = true;
            homePos = display.rectTransform.anchoredPosition;   // remember where the painting actually sits
        }
        SetupSecondary();
    }

    void Start()
    {
        BuildDots();
        if (items != null && items.Length > 0)
        {
            index = 0;
            if (display != null) display.sprite = items[0].image;
            if (titleText != null) titleText.text = items[0].title;
            UpdateDots();
        }
        nextAdvance = Time.unscaledTime + autoplaySeconds;
    }

    void Update()
    {
        if (autoplaySeconds > 0f && !sliding && items != null && items.Length > 1
            && Time.unscaledTime >= nextAdvance)
            Move(+1);
    }

    // hook these to arrow buttons too if you like
    public void Next() => Move(+1);
    public void Prev() => Move(-1);

    void Move(int dir)
    {
        if (sliding || items == null || items.Length == 0) return;

        int ni = index + dir;
        if (loop) ni = (ni + items.Length) % items.Length;
        else { ni = Mathf.Clamp(ni, 0, items.Length - 1); if (ni == index) { nextAdvance = Time.unscaledTime + autoplaySeconds; return; } }

        StartCoroutine(Slide(ni, dir));
    }

    IEnumerator Slide(int newIndex, int dir)
    {
        sliding = true;
        index = newIndex;
        if (titleText != null) titleText.text = items[index].title;
        UpdateDots();

        var dRT = display.rectTransform;
        var parent = dRT.parent as RectTransform;
        float w = (parent != null && parent.rect.width > 1f) ? parent.rect.width : dRT.rect.width;

        secondary.sprite = items[index].image;
        secondary.gameObject.SetActive(true);
        var sRT = secondary.rectTransform;
        Vector2 inStart = homePos + new Vector2(dir * w, 0f);    // new painting starts off to the side
        Vector2 outEnd  = homePos + new Vector2(-dir * w, 0f);   // old painting exits the other side
        sRT.anchoredPosition = inStart;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / slideDuration);
            dRT.anchoredPosition = Vector2.Lerp(homePos, outEnd, k);
            sRT.anchoredPosition = Vector2.Lerp(inStart, homePos, k);
            yield return null;
        }

        display.sprite = items[index].image;
        dRT.anchoredPosition = homePos;
        secondary.gameObject.SetActive(false);

        sliding = false;
        nextAdvance = Time.unscaledTime + autoplaySeconds;
    }

    // ---------- swipe ----------

    public void OnBeginDrag(PointerEventData e) => startX = e.position.x;

    public void OnEndDrag(PointerEventData e)
    {
        float delta = e.position.x - startX;
        if (Mathf.Abs(delta) < swipeThreshold) { nextAdvance = Time.unscaledTime + autoplaySeconds; return; }
        Move(delta < 0 ? +1 : -1);
    }

    // ---------- setup helpers ----------

    void SetupSecondary()
    {
        if (display == null) return;

        var parent = display.transform.parent as RectTransform;
        if (parent != null && parent.GetComponent<RectMask2D>() == null)
            parent.gameObject.AddComponent<RectMask2D>();   // clip the sliding overflow to the frame

        var go = new GameObject("CarouselSlide", typeof(RectTransform), typeof(Image));
        secondary = go.GetComponent<Image>();
        var sRT = secondary.rectTransform;
        var dRT = display.rectTransform;
        sRT.SetParent(dRT.parent, false);
        sRT.anchorMin = dRT.anchorMin;
        sRT.anchorMax = dRT.anchorMax;
        sRT.pivot = dRT.pivot;
        sRT.sizeDelta = dRT.sizeDelta;
        sRT.localScale = dRT.localScale;
        sRT.anchoredPosition = homePos;
        secondary.preserveAspect = true;
        secondary.raycastTarget = false;
        secondary.gameObject.SetActive(false);
    }

    void EnsureDotsContainer()
    {
        if (dotsContainer != null) return;

        // parent to the painting's PARENT (not the painting itself) so the dots
        // stay put while only the painting slides; pinned just inside the bottom edge.
        Transform parent = (display != null && display.transform.parent != null)
            ? display.transform.parent : transform;

        var go = new GameObject("Dots", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.SetAsLastSibling();                          // draw on top of the painting
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(0f, dotSize + 8f);
        rt.anchoredPosition = new Vector2(0f, 10f);     // inside the frame so the clip mask won't hide it
        dotsContainer = rt;
    }

    void BuildDots()
    {
        if (items == null || !showDots) return;
        EnsureDotsContainer();
        if (dotsContainer == null) return;

        for (int i = dotsContainer.childCount - 1; i >= 0; i--)
            Destroy(dotsContainer.GetChild(i).gameObject);
        dots.Clear();

        var layout = dotsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = dotsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = dotSize * 0.6f;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        for (int i = 0; i < items.Length; i++)
        {
            var go = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(dotsContainer, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(dotSize, dotSize);
            var img = go.GetComponent<Image>();
            img.sprite = dotSprite;
            img.raycastTarget = false;
            dots.Add(img);
        }
    }

    void UpdateDots()
    {
        for (int i = 0; i < dots.Count; i++)
            dots[i].color = (i == index) ? activeColor : inactiveColor;
    }
}
