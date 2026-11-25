using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroductionController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Gameplay";

    public void StartLevel()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
}