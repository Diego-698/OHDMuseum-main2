using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to the back button and hook GoToHomeScreen() to its OnClick event.
public class BackButton : MonoBehaviour
{
    [SerializeField] string homeSceneName = "HomeScene";

    public void GoToHomeScreen()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}
