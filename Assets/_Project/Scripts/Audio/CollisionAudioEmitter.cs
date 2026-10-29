using UnityEngine;

/// <summary>
/// Decoupled collision audio trigger.
/// Hooks into physics collisions (OnCollisionEnter) and CharacterController impacts (OnControllerColliderHit),
/// scales impact volume dynamically based on relative impact velocity, and enforces a cooldown interval.
/// </summary>
public class CollisionAudioEmitter : MonoBehaviour
{
    [Header("Sound Event")]
    [SerializeField] private SoundDataSO impactSound;
    [SerializeField] private string soundId = "SFX_Impact";

    [Header("Velocity Thresholds")]
    [Tooltip("Impact speed below which no sound is played.")]
    [SerializeField] private float minImpactVelocity = 1.8f;

    [Tooltip("Impact speed at which volume reaches maximum.")]
    [SerializeField] private float maxImpactVelocity = 12.0f;

    [Header("Volume Scaling")]
    [SerializeField, Range(0.05f, 1f)] private float minVolumeScale = 0.2f;
    [SerializeField, Range(0.1f, 1f)] private float maxVolumeScale = 1.0f;

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime = 0.08f;

    private float _lastImpactTime = float.NegativeInfinity;

    private void OnCollisionEnter(Collision collision)
    {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactVelocity)
            return;

        Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        TriggerImpactAudio(speed, contactPoint);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // For CharacterController, evaluate downward or lateral impact speed
        float speed = hit.controller.velocity.magnitude;
        if (speed < minImpactVelocity)
            return;

        TriggerImpactAudio(speed, hit.point);
    }

    private void TriggerImpactAudio(float impactSpeed, Vector3 contactPoint)
    {
        if (Time.time - _lastImpactTime < cooldownTime)
            return;

        _lastImpactTime = Time.time;

        float t = Mathf.InverseLerp(minImpactVelocity, maxImpactVelocity, impactSpeed);
        float volumeScale = Mathf.Lerp(minVolumeScale, maxVolumeScale, t);

        if (impactSound != null)
        {
            AudioManager.Instance?.PlaySoundAtPosition(impactSound, contactPoint, volumeScale);
        }
        else if (!string.IsNullOrWhiteSpace(soundId))
        {
            AudioManager.Instance?.PlaySoundAtPosition(soundId, contactPoint, volumeScale);
        }
    }
}
