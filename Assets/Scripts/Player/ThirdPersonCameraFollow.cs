using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TouchControl = UnityEngine.InputSystem.Controls.TouchControl;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(Camera))]
public sealed class ThirdPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Orbit")]
    [SerializeField, Min(0f)] private float cameraDistance = 5f;
    [SerializeField, Range(-89f, 89f)] private float verticalAngle = 13.4f;
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Range(-89f, 89f)] private float minimumVerticalAngle = -8f;
    [SerializeField, Range(-89f, 89f)] private float maximumVerticalAngle = 65f;
    [Tooltip("Look around by moving the mouse (cursor locked; Esc frees it, click to lock again). " +
             "Off: orbit by right-drag, keeping the cursor free for clicking objects.")]
    [SerializeField] private bool freeLook;

    [Header("Water")]
    [Tooltip("Keeps the camera this far from the water surface so the view is never cut by it.")]
    [SerializeField, Min(0f)] private float waterSurfaceClearance = 0.45f;

    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float followSmoothing = 0.15f;
    [SerializeField, Min(0f)] private float rotationSmoothing = 0.12f;

    private Vector3 positionSmoothVelocity;
    private float yaw;
    private int trackedTouchId = -1;
    private Vector2 lastTouchPosition;
    private bool blockTouchInputUntilRelease;
    private bool warnedAboutTarget;

    private void Awake()
    {
        if (target == null)
        {
            WarnAboutTarget();
            return;
        }

        Vector3 planarForward = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        if (planarForward.sqrMagnitude > 0.0001f)
        {
            yaw = Mathf.Atan2(planarForward.x, planarForward.z) * Mathf.Rad2Deg;
        }

        verticalAngle = Mathf.Clamp(verticalAngle, minimumVerticalAngle, maximumVerticalAngle);
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            WarnAboutTarget();
            return;
        }

        bool touchInputActive = ReadTouchOrbit();
        if (!touchInputActive)
        {
            ReadMouseOrbit();
        }

        Vector3 desiredPosition = GetDesiredPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionSmoothVelocity,
            followSmoothing,
            Mathf.Infinity,
            Time.deltaTime);

        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        float rotationBlend = rotationSmoothing <= 0f
            ? 1f
            : 1f - Mathf.Exp(-Time.deltaTime / rotationSmoothing);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
    }

    private void OnEnable()
    {
        if (freeLook)
        {
            SetCursorLocked(true);
        }
    }

    private void OnDisable()
    {
        trackedTouchId = -1;
        blockTouchInputUntilRelease = false;
        if (freeLook)
        {
            SetCursorLocked(false);
        }
    }

    private void ReadMouseOrbit()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (freeLook)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }
            else if (Cursor.lockState != CursorLockMode.Locked && mouse.leftButton.wasPressedThisFrame &&
                     (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                SetCursorLocked(true);
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                ApplyOrbitDelta(mouse.delta.ReadValue());
            }

            return;
        }

        if (mouse.rightButton.isPressed)
        {
            ApplyOrbitDelta(mouse.delta.ReadValue());
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private bool ReadTouchOrbit()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            trackedTouchId = -1;
            blockTouchInputUntilRelease = false;
            return false;
        }

        int activeTouchCount = 0;
        TouchControl activeTouch = null;
        foreach (TouchControl touch in touchscreen.touches)
        {
            TouchPhase phase = touch.phase.ReadValue();
            if (!IsTouchActive(phase))
            {
                continue;
            }

            activeTouchCount++;
            activeTouch = touch;
        }

        if (blockTouchInputUntilRelease)
        {
            if (activeTouchCount == 0)
            {
                blockTouchInputUntilRelease = false;
            }

            return activeTouchCount > 0;
        }

        if (activeTouchCount > 1)
        {
            trackedTouchId = -1;
            blockTouchInputUntilRelease = true;
            return true;
        }

        if (trackedTouchId >= 0)
        {
            TouchControl trackedTouch = FindTouch(touchscreen, trackedTouchId);
            if (trackedTouch == null)
            {
                trackedTouchId = -1;
                return activeTouchCount > 0;
            }

            TouchPhase phase = trackedTouch.phase.ReadValue();
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled || phase == TouchPhase.None)
            {
                trackedTouchId = -1;
                return true;
            }

            if (phase == TouchPhase.Moved)
            {
                Vector2 touchPosition = trackedTouch.position.ReadValue();
                ApplyOrbitDelta(touchPosition - lastTouchPosition);
                lastTouchPosition = touchPosition;
            }

            return true;
        }

        // Only start tracking on the exact began frame. Never take over a touch that
        // began on the left movement area or over interactive UI.
        if (activeTouchCount == 1 && activeTouch != null &&
            activeTouch.phase.ReadValue() == TouchPhase.Began)
        {
            int touchId = activeTouch.touchId.ReadValue();
            Vector2 startPosition = activeTouch.startPosition.ReadValue();
            bool startedOnRightHalf = startPosition.x >= Screen.width * 0.5f;
            bool startedOverUi = EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touchId);

            if (!startedOnRightHalf || startedOverUi)
            {
                blockTouchInputUntilRelease = true;
                return true;
            }

            trackedTouchId = touchId;
            lastTouchPosition = activeTouch.position.ReadValue();
            return true;
        }

        // A touch already in progress without a recorded Began phase is ignored.
        return activeTouchCount > 0;
    }

    private static bool IsTouchActive(TouchPhase phase)
    {
        return phase == TouchPhase.Began || phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
    }

    private static TouchControl FindTouch(Touchscreen touchscreen, int touchId)
    {
        foreach (TouchControl touch in touchscreen.touches)
        {
            if (touch.touchId.ReadValue() == touchId && IsTouchActive(touch.phase.ReadValue()))
            {
                return touch;
            }
        }

        return null;
    }

    private void ApplyOrbitDelta(Vector2 delta)
    {
        yaw += delta.x * mouseSensitivity;
        verticalAngle = Mathf.Clamp(
            verticalAngle - delta.y * mouseSensitivity,
            minimumVerticalAngle,
            maximumVerticalAngle);
    }

    private Vector3 GetDesiredPosition()
    {
        float safeDistance = Mathf.Max(0.01f, cameraDistance);
        float groundHeight = GetGroundHeight();
        float minimumSafeAngle = Mathf.Asin(
            Mathf.Clamp((groundHeight + 0.25f - target.position.y) / safeDistance, -0.99f, 0.99f))
            * Mathf.Rad2Deg;
        float safeVerticalAngle = Mathf.Max(verticalAngle, minimumSafeAngle);
        Quaternion orbitRotation = Quaternion.Euler(safeVerticalAngle, yaw, 0f);
        Vector3 desiredPosition = target.position + orbitRotation * (Vector3.back * safeDistance);

        // Keep the camera above the Player root even if target height or orbit distance changes.
        desiredPosition.y = Mathf.Max(desiredPosition.y, groundHeight + 0.25f);

        // In water, stay on the same side of the surface as the player's head: above it while
        // swimming at the surface, below it while diving.
        if (WaterZone.TryGetSurface(target.position, out float surfaceY))
        {
            // The target sits near the shoulders, just under the surface while floating.
            desiredPosition.y = target.position.y > surfaceY - 0.4f
                ? Mathf.Max(desiredPosition.y, surfaceY + waterSurfaceClearance)
                : Mathf.Min(desiredPosition.y, surfaceY - waterSurfaceClearance);
        }

        return desiredPosition;
    }

    private float GetGroundHeight()
    {
        return target.parent != null
            ? target.parent.position.y
            : target.position.y - target.localPosition.y;
    }

    private void SnapToTarget()
    {
        transform.position = GetDesiredPosition();
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }

        positionSmoothVelocity = Vector3.zero;
    }

    private void WarnAboutTarget()
    {
        if (warnedAboutTarget)
        {
            return;
        }

        Debug.LogWarning(
            "ThirdPersonCameraFollow has no CameraTarget assigned. The camera will remain at its current transform.",
            this);
        warnedAboutTarget = true;
    }
}
