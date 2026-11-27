using System.Collections;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(TMP_Text))]
public class TypewriterText : MonoBehaviour
{
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private float startDelay = 0.8f;

    private TMP_Text label;
    private Coroutine routine;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        routine = StartCoroutine(TypeRoutine());
    }

    private void Update()
    {
        if (routine != null && ClickedThisFrame())
        {
            StopCoroutine(routine);
            routine = null;
            label.maxVisibleCharacters = int.MaxValue; // show everything
        }
    }

    private bool ClickedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private IEnumerator TypeRoutine()
    {
        label.ForceMeshUpdate();
        int total = label.textInfo.characterCount;
        label.maxVisibleCharacters = 0;

        yield return new WaitForSecondsRealtime(startDelay);

        float delay = 1f / charactersPerSecond;
        for (int i = 1; i <= total; i++)
        {
            label.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(delay);
        }

        label.maxVisibleCharacters = int.MaxValue;
        routine = null;
    }
}