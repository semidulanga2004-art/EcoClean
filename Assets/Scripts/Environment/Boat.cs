using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// A drivable boat that stays on the water surface. Hold Interact near the seat to board or leave.
[RequireComponent(typeof(Rigidbody))]
public sealed class Boat : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    [Header("References")]
    [SerializeField] private Transform seat;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform hullVisual;
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private AudioSource motorAudio;

    [Header("Driving")]
    [SerializeField, Min(0f)] private float boardRange = 4f;
    [SerializeField, Min(0f)] private float maxSpeed = 10f;
    [SerializeField, Min(0f)] private float acceleration = 4f;
    [SerializeField, Min(0f)] private float turnSpeed = 70f;

    private Rigidbody body;
    private PlayerMovement player;
    private bool driving;
    private float speed;
    private bool showingPrompt;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezePositionY |
                           RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationZ;
        player = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update()
    {
        if (player == null || seat == null)
        {
            return;
        }

        bool inRange = !driving &&
            Vector3.Distance(player.transform.position, seat.position) <= boardRange;

        if (interactAction != null && interactAction.action.WasPerformedThisFrame())
        {
            if (driving)
            {
                Leave();
            }
            else if (inRange)
            {
                Board();
            }
        }

        UpdatePrompt(driving ? "Hold E to leave the boat" : inRange ? "Hold E to board the boat" : null);

        UpdateMotorSound();

        if (hullVisual != null)
        {
            float t = Time.time;
            hullVisual.localPosition = new Vector3(0f, Mathf.Sin(t * 1.3f) * 0.06f, 0f);
            hullVisual.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.9f) * 1.5f, 0f, Mathf.Sin(t * 1.1f) * 2.5f);
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = driving && moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        speed = Mathf.MoveTowards(speed, input.y * maxSpeed, acceleration * Time.fixedDeltaTime);

        Vector3 velocity = transform.forward * speed;
        velocity.y = 0f;
        body.linearVelocity = velocity;

        // Steering needs some forward motion, like a real boat; reversing flips the turn.
        float steer = input.x * turnSpeed * Mathf.Clamp(speed / 2f, -1f, 1f);
        body.angularVelocity = new Vector3(0f, steer * Mathf.Deg2Rad, 0f);
    }

    private void Board()
    {
        driving = true;
        player.enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.SetParent(seat, false);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
    }

    private void Leave()
    {
        driving = false;
        Transform exit = exitPoint != null ? exitPoint : seat;
        player.transform.SetParent(null, true);
        player.transform.SetPositionAndRotation(exit.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
        player.GetComponent<CharacterController>().enabled = true;
        player.enabled = true;
    }

    // Idles while someone is aboard and revs up with speed; silent when empty.
    private void UpdateMotorSound()
    {
        if (motorAudio == null)
        {
            return;
        }

        float throttle = Mathf.Abs(speed) / Mathf.Max(0.01f, maxSpeed);
        float targetVolume = driving ? 0.35f + 0.5f * throttle : 0f;
        motorAudio.volume = Mathf.MoveTowards(motorAudio.volume, targetVolume, Time.deltaTime);
        motorAudio.pitch = 0.8f + 0.7f * throttle;

        if (motorAudio.volume > 0f && !motorAudio.isPlaying)
        {
            motorAudio.Play();
        }
        else if (motorAudio.volume <= 0f && motorAudio.isPlaying)
        {
            motorAudio.Stop();
        }
    }

    private void UpdatePrompt(string message)
    {
        if (prompt == null)
        {
            return;
        }

        if (message != null)
        {
            prompt.text = message;
            prompt.gameObject.SetActive(true);
            showingPrompt = true;
        }
        else if (showingPrompt)
        {
            prompt.gameObject.SetActive(false);
            showingPrompt = false;
        }
    }
}
