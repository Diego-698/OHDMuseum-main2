using UnityEngine;
using UnityEngine.EventSystems;

// Attach to any UI Button (or any UI element) to make it shrink slightly
// while it is being pressed, then spring back when released.
public class ButtonPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float pressedScale = 0.95f;   // 0.95 = 5% smaller while held
    [SerializeField] float speed = 14f;            // how quickly it scales (higher = snappier)

    Vector3 baseScale;
    Vector3 target;

    void Awake()
    {
        baseScale = transform.localScale;
        target = baseScale;
    }

    void Update()
    {
        // smoothly move toward the target scale; unscaledDeltaTime so it works even if paused
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerDown(PointerEventData e) => target = baseScale * pressedScale;
    public void OnPointerUp(PointerEventData e)   => target = baseScale;
}
