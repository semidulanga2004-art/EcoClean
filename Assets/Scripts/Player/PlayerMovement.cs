using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float animatorDamping = 0.1f;

    private CharacterController characterController;
    private Transform cachedCameraTransform;
    private float verticalVelocity;
    private bool warnedAboutMoveAction;
    private bool warnedAboutCamera;
    private bool warnedAboutAnimator;

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
        Vector3 horizontalVelocity = moveDirection * moveSpeed;

        RotateToward(moveDirection);
        UpdateAnimator(moveInput.magnitude);
        MoveWithGravity(horizontalVelocity);
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
