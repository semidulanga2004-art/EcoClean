using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AudioManagerPlayModeTest : MonoBehaviour
{
    [SerializeField] private AudioManager managerPrefab;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip uiClick;

    private AudioManager manager;
    private bool sceneTestStarted;
    private string status = "Ready.";

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<AudioListener>();
        Camera testCamera = gameObject.AddComponent<Camera>();
        testCamera.clearFlags = CameraClearFlags.SolidColor;
        testCamera.backgroundColor = Color.black;
        testCamera.cullingMask = 0;
        manager = managerPrefab == null ? null : Instantiate(managerPrefab);
        if (manager == null)
        {
            status = "AudioManager prefab is missing.";
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(16, 16, 480, 310), GUI.skin.box);
        GUILayout.Label("AudioManager Play Mode test");

        GUI.enabled = manager != null && backgroundMusic != null;
        if (GUILayout.Button("Start music"))
        {
            manager.PlayMusic(backgroundMusic);
            status = "Music requested.";
        }

        GUI.enabled = manager != null;
        if (GUILayout.Button("Stop music"))
        {
            manager.StopMusic();
            status = "Music stopped.";
        }

        GUI.enabled = manager != null && uiClick != null;
        if (GUILayout.Button("Play UI click"))
        {
            manager.PlaySoundEffect(uiClick);
            status = "UI click requested.";
        }

        GUI.enabled = manager != null && !sceneTestStarted;
        if (GUILayout.Button("Change scene and try a duplicate"))
        {
            sceneTestStarted = true;
            StartCoroutine(CheckDuplicateAfterSceneChange());
        }

        GUI.enabled = true;
        if (backgroundMusic == null || uiClick == null)
        {
            GUILayout.Label("A test clip is missing. Its playback button is disabled.");
        }

        GUILayout.Label(status);
        GUILayout.EndArea();
    }

    private IEnumerator CheckDuplicateAfterSceneChange()
    {
        status = "Changing to a temporary scene...";
        Scene originalScene = SceneManager.GetActiveScene();
        Scene temporaryScene = SceneManager.CreateScene("AudioManager Duplicate Test");
        SceneManager.SetActiveScene(temporaryScene);
        yield return SceneManager.UnloadSceneAsync(originalScene);

        AudioManager duplicate = Instantiate(managerPrefab);
        yield return null;

        bool passed = duplicate == null && manager != null &&
            AudioManager.Instance == manager &&
            FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length == 1;
        status = passed
            ? "PASS: original manager survived and the duplicate was destroyed."
            : "FAIL: expected only the original AudioManager to survive.";
    }
}
