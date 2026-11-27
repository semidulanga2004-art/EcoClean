using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AudioManagerPlayModeTest : MonoBehaviour
{
    [SerializeField] private AudioManager managerPrefab;

    private AudioManager manager;
    private bool sceneTestStarted;
    private float musicVolume = 0.45f;
    private float ambienceVolume = 0.22f;
    private float sfxVolume = 0.75f;
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
        GUILayout.BeginArea(new Rect(16, 16, 480, 510), GUI.skin.box);
        GUILayout.Label("AudioManager Play Mode test");

        GUI.enabled = manager != null;
        if (GUILayout.Button("Start music"))
        {
            manager.PlayMusic();
            status = "Music requested.";
        }

        GUI.enabled = manager != null;
        if (GUILayout.Button("Stop music"))
        {
            manager.StopMusic();
            status = "Music stopped.";
        }

        if (GUILayout.Button("Start ambience"))
        {
            manager.PlayAmbience();
            status = "Ambience requested.";
        }

        if (GUILayout.Button("Stop ambience"))
        {
            manager.StopAmbience();
            status = "Ambience stopped.";
        }

        DrawVolumeSlider("Music volume", ref musicVolume, manager == null ? null : manager.SetMusicVolume);
        DrawVolumeSlider("Ambience volume", ref ambienceVolume, manager == null ? null : manager.SetAmbienceVolume);

        DrawVolumeSlider("SFX volume", ref sfxVolume, manager == null ? null : manager.SetSfxVolume);

        if (GUILayout.Button("Play UI click"))
        {
            manager.PlayUIClick();
            status = "UI click requested.";
        }

        if (GUILayout.Button("Play correct sort"))
        {
            manager.PlayCorrectSort();
            status = "Correct sort requested.";
        }

        if (GUILayout.Button("Play incorrect sort"))
        {
            manager.PlayIncorrectSort();
            status = "Incorrect sort requested.";
        }

        GUI.enabled = manager != null && !sceneTestStarted;
        if (GUILayout.Button("Change scene and try a duplicate"))
        {
            sceneTestStarted = true;
            StartCoroutine(CheckDuplicateAfterSceneChange());
        }

        GUI.enabled = true;
        GUILayout.Label(status);
        GUILayout.EndArea();
    }

    private static void DrawVolumeSlider(string label, ref float volume, System.Action<float> apply)
    {
        GUILayout.Label(label);
        float next = GUILayout.HorizontalSlider(volume, 0f, 1f);
        if (next != volume)
        {
            volume = next;
            apply?.Invoke(volume);
        }
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
