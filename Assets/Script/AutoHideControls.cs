using UnityEngine;
using UnityEngine.InputSystem;

// Put this (plus a CanvasGroup) on the video controls panel.
// It shows the controls, fades them out after a couple of seconds of no input,
// and brings them back whenever the screen is tapped.
[RequireComponent(typeof(CanvasGroup))]
public class AutoHideControls : MonoBehaviour
{
    [SerializeField] float visibleSeconds = 2f;   // stay up this long after a tap
    [SerializeField] float fadeSpeed = 6f;        // higher = snappier fade

    CanvasGroup group;
    float hideAt;

    void Awake() => group = GetComponent<CanvasGroup>();

    void OnEnable() => Show();   // visible the moment the controls appear

    void Update()
    {
        if (Tapped()) Show();

        float target = (Time.unscaledTime < hideAt) ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);

        bool visible = group.alpha > 0.5f;
        group.interactable = visible;     // only clickable while shown
        group.blocksRaycasts = visible;
    }

    // call this to keep the controls up (also hooked from the play/pause button if you like)
    public void Show() => hideAt = Time.unscaledTime + visibleSeconds;

    static bool Tapped()
    {
        var p = Pointer.current;   // covers touch, mouse, or pen
        if (p != null && p.press.wasPressedThisFrame) return true;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame) return true;

        return false;
    }
}
