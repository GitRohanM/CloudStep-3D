using System;
using UnityEngine;

/// <summary>
/// Reusable pooled audio emitter wrapper that hosts an AudioSource,
/// tracks active playback duration, optionally tracks moving parent Transforms,
/// and returns itself to the AudioManager pool with zero GC allocation.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourceEmitter : MonoBehaviour
{
    private AudioSource _source;
    private SoundDataSO _currentData;
    private Transform _followTarget;
    private Action<AudioSourceEmitter> _onRelease;
    private bool _isActive;
    private float _playTimer;
    private float _expectedDuration;

    public AudioSource Source => _source != null ? _source : (_source = GetComponent<AudioSource>());
    public bool IsActive => _isActive;
    public SoundDataSO CurrentData => _currentData;

    private void Awake()
    {
        if (_source == null)
            _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    private void LateUpdate()
    {
        if (!_isActive)
            return;

        if (_followTarget != null)
        {
            transform.position = _followTarget.position;
        }

        if (_currentData != null && !_currentData.Loop)
        {
            _playTimer += Time.unscaledDeltaTime;
            // Allow an initial grace period of 0.08s before querying isPlaying to prevent premature release on buffer latency
            if (_playTimer >= _expectedDuration || (_playTimer > 0.08f && !_source.isPlaying))
            {
                Release();
            }
        }
    }

    /// <summary>Initializes and plays a SoundDataSO event on this pooled emitter.</summary>
    public void Play(SoundDataSO data, Vector3 position, Transform followTarget, float volumeMultiplier, Action<AudioSourceEmitter> onRelease)
    {
        if (this == null || gameObject == null)
            return;

        _currentData = data;
        _followTarget = followTarget;
        _onRelease = onRelease;
        _isActive = true;
        _playTimer = 0f;

        transform.position = followTarget != null ? followTarget.position : position;

        AudioClip clip = data.GetRandomClip();
        if (clip == null)
        {
            Release();
            return;
        }

        float pitch = data.GetRandomPitch();
        float finalVolume = Mathf.Clamp01(data.Volume * volumeMultiplier);

        _source.clip = clip;
        _source.outputAudioMixerGroup = data.MixerGroup;
        _source.volume = finalVolume;
        _source.pitch = pitch;
        _source.loop = data.Loop;
        _source.spatialBlend = data.SpatialBlend;
        _source.minDistance = data.MinMaxDistance.x;
        _source.maxDistance = data.MinMaxDistance.y;
        _source.rolloffMode = AudioRolloffMode.Logarithmic;
        _source.dopplerLevel = 0f;
        _source.ignoreListenerPause = true;

        _expectedDuration = pitch > 0.01f ? (clip.length / pitch) + 0.1f : clip.length + 0.1f;

        data.RecordPlayed();
        _source.Play();
    }

    /// <summary>Stops playback and returns this emitter to its pool immediately.</summary>
    public void Stop()
    {
        if (_source != null && _source.isPlaying)
            _source.Stop();

        Release();
    }

    private void Release()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _followTarget = null;
        _currentData = null;

        if (_source != null)
        {
            _source.Stop();
            _source.clip = null;
        }

        _onRelease?.Invoke(this);
        _onRelease = null;
    }
}
