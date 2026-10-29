using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera-relative platform movement with buffered jumps and coyote time.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 6f;
    [Min(0f)] public float acceleration = 20f;
    [Min(0f)] public float rotationSpeed = 720f;

    [Header("Jump")]
    [Min(0.1f)] public float jumpHeight = 2.6f;
    public float gravity = -20f;
    [Min(0f)] public float coyoteTime = 0.1f;
    [Min(0f)] public float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    [Min(0.01f)] public float groundCheckRadius = 0.2f;
    public float groundCheckOffset = 0.12f;
    public LayerMask groundLayers = ~(1 << 2);

    [Header("References")]
    public Transform cameraTransform;
    public PlayerAnimationDriver animationDriver;

    public event System.Action OnJumped;
    public event System.Action<float> OnLanded;

    private CharacterController _characterController;
    private CameraFollow _cameraFollow;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;
    private float _lastGroundedTime = float.NegativeInfinity;
    private float _lastJumpPressedTime = float.NegativeInfinity;
    private bool _wasGroundedLastFrame = true;
    private bool _hasTriggeredFallAudio = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (cameraTransform != null)
            _cameraFollow = cameraTransform.GetComponent<CameraFollow>();
        if (animationDriver == null)
            animationDriver = GetComponentInChildren<PlayerAnimationDriver>();

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        _moveAction.AddBinding("<Gamepad>/leftStick");

        _jumpAction = new InputAction("Jump", InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _jumpAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();
    }

    private void OnDestroy()
    {
        _moveAction?.Dispose();
        _jumpAction?.Dispose();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        Vector2 input = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector3 moveDirection = GetCameraRelativeMoveDirection(input);
        float inputAmount = Mathf.Clamp01(input.magnitude);
        Vector3 targetVelocity = moveDirection * (moveSpeed * inputAmount);
        _horizontalVelocity = Vector3.MoveTowards(
            _horizontalVelocity, targetVelocity, acceleration * deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }

        if (_jumpAction != null && _jumpAction.WasPressedThisFrame())
            _lastJumpPressedTime = Time.time;

        bool grounded = _characterController.isGrounded || CheckGrounded();
        if (grounded)
        {
            _lastGroundedTime = Time.time;
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;
        }

        bool jumpBuffered = Time.time - _lastJumpPressedTime <= jumpBufferTime;
        bool withinCoyoteTime = Time.time - _lastGroundedTime <= coyoteTime;
        bool jumpedThisFrame = jumpBuffered && withinCoyoteTime;
        if (jumpedThisFrame)
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _lastJumpPressedTime = float.NegativeInfinity;
            _lastGroundedTime = float.NegativeInfinity;
            grounded = false;
            _wasGroundedLastFrame = false;
            animationDriver?.PlayJump();
            OnJumped?.Invoke();
        }

        if (!grounded)
            _verticalVelocity += gravity * deltaTime;

        float preMoveVerticalVelocity = _verticalVelocity;
        Vector3 velocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
        _characterController.Move(velocity * deltaTime);

        bool groundedAfterMove = _characterController.isGrounded || CheckGrounded();
        if (groundedAfterMove)
        {
            _hasTriggeredFallAudio = false;

            if (!_wasGroundedLastFrame && preMoveVerticalVelocity < -1f)
            {
                OnLanded?.Invoke(Mathf.Abs(preMoveVerticalVelocity));
            }

            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
                _lastGroundedTime = Time.time;
            }
        }
        else
        {
            // Player has tumbled off the platform edge into the void
            if (!_hasTriggeredFallAudio && transform.position.y < -1.8f)
            {
                _hasTriggeredFallAudio = true;
                AudioManager.Instance?.PlaySound("SFX_Player_Fall");
            }

            // Guaranteed safety net if player falls outside KillZone trigger boundaries
            if (transform.position.y < -12.0f && GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied(this);
            }
        }
        _wasGroundedLastFrame = groundedAfterMove;

        float normalizedSpeed = moveSpeed > 0f ? _horizontalVelocity.magnitude / moveSpeed : 0f;
        animationDriver?.SetMovement(normalizedSpeed, groundedAfterMove);
    }

    private bool CheckGrounded()
    {
        Vector3 checkPosition = transform.position + Vector3.up * groundCheckOffset;
        return Physics.CheckSphere(
            checkPosition, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetCameraRelativeMoveDirection(Vector2 input)
    {
        // Clamp input before combining axes so keyboard diagonals and stick diagonals
        // keep the same top speed as straight movement.
        input = Vector2.ClampMagnitude(input, 1f);
        if (input.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 forward = _cameraFollow != null
            ? _cameraFollow.MovementForward
            : cameraTransform != null
                ? Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * Vector3.forward
                : Vector3.forward;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        // Build the side axis from the same flattened forward vector. This keeps the
        // movement basis orthogonal even when the camera is pitched or colliding.
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 direction = forward * input.y + right * input.x;
        return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.zero;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        acceleration = Mathf.Max(0f, acceleration);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        jumpHeight = Mathf.Max(0.1f, jumpHeight);
        if (gravity >= -0.01f) gravity = -20f;
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        groundCheckRadius = Mathf.Max(0.01f, groundCheckRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius);
    }
}
