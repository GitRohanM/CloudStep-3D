using UnityEngine;

/// <summary>
/// Audio driver for the player character.
/// Subscribes to PlayerController movement state to drive footsteps, jump vocatives/whooshes,
/// and landing impacts with zero allocation.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerAudioDriver : MonoBehaviour
{
    [Header("Sound IDs")]
    [SerializeField] private string jumpSoundId = "SFX_Player_Jump";
    [SerializeField] private string landSoundId = "SFX_Player_Land";
    [SerializeField] private string footstepSoundId = "SFX_Player_Footstep";

    [Header("Direct Sound Data References")]
    [SerializeField] private SoundDataSO jumpSound;
    [SerializeField] private SoundDataSO landSound;
    [SerializeField] private SoundDataSO footstepSound;

    [Header("Footstep Parameters")]
    [Tooltip("Distance in meters traveled per footstep sound.")]
    [SerializeField] private float stepDistance = 1.8f;
    [SerializeField] private float minMoveSpeed = 0.5f;

    [Header("Landing Impact Parameters")]
    [SerializeField] private float minLandVelocity = 3.0f;
    [SerializeField] private float maxLandVelocity = 15.0f;

    private CharacterController _characterController;
    private PlayerController _playerController;
    private float _distanceTraveled;
    private bool _wasGrounded = true;
    private float _previousVerticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (_playerController != null)
        {
            _playerController.OnJumped += HandleJump;
            _playerController.OnLanded += HandleLand;
        }
    }

    private void OnDisable()
    {
        if (_playerController != null)
        {
            _playerController.OnJumped -= HandleJump;
            _playerController.OnLanded -= HandleLand;
        }
    }

    private void Update()
    {
        if (_characterController == null)
            return;

        Vector3 vel = _characterController.velocity;
        Vector3 horizontalVel = new Vector3(vel.x, 0f, vel.z);
        float speed = horizontalVel.magnitude;
        bool grounded = _characterController.isGrounded;

        // Grounded footstep stride tracking
        if (grounded && speed >= minMoveSpeed)
        {
            _distanceTraveled += speed * Time.deltaTime;
            if (_distanceTraveled >= stepDistance)
            {
                _distanceTraveled = 0f;
                PlayFootstep();
            }
        }
        else
        {
            _distanceTraveled = Mathf.Min(_distanceTraveled, stepDistance * 0.5f);
        }

        // Fallback ground state tracking if events are not wired
        if (_playerController == null)
        {
            if (!_wasGrounded && grounded)
            {
                HandleLand(Mathf.Abs(_previousVerticalVelocity));
            }
        }

        _wasGrounded = grounded;
        _previousVerticalVelocity = vel.y;
    }

    public void HandleJump()
    {
        if (jumpSound != null)
            AudioManager.Instance?.PlaySoundAtPosition(jumpSound, transform.position);
        else if (!string.IsNullOrWhiteSpace(jumpSoundId))
            AudioManager.Instance?.PlaySoundAtPosition(jumpSoundId, transform.position);
    }

    public void HandleLand(float impactVelocity)
    {
        float t = Mathf.InverseLerp(minLandVelocity, maxLandVelocity, impactVelocity);
        float volumeScale = Mathf.Lerp(0.35f, 1.0f, t);

        if (landSound != null)
            AudioManager.Instance?.PlaySoundAtPosition(landSound, transform.position, volumeScale);
        else if (!string.IsNullOrWhiteSpace(landSoundId))
            AudioManager.Instance?.PlaySoundAtPosition(landSoundId, transform.position, volumeScale);
    }

    private void PlayFootstep()
    {
        if (footstepSound != null)
            AudioManager.Instance?.PlaySoundAtPosition(footstepSound, transform.position, 0.6f);
        else if (!string.IsNullOrWhiteSpace(footstepSoundId))
            AudioManager.Instance?.PlaySoundAtPosition(footstepSoundId, transform.position, 0.6f);
    }
}
