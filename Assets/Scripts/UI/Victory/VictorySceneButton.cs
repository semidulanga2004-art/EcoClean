using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoClean.Victory
{
    /// <summary>
    /// Put on any button. Set "Scene Name" in the Inspector (e.g. Gameplay or MainMenu),
    /// then hook Load() to the Button's OnClick event.
    /// The scene must be in the Build Profile scene list (project lead's step).
    /// </summary>
    public class VictorySceneButton : MonoBehaviour
    {
        [SerializeField] private string sceneName = "Gameplay";

        public void Load()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("VictorySceneButton: Scene Name is empty.", this);
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
