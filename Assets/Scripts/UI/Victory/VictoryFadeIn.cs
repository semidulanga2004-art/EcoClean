using System.Collections;
using UnityEngine;

namespace EcoClean.Victory
{
    /// <summary>Fades a CanvasGroup in when the scene starts. Put on the main panel.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VictoryFadeIn : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;

        private IEnumerator Start()
        {
            var group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            group.alpha = 1f;
        }
    }
}
