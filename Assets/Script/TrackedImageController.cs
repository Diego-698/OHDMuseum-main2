using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Tempelkan script ini di GameObject yang sama dengan ARTrackedImageManager
// (di dalam XR Origin). Fungsinya: cube hanya tampil saat QR benar-benar
// sedang dilacak, dan disembunyikan saat QR keluar dari pandangan kamera.
[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageController : MonoBehaviour
{
    ARTrackedImageManager manager;

    void Awake()
    {
        manager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        manager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        manager.trackablesChanged.RemoveListener(OnChanged);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var image in args.added)
            UpdateVisibility(image);

        foreach (var image in args.updated)
            UpdateVisibility(image);
    }

    void UpdateVisibility(ARTrackedImage image)
    {
        // Tampilkan prefab hanya saat status pelacakan = Tracking
        bool tracking = image.trackingState == TrackingState.Tracking;
        if (image.transform.childCount > 0)
            image.transform.GetChild(0).gameObject.SetActive(tracking);
        else
            image.gameObject.SetActive(tracking);
    }
}
