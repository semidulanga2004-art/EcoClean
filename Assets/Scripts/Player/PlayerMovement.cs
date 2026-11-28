using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    // Swimming starts this far below the float height (about chest-deep water)...
    private const float SwimStartMargin = 0.45f;
    // ...and standing up is offered where the ground is at most this far below it, so standing never
    // lands deep enough to start swimming again.
    private const float StandUpMaxDepth = 0.35f;

    private readonly RaycastHit[] groundHits = new RaycastHit[8];

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("At the shore while swimming: stand up and walk onto the beach.")]
    [FormerlySerializedAs("swimUpAction")]
    [SerializeField] private InputActionReference standUpAction;
    [Tooltip("While swimming: toggle diving down to walk on the sea floor, and back up.")]
    [FormerlySerializedAs("swimDownAction")]
    [SerializeField] private InputActionReference diveAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;

    [Header("Swimming")]
    [SerializeField, Min(0f)] private float swimSpeed = 3f;
    [Tooltip("How far below the water surface the feet float, so head, chest and arms stay above water.")]
    [SerializeField, Min(0f)] private float floatDepth = 0.95f;
    [SerializeField, Min(0f)] private float surfacingSpeed = 1.8f;

    [Header("Diving")]
    [Tooltip("Walking speed on the sea floor while diving.")]
    [SerializeField, Min(0f)] private float seabedWalkSpeed = 2.2f;
    [Tooltip("Slow underwater sinking speed (m/s) on the way down to the sea floor.")]
    [SerializeField, Min(0f)] private float sinkSpeed = 2.5f;

    [Header("Climbing")]
    [Tooltip("Speed of scrambling up objects marked Climbable (rocks).")]
    [SerializeField, Min(0f)] private float climbSpeed = 2.5f;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float animatorDamping = 0.1f;

    private CharacterController characterController;
    private Transform cachedCameraTransform;
    private float verticalVelocity;
    private bool warnedAboutMoveAction;
    private bool warnedAboutCamera;
    private bool warnedAboutAnimator;

    private bool touchingClimbable;
    private Vector3 climbableNormal;

    public bool IsSwimming { get; private set; }
    public bool IsDiving { get; private set; }
    // Swimming at the surface with shallow ground ahead: the stand-up button walks onto the beach.
    public bool CanStandUp { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            WarnAboutAnimator();
        }

        cachedCameraTransform = cameraTransform != null
            ? cameraTransform
            : Camera.main != null ? Camera.main.transform : null;

        if (cachedCameraTransform == null)
        {
            WarnAboutCamera();
        }
    }

    private void Update()
    {
        Vector2 moveInput = ReadMoveInput();
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;
        Transform movementCamera = GetMovementCamera();

        if (movementCamera != null)
        {
            cameraForward = Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up);
            if (cameraForward.sqrMagnitude > 0.0001f)
            {
                cameraForward.Normalize();
                cameraRight = Vector3.Cross(Vector3.up, cameraForward);
            }
            else
            {
                // A camera looking nearly straight up or down has no useful horizontal forward.
                cameraForward = Vector3.forward;
                cameraRight = Vector3.right;
            }
        }

        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
        UpdateSwimState();

        if (IsSwimming)
        {
            if (WasPressed(diveAction))
            {
                IsDiving = !IsDiving;
                verticalVelocity = 0f;
            }

            RotateToward(moveDirection);
            UpdateAnimator(moveInput.magnitude);

            if (IsDiving)
            {
                CanStandUp = false;
                WalkOnSeabed(moveDirection * seabedWalkSpeed);
                return;
            }

            if (!(CanStandUp && WasPressed(standUpAction)))
            {
                SwimAtSurface(moveDirection * swimSpeed);
                return;
            }

            // Stand up in the shallows and walk onto the beach.
            IsSwimming = false;
            CanStandUp = false;
            verticalVelocity = 0f;
        }

        CanStandUp = false;
        RotateToward(moveDirection);
        UpdateAnimator(moveInput.magnitude);
        if (IsClimbing(moveDirection))
        {
            verticalVelocity = climbSpeed;
        }

        MoveWithGravity(moveDirection * moveSpeed);
    }

    private void UpdateSwimState()
    {
        float feetY = transform.position.y;
        if (!WaterZone.TryGetSurface(transform.position, out float surfaceY))
        {
            IsSwimming = false;
            IsDiving = false;
            return;
        }

        float floatY = surfaceY - floatDepth;
        if (!IsSwimming && feetY < floatY - SwimStartMargin)
        {
            // About chest-deep: start swimming at the surface.
            IsSwimming = true;
            IsDiving = false;
            verticalVelocity = 0f;
        }
        else if (IsSwimming && !IsDiving && feetY > floatY + 0.6f)
        {
            // Lifted clearly out of the water (pier, boat, teleport): walk or fall again.
            IsSwimming = false;
        }
        else if (IsDiving && feetY > floatY + 0.05f && characterController.isGrounded)
        {
            // Walked along the sea floor up into shallow water: the dive is over.
            IsSwimming = false;
            IsDiving = false;
        }
    }

    // Float with head, chest and arms above water. The beach cannot be swum onto: at the shore the
    // player stops and may stand up instead.
    private void SwimAtSurface(Vector3 velocity)
    {
        WaterZone.TryGetSurface(transform.position, out float surfaceY);
        float floatY = surfaceY - floatDepth;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float offset = floatY - transform.position.y;
        velocity.y = Mathf.Clamp(offset / dt, -surfacingSpeed, surfacingSpeed);

        Vector3 position = transform.position;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude > 0.0001f &&
            GroundHeight(position + horizontal.normalized * 0.6f) > floatY - 0.05f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        CanStandUp = GroundHeight(position) > floatY - StandUpMaxDepth ||
                     (horizontal.sqrMagnitude > 0.0001f && velocity.x == 0f && velocity.z == 0f);

        characterController.Move(velocity * dt);
    }

    private void WalkOnSeabed(Vector3 horizontalVelocity)
    {
        float dt = Time.deltaTime;
        if (IsClimbing(horizontalVelocity))
        {
            verticalVelocity = climbSpeed * 0.6f;
        }
        else if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }
        else
        {
            // Sink slowly through the water down to the sea floor.
            verticalVelocity = Mathf.Max(verticalVelocity + gravity * 0.3f * dt, -sinkSpeed);
        }

        characterController.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
    }

    private bool IsClimbing(Vector3 moveDirection)
    {
        bool climbing = touchingClimbable && moveDirection.sqrMagnitude > 0.01f &&
            Vector3.Dot(moveDirection.normalized, -new Vector3(climbableNormal.x, 0f, climbableNormal.z).normalized) > 0.3f;
        touchingClimbable = false; // refreshed by OnControllerColliderHit during this frame's Move
        return climbing;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Steep faces of Climbable objects (rocks) can be scrambled up instead of blocking the player.
        if (hit.normal.y < 0.7f && hit.collider.GetComponentInParent<Climbable>() != null)
        {
            touchingClimbable = true;
            climbableNormal = hit.normal;
        }
    }

    // Highest solid surface below a point, ignoring the player's own collider.
    private float GroundHeight(Vector3 point)
    {
        int count = Physics.RaycastNonAlloc(point + Vector3.up * 3f, Vector3.down, groundHits, 40f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        float best = float.NegativeInfinity;
        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].collider != characterController && groundHits[i].point.y > best)
            {
                best = groundHits[i].point.y;
            }
        }

        return best;
    }

    private void OnDisable()
    {
        // Disabled while riding the boat (or paused): stand idle instead of freezing mid-walk.
        if (animator != null)
        {
            animator.SetFloat(SpeedParameter, 0f);
        }
    }

    private static bool WasPressed(InputActionReference reference)
    {
        return reference != null && reference.action != null && reference.action.WasPressedThisFrame();
    }

    private Vector2 ReadMoveInput()
    {
        if (moveAction == null || moveAction.action == null)
        {
            if (!warnedAboutMoveAction)
            {
                Debug.LogWarning(
                    "PlayerMovement has no Move InputActionReference. Assign the existing Player/Move action.",
                    this);
                warnedAboutMoveAction = true;
            }

            return Vector2.zero;
        }

        // The referenced action is owned by its input-map owner (for example PlayerInput).
        // This component only reads it; it never enables or disables a potentially shared action.
        if (!moveAction.action.enabled)
        {
            if (!warnedAboutMoveAction)
            {
                Debug.LogWarning(
                    "PlayerMovement's Move action is disabled. Enable its Player action map from its input owner.",
                    this);
                warnedAboutMoveAction = true;
            }

            return Vector2.zero;
        }

        return moveAction.action.ReadValue<Vector2>();
    }

    private Transform GetMovementCamera()
    {
        if (cameraTransform != null)
        {
            cachedCameraTransform = cameraTransform;
            return cameraTransform;
        }

        if (cachedCameraTransform != null)
        {
            return cachedCameraTransform;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cachedCameraTransform = mainCamera.transform;
            return cachedCameraTransform;
        }

        WarnAboutCamera();
        return null;
    }

    private void MoveWithGravity(Vector3 horizontalVelocity)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            // Keep a small downward force so the controller stays grounded on slopes.
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void RotateToward(Vector3 moveDirection)
    {
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void UpdateAnimator(float moveAmount)
    {
        if (animator == null)
        {
            if (!warnedAboutAnimator)
            {
                WarnAboutAnimator();
            }

            return;
        }

        animator.SetFloat(SpeedParameter, moveAmount, animatorDamping, Time.deltaTime);
    }

    private void WarnAboutCamera()
    {
        if (warnedAboutCamera)
        {
            return;
        }

        Debug.LogWarning(
            "PlayerMovement has no assigned camera and no active MainCamera. Using world-relative movement until a camera is available.",
            this);
        warnedAboutCamera = true;
    }

    private void WarnAboutAnimator()
    {
        if (warnedAboutAnimator)
        {
            return;
        }

        Debug.LogWarning(
            "PlayerMovement could not find an Animator. Movement will continue without updating the Speed parameter.",
            this);
        warnedAboutAnimator = true;
    }
}
