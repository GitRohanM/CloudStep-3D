using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Data definition for a sound effect or music track with randomized variations,
/// mixer routing, 3D attenuation parameters, and spam-prevention cooldowns.
/// </summary>
[CreateAssetMenu(fileName = "NewSoundData", menuName = "Audio/Sound Data")]
public class SoundDataSO : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("Unique identifier used for AudioManager dictionary lookups.")]
    public string SoundID = string.Empty;

    [Header("Audio Clips")]
    [Tooltip("List of clip variations played randomly to prevent acoustic fatigue.")]
    public AudioClip[] Clips = Array.Empty<AudioClip>();

    [Header("Mixer Routing & Volume")]
    [Tooltip("AudioMixerGroup destination (e.g. Master, BGM, SFX, UI).")]
    public AudioMixerGroup MixerGroup;

    [Range(0f, 1f)]
    [Tooltip("Base volume for playback.")]
    public float Volume = 0.8f;

    [Tooltip("Min and Max pitch randomization range (X = min, Y = max).")]
    public Vector2 PitchVariation = new Vector2(0.92f, 1.08f);

    [Header("3D Spatial Settings")]
    [Range(0f, 1f)]
    [Tooltip("0 = 2D (no panning/attenuation), 1 = full 3D spatial positioning.")]
    public float SpatialBlend = 0f;

    [Tooltip("Min distance (rolloff start) and Max distance (rolloff end) for 3D sounds.")]
    public Vector2 MinMaxDistance = new Vector2(1f, 35f);

    [Header("Playback Behavior")]
    [Tooltip("True if this audio loops continuously until stopped.")]
    public bool Loop = false;

    [Tooltip("Minimum time interval in seconds between consecutive triggers to prevent physics spamming.")]
    public float CooldownTime = 0.05f;

    [NonSerialized]
    private float _lastPlayedTime = float.NegativeInfinity;

    [NonSerialized]
    private int _lastClipIndex = -1;

    /// <summary>Returns true if the sound cooldown has elapsed.</summary>
    public bool CanPlay()
    {
        return Time.unscaledTime - _lastPlayedTime >= CooldownTime;
    }

    /// <summary>Records playback timestamp for cooldown enforcement.</summary>
    public void RecordPlayed()
    {
        _lastPlayedTime = Time.unscaledTime;
    }

    /// <summary>Selects a random AudioClip from the collection without immediate repeat if multiple clips exist.</summary>
    public AudioClip GetRandomClip()
    {
        if (Clips == null || Clips.Length == 0)
            return null;

        if (Clips.Length == 1)
            return Clips[0];

        int index = UnityEngine.Random.Range(0, Clips.Length);
        if (index == _lastClipIndex)
        {
            index = (index + 1) % Clips.Length;
        }

        _lastClipIndex = index;
        return Clips[index];
    }

    /// <summary>Calculates a randomized pitch within the configured range.</summary>
    public float GetRandomPitch()
    {
        float min = Mathf.Min(PitchVariation.x, PitchVariation.y);
        float max = Mathf.Max(PitchVariation.x, PitchVariation.y);
        if (Mathf.Approximately(min, max))
            return min <= 0f ? 1f : min;

        return UnityEngine.Random.Range(min, max);
    }
}
