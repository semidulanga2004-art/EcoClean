using TMPro;
using UnityEngine;

// Tells the player which key does what while in the water.
public sealed class SwimPrompt : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private TMP_Text label;

    private string shown;

    private void Update()
    {
        string message = null;
        if (player != null && player.isActiveAndEnabled && player.IsSwimming)
        {
            message = player.IsDiving ? "Diving  •  Press C to come up"
                : player.CanStandUp ? "Press Space to walk onto the beach"
                : "Press C to dive";
        }

        if (message == shown || label == null)
        {
            return;
        }

        shown = message;
        label.gameObject.SetActive(message != null);
        if (message != null)
        {
            label.text = message;
        }
    }
}
