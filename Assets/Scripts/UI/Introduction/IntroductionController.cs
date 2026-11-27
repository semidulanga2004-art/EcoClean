using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroductionController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private bool isStarting;

    private void Start()
    {
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = false; // don't block button clicks
            fadeGroup.alpha = 1f;
            StartCoroutine(Fade(1f, 0f, fadeInDuration));
        }
    }

    private void Update()
    {
        if (isStarting) return;

        if (StartKeyPressed())
        {
            StartLevel();
        }
    }

    private bool StartKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null &&
               (kb.spaceKey.wasPressedThisFrame ||
                kb.enterKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }

    // Hooked to the Start button's OnClick and also called by Space/Enter
    public void StartLevel()
    {
        if (isStarting) return;

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogWarning("Scene '" + gameplaySceneName +
                "' is not in the Build Profile scene list. " +
                "The project lead must add it (File > Build Profiles).");
            return;
        }

        isStarting = true;

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true; // stop extra clicks during fade
            yield return Fade(fadeGroup.alpha, 1f, fadeOutDuration);
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}