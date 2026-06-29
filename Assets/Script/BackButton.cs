using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to the back button and hook GoToHomeScreen() to its OnClick event.
public class BackButton : MonoBehaviour
{
    [SerializeField] string homeSceneName = "HomeScene";
    [SerializeField] string ARScene;
    [SerializeField] string TutorialScene;
    [SerializeField] string AboutScene;

    public void GoToHomeScreen()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    public void GoToARScene()
    {
        SceneManager.LoadScene(ARScene);
    }

    public void GoToTutorialScene()
    {
        SceneManager.LoadScene(TutorialScene);
    }

    public void GoToAboutScene()
    {
        SceneManager.LoadScene(AboutScene);
    }
}
