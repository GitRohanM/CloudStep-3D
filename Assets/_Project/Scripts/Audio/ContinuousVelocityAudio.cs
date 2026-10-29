using UnityEngine;

/// <summary>
/// Smoothly modulates the volume and pitch of a continuous audio loop
/// based on the velocity magnitude of a CharacterController, Rigidbody, or Rigidbody2D.
/// Ideal for wind velocity, thrusters, rolling, or sliding feedback.
/// </summary>
public class ContinuousVelocityAudio : MonoBehaviour
{
    [Header("Sound Event")]
    [SerializeField] private SoundDataSO soundData;
    [SerializeField] private string soundId = "SFX_Wind_Velocity";

    [Header("Velocity Range")]
    [Tooltip("Minimum velocity magnitude before audio begins to fade in.")]
    [SerializeField] private float minVelocity = 7.5f;

    [Tooltip("Velocity magnitude at which volume and pitch reach maximum.")]
    [SerializeField] private float maxVelocity = 18.0f;

    [Header("Modulation Limits")]
    [SerializeField, Range(0f, 1f)] private float minVolume = 0.0f;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 0.35f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.25f;

    [Header("Smoothing")]
    [Tooltip("Smoothing factor for volume and pitch changes to avoid audio popping.")]
    [SerializeField] private float lerpSpeed = 8.0f;

    private CharacterController _characterController;
    private Rigidbody _rigidbody;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _audioSource;
    private float _currentVolume;
    private float _currentPitch;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody2D = GetComponent<Rigidbody2D>();

        // Create a dedicated local AudioSource for continuous loop modulation
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = true;
        _audioSource.spatialBlend = 0f; // Velocity rush is perceived in stereo ears
    }

    private void Start()
    {
        InitializeClip();
    }

    private void OnEnable()
    {
        if (_audioSource != null && _audioSource.clip != null && !_audioSource.isPlaying)
        {
            _audioSource.volume = 0f;
            _audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    private void InitializeClip()
    {
        if (soundData == null && !string.IsNullOrWhiteSpace(soundId) && AudioManager.Instance != null)
        {
            AudioManager.Instance.TryGetSound(soundId, out soundData);
        }

        if (soundData != null)
        {
            _audioSource.clip = soundData.GetRandomClip();
            _audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            _audioSource.spatialBlend = soundData.SpatialBlend;
            _audioSource.volume = 0f;
            _audioSource.pitch = minPitch;
            _audioSource.Play();
        }
    }

    private void Update()
    {
        if (_audioSource == null)
            return;

        if (_audioSource.clip == null)
        {
            InitializeClip();
            if (_audioSource.clip == null) return;
        }

        float speed = GetCurrentSpeed();
        float t = Mathf.InverseLerp(minVelocity, maxVelocity, speed);

        float targetVolume = Mathf.Lerp(minVolume, maxVolume, t);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, t);

        float dt = Time.deltaTime;
        _currentVolume = Mathf.Lerp(_currentVolume, targetVolume, dt * lerpSpeed);
        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, dt * lerpSpeed);

        _audioSource.volume = _currentVolume;
        _audioSource.pitch = _currentPitch;
    }

    private float GetCurrentSpeed()
    {
        if (_characterController != null)
            return _characterController.velocity.magnitude;
        if (_rigidbody != null)
            return _rigidbody.linearVelocity.magnitude;
        if (_rigidbody2D != null)
            return _rigidbody2D.linearVelocity.magnitude;

        return 0f;
    }
}
