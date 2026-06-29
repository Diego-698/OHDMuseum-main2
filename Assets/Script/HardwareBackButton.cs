using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Put one of these in each scene. It makes the phone's hardware/gesture BACK button
// do something: load the "Back Scene", or quit the app if that's left empty.
// (Android routes the back button to the Escape key, which we read via the new Input System.)
public class HardwareBackButton : MonoBehaviour
{
    [SerializeField] string backScene = "HomeScene";   // where back goes; leave EMPTY to quit the app

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Back();
    }

    // also callable from an on-screen button if you want
    public void Back()
    {
        if (string.IsNullOrEmpty(backScene))
            Application.Quit();
        else
            SceneManager.LoadScene(backScene);
    }
}
