using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smooth third-person camera that follows a target and pulls in around level geometry.
/// </summary>
public sealed class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float lookAtHeight = 1.1f;
    [SerializeField, Min(0.1f)] private float followDistance = 6.36f;

    [Header("Follow")]
    [SerializeField, Min(0f)] private float smoothTime = 0.12f;

    [Header("Camera Look")]
    [SerializeField, Range(-80f, 80f)] private float startingPitch = 18.2f;
    [SerializeField, Range(0f, 1f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Min(0f)] private float gamepadLookSpeed = 140f;
    [SerializeField, Range(-20f, 5f)] private float minPitch = -5f;
    [SerializeField, Range(10f, 85f)] private float maxPitch = 65f;

    [Header("Collision")]
    [SerializeField, Min(0.01f)] private float collisionRadius = 0.24f;
    [SerializeField, Min(0f)] private float collisionPadding = 0.12f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private Vector3 positionVelocity;
    private float yaw;
    private float initialYaw;
    private float pitch;
    private InputAction mouseLookAction;
    private InputAction gamepadLookAction;
    private bool lookInputEnabled;
    private bool suppressNextLookSample;

    /// <summary>Stable movement axes: the player turning never rotates the camera boom.</summary>
    public Vector3 MovementForward => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    public Vector3 MovementRight => Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private void Awake()
    {
        yaw = transform.eulerAngles.y;
        initialYaw = yaw;
        pitch = startingPitch;
        mouseLookAction = new InputAction("Camera Look Mouse", InputActionType.Value, "<Mouse>/delta");
        gamepadLookAction = new InputAction("Camera Look Stick", InputActionType.Value, "<Gamepad>/rightStick");
    }

    public void ResetOrientation()
    {
        yaw = initialYaw;
        pitch = startingPitch;
        positionVelocity = Vector3.zero;
        suppressNextLookSample = true;
    }

    public void SetLookInputEnabled(bool enabled)
    {
        lookInputEnabled = enabled;
        if (enabled)
        {
            // Don't apply the mouse movement used to click Start/Retry as camera input.
            suppressNextLookSample = true;
            mouseLookAction?.Enable();
            gamepadLookAction?.Enable();
        }
        else
        {
            suppressNextLookSample = false;
            mouseLookAction?.Disable();
            gamepadLookAction?.Disable();
        }
    }

    private void OnDestroy()
    {
        mouseLookAction?.Dispose();
        gamepadLookAction?.Dispose();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (lookInputEnabled)
        {
            Vector2 mouseDelta = mouseLookAction.ReadValue<Vector2>();
            Vector2 stick = gamepadLookAction.ReadValue<Vector2>();
            if (suppressNextLookSample)
            {
                mouseDelta = Vector2.zero;
                stick = Vector2.zero;
                suppressNextLookSample = false;
            }
            yaw += mouseDelta.x * mouseSensitivity + stick.x * gamepadLookSpeed * Time.unscaledDeltaTime;
            pitch = Mathf.Clamp(
                pitch - mouseDelta.y * mouseSensitivity - stick.y * gamepadLookSpeed * Time.unscaledDeltaTime,
                minPitch,
                maxPitch);
        }

        Vector3 focusPoint = target.position + Vector3.up * lookAtHeight;
        // Build the boom from camera input, not target rotation. A/D turns the character
        // without swinging the camera; only mouse or right-stick input orbits the view.
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = focusPoint + orbit * (Vector3.back * followDistance);
        Vector3 toDesired = desiredPosition - focusPoint;
        float desiredDistance = toDesired.magnitude;

        if (desiredDistance > 0.001f)
        {
            Vector3 direction = toDesired / desiredDistance;
            float allowedDistance = FindAllowedDistance(focusPoint, direction, desiredDistance);
            Vector3 safePosition = focusPoint + direction * allowedDistance;
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                safePosition,
                ref positionVelocity,
                smoothTime);

            // The damped position can trail behind a rapidly moving safe position, so clamp it too.
            Vector3 smoothedOffset = smoothedPosition - focusPoint;
            float smoothedDistance = smoothedOffset.magnitude;
            if (smoothedDistance > 0.001f)
            {
                Vector3 smoothedDirection = smoothedOffset / smoothedDistance;
                float smoothedAllowedDistance = FindAllowedDistance(focusPoint, smoothedDirection, smoothedDistance);
                if (smoothedAllowedDistance < smoothedDistance)
                {
                    smoothedPosition = focusPoint + smoothedDirection * smoothedAllowedDistance;
                    positionVelocity = Vector3.zero;
                }
            }

            transform.position = smoothedPosition;
        }

        Vector3 lookDirection = focusPoint - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    private float FindAllowedDistance(Vector3 origin, Vector3 direction, float maxDistance)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            collisionRadius,
            direction,
            maxDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore);

        float closestDistance = maxDistance;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.transform == target || hitCollider.transform.IsChildOf(target))
                continue;

            if (hits[i].distance < closestDistance)
                closestDistance = hits[i].distance;
        }

        return closestDistance < maxDistance
            ? Mathf.Max(0.1f, closestDistance - collisionPadding)
            : maxDistance;
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0f, smoothTime);
        followDistance = Mathf.Max(0.1f, followDistance);
        collisionRadius = Mathf.Max(0.01f, collisionRadius);
        collisionPadding = Mathf.Max(0f, collisionPadding);
        maxPitch = Mathf.Max(minPitch, maxPitch);
    }
}
