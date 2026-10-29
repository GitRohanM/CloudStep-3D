using UnityEngine;

/// <summary>
/// Smoothly oscillates a platform between two endpoints and carries any CharacterController riding it.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 moveDelta = new Vector3(0f, 0f, 8f);
    [SerializeField] private float travelDuration = 3.5f;
    [SerializeField] private float waitDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private bool enableHumAudio = true;

    private Vector3 startPos;
    private Vector3 targetPos;
    private CharacterController ridingPlayer;
    private float timer;
    private bool movingToTarget = true;
    private float waitTimer;
    private AudioSource _humAudioSource;
    private float _baseHumVolume = 0.4f;

    public Vector3 MoveDelta { get => moveDelta; set => moveDelta = value; }
    public float TravelDuration { get => travelDuration; set => travelDuration = value; }
    public float WaitDuration { get => waitDuration; set => waitDuration = value; }

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveDelta;

        if (enableHumAudio && AudioManager.Instance != null && AudioManager.Instance.TryGetSound("SFX_Platform_Hum", out var humData))
        {
            _humAudioSource = gameObject.AddComponent<AudioSource>();
            _humAudioSource.clip = humData.GetRandomClip();
            _humAudioSource.outputAudioMixerGroup = humData.MixerGroup ?? AudioManager.Instance.SFXGroup;
            _humAudioSource.loop = true;
            _humAudioSource.spatialBlend = 1f;
            _humAudioSource.minDistance = 1f;
            _humAudioSource.maxDistance = 6f;
            _baseHumVolume = 0.18f;
            _humAudioSource.volume = _baseHumVolume;
            _humAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _humAudioSource.Play();
        }
    }

    private void Update()
    {
        Vector3 previousPosition = transform.position;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            timer += Time.deltaTime / Mathf.Max(travelDuration, 0.1f);
            float t = Mathf.Clamp01(timer);
            // Smooth ease in and out
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 from = movingToTarget ? startPos : targetPos;
            Vector3 to = movingToTarget ? targetPos : startPos;
            transform.position = Vector3.Lerp(from, to, smoothT);

            if (t >= 1f)
            {
                timer = 0f;
                movingToTarget = !movingToTarget;
                waitTimer = waitDuration;
            }
        }

        Vector3 delta = transform.position - previousPosition;
        if (_humAudioSource != null)
        {
            float targetHum = waitTimer > 0f ? _baseHumVolume * 0.25f : _baseHumVolume;
            _humAudioSource.volume = Mathf.MoveTowards(_humAudioSource.volume, targetHum, Time.deltaTime * 2f);
        }

        if (ridingPlayer != null && ridingPlayer.enabled && delta.sqrMagnitude > 0.0000001f)
        {
            ridingPlayer.Move(delta);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            ridingPlayer = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc == ridingPlayer)
            {
                ridingPlayer = null;
            }
        }
    }
}
