using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central production-grade audio manager singleton.
/// Features zero-allocation object-pooled SFX emitters, dual-channel BGM crossfading,
/// ambient background streaming, and logarithmic decibel volume conversion.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer & Routing")]
    [Tooltip("Master AudioMixer controlling overall sub-mix channels.")]
    [SerializeField] private AudioMixer masterMixer;

    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;

    [Header("Mixer Exposed Parameter Names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string bgmParam = "BGMVolume";
    [SerializeField] private string ambienceParam = "AmbienceVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string uiParam = "UIVolume";

    [Header("Sound Registry")]
    [Tooltip("All registered sound events available for SoundID string queries.")]
    [SerializeField] private List<SoundDataSO> soundDatabase = new List<SoundDataSO>();

    [Header("Pooling Configuration")]
    [SerializeField, Min(5)] private int initialPoolSize = 20;

    // Dedicated channels for music crossfading
    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;
    private bool _isChannelAActive = false;
    private Coroutine _bgmFadeCoroutine;

    // Dedicated channel for ambient loops
    private AudioSource _ambienceSource;
    private Coroutine _ambienceFadeCoroutine;

    // PlayerPrefs storage keys
    private const string PrefMasterVol = "Cloudstep.Audio.Master";
    private const string PrefBGMVol = "Cloudstep.Audio.BGM";
    private const string PrefAmbienceVol = "Cloudstep.Audio.Ambience";
    private const string PrefSFXVol = "Cloudstep.Audio.SFX";
    private const string PrefUIVol = "Cloudstep.Audio.UI";
    private const string PrefIsMuted = "Cloudstep.Audio.Muted";

    // Observable audio events for settings UI synchronization
    public event Action<float> OnMasterVolumeChanged;
    public event Action<float> OnBGMVolumeChanged;
    public event Action<float> OnAmbienceVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;
    public event Action<float> OnUIVolumeChanged;
    public event Action<bool> OnMuteToggled;

    // SFX Object Pool
    private readonly Queue<AudioSourceEmitter> _availablePool = new Queue<AudioSourceEmitter>();
    private readonly List<AudioSourceEmitter> _activeEmitters = new List<AudioSourceEmitter>();
    private Transform _poolContainer;

    // Fast lookup dictionary
    private readonly Dictionary<string, SoundDataSO> _soundDict = new Dictionary<string, SoundDataSO>(StringComparer.OrdinalIgnoreCase);

    // Balanced default linear volume states (0.0 to 1.0)
    private float _masterVolume = 0.85f;
    private float _bgmVolume = 0.38f;
    private float _ambienceVolume = 0.22f;
    private float _sfxVolume = 0.70f;
    private float _uiVolume = 0.60f;
    private bool _isMuted = false;

    public AudioMixer MasterMixer => masterMixer;
    public AudioMixerGroup MasterGroup => masterGroup;
    public AudioMixerGroup BGMGroup => bgmGroup;
    public AudioMixerGroup AmbienceGroup => ambienceGroup;
    public AudioMixerGroup SFXGroup => sfxGroup;
    public AudioMixerGroup UIGroup => uiGroup;

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float AmbienceVolume => _ambienceVolume;
    public float SFXVolume => _sfxVolume;
    public float UIVolume => _uiVolume;
    public bool IsMuted => _isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        BuildSoundDictionary();
        SetupBGMAndAmbienceChannels();
        SetupObjectPool();
        LoadAudioPreferences();
    }

    private void BuildSoundDictionary()
    {
        _soundDict.Clear();
        foreach (var data in soundDatabase)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.SoundID))
                continue;

            if (_soundDict.ContainsKey(data.SoundID))
            {
                Debug.LogWarning($"[AudioManager] Duplicate SoundID '{data.SoundID}' detected in database. Skipping duplicate.", this);
                continue;
            }

            _soundDict.Add(data.SoundID, data);
        }
    }

    private void SetupBGMAndAmbienceChannels()
    {
        GameObject bgmRoot = new GameObject("BGM_Channels");
        bgmRoot.transform.SetParent(transform);

        _bgmSourceA = bgmRoot.AddComponent<AudioSource>();
        _bgmSourceA.loop = true;
        _bgmSourceA.playOnAwake = false;
        _bgmSourceA.spatialBlend = 0f;
        _bgmSourceA.outputAudioMixerGroup = bgmGroup;

        _bgmSourceB = bgmRoot.AddComponent<AudioSource>();
        _bgmSourceB.loop = true;
        _bgmSourceB.playOnAwake = false;
        _bgmSourceB.spatialBlend = 0f;
        _bgmSourceB.outputAudioMixerGroup = bgmGroup;

        GameObject ambRoot = new GameObject("Ambience_Channel");
        ambRoot.transform.SetParent(transform);

        _ambienceSource = ambRoot.AddComponent<AudioSource>();
        _ambienceSource.loop = true;
        _ambienceSource.playOnAwake = false;
        _ambienceSource.spatialBlend = 0f;
        _ambienceSource.outputAudioMixerGroup = ambienceGroup;
    }

    private void SetupObjectPool()
    {
        _poolContainer = new GameObject("SFX_Pool").transform;
        _poolContainer.SetParent(transform);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledEmitter();
        }
    }

    private AudioSourceEmitter CreatePooledEmitter()
    {
        GameObject obj = new GameObject($"Emitter_{_availablePool.Count + _activeEmitters.Count:00}");
        obj.transform.SetParent(_poolContainer);
        AudioSourceEmitter emitter = obj.AddComponent<AudioSourceEmitter>();
        _availablePool.Enqueue(emitter);
        return emitter;
    }

    private AudioSourceEmitter GetEmitter()
    {
        while (_availablePool.Count > 0)
        {
            AudioSourceEmitter candidate = _availablePool.Dequeue();
            if (candidate != null && candidate.gameObject != null)
            {
                _activeEmitters.Add(candidate);
                return candidate;
            }
        }

        // Expand pool on demand if depleted or cleaned up
        AudioSourceEmitter emitter = CreatePooledEmitter();
        _availablePool.Dequeue();
        _activeEmitters.Add(emitter);
        return emitter;
    }

    private void ReturnEmitterToPool(AudioSourceEmitter emitter)
    {
        if (emitter == null || emitter.gameObject == null) return;

        _activeEmitters.Remove(emitter);
        if (_poolContainer != null)
        {
            emitter.transform.SetParent(_poolContainer);
            emitter.transform.localPosition = Vector3.zero;
        }
        _availablePool.Enqueue(emitter);
    }

    // =========================================================================
    // SFX PLAYBACK METHODS (2D & 3D SPATIAL)
    // =========================================================================

    public AudioSourceEmitter PlaySound(string soundId, float volumeMultiplier = 1f)
    {
        if (TryGetSound(soundId, out SoundDataSO data))
        {
            return PlaySound(data, volumeMultiplier);
        }
        return null;
    }

    public AudioSourceEmitter PlaySound(SoundDataSO data, float volumeMultiplier = 1f)
    {
        if (data == null || !data.CanPlay())
            return null;

        AudioSourceEmitter emitter = GetEmitter();
        emitter.Play(data, Vector3.zero, null, volumeMultiplier, ReturnEmitterToPool);
        return emitter;
    }

    public AudioSourceEmitter PlaySoundAtPosition(string soundId, Vector3 position, float volumeMultiplier = 1f)
    {
        if (TryGetSound(soundId, out SoundDataSO data))
        {
            return PlaySoundAtPosition(data, position, volumeMultiplier);
        }
        return null;
    }

    public AudioSourceEmitter PlaySoundAtPosition(SoundDataSO data, Vector3 position, float volumeMultiplier = 1f)
    {
        if (data == null || !data.CanPlay())
            return null;

        AudioSourceEmitter emitter = GetEmitter();
        emitter.Play(data, position, null, volumeMultiplier, ReturnEmitterToPool);
        return emitter;
    }

    public AudioSourceEmitter PlaySoundAttached(string soundId, Transform target, float volumeMultiplier = 1f)
    {
        if (TryGetSound(soundId, out SoundDataSO data))
        {
            return PlaySoundAttached(data, target, volumeMultiplier);
        }
        return null;
    }

    public AudioSourceEmitter PlaySoundAttached(SoundDataSO data, Transform target, float volumeMultiplier = 1f)
    {
        if (data == null || !data.CanPlay() || target == null)
            return null;

        AudioSourceEmitter emitter = GetEmitter();
        emitter.Play(data, target.position, target, volumeMultiplier, ReturnEmitterToPool);
        return emitter;
    }

    public bool TryGetSound(string soundId, out SoundDataSO data)
    {
        if (string.IsNullOrWhiteSpace(soundId))
        {
            data = null;
            return false;
        }

        if (_soundDict.TryGetValue(soundId, out data))
        {
            return true;
        }

        Debug.LogWarning($"[AudioManager] SoundID '{soundId}' was requested but not found in the sound registry.");
        data = null;
        return false;
    }

    // =========================================================================
    // BGM CROSSFADING
    // =========================================================================

    public void PlayBGM(string soundId, float fadeDuration = 1.0f)
    {
        if (TryGetSound(soundId, out SoundDataSO data))
        {
            PlayBGM(data, fadeDuration);
        }
    }

    public void PlayBGM(SoundDataSO data, float fadeDuration = 1.0f)
    {
        if (data == null) return;
        AudioClip clip = data.GetRandomClip();
        if (clip == null) return;

        PlayBGMClip(clip, data.Volume, data.MixerGroup ?? bgmGroup, fadeDuration);
    }

    public void PlayBGM(AudioClip clip, float fadeDuration = 1.0f)
    {
        PlayBGMClip(clip, 0.85f, bgmGroup, fadeDuration);
    }

    private void PlayBGMClip(AudioClip clip, float baseVolume, AudioMixerGroup group, float fadeDuration)
    {
        if (clip == null) return;

        AudioSource currentSource = _isChannelAActive ? _bgmSourceA : _bgmSourceB;
        AudioSource incomingSource = _isChannelAActive ? _bgmSourceB : _bgmSourceA;

        // If the exact clip is already playing on the active channel, do not restart
        if (currentSource.isPlaying && currentSource.clip == clip)
            return;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        _bgmFadeCoroutine = StartCoroutine(CrossfadeBGMRoutine(currentSource, incomingSource, clip, baseVolume, group, fadeDuration));
        _isChannelAActive = !_isChannelAActive;
    }

    private IEnumerator CrossfadeBGMRoutine(AudioSource fadeOutSource, AudioSource fadeInSource, AudioClip newClip, float targetBaseVol, AudioMixerGroup group, float duration)
    {
        fadeInSource.clip = newClip;
        fadeInSource.outputAudioMixerGroup = group;
        fadeInSource.volume = 0f;
        fadeInSource.Play();

        float targetFadeInVol = targetBaseVol * _bgmVolume;
        float startFadeOutVol = fadeOutSource.volume;
        float elapsed = 0f;

        if (duration <= 0.01f)
        {
            fadeInSource.volume = targetFadeInVol;
            fadeOutSource.volume = 0f;
            fadeOutSource.Stop();
            fadeOutSource.clip = null;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            fadeInSource.volume = Mathf.Lerp(0f, targetFadeInVol, t);
            if (fadeOutSource.isPlaying)
            {
                fadeOutSource.volume = Mathf.Lerp(startFadeOutVol, 0f, t);
            }

            yield return null;
        }

        fadeInSource.volume = targetFadeInVol;
        if (fadeOutSource.isPlaying)
        {
            fadeOutSource.Stop();
            fadeOutSource.clip = null;
        }
    }

    public void StopBGM(float fadeDuration = 1.0f)
    {
        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        AudioSource activeSource = _isChannelAActive ? _bgmSourceA : _bgmSourceB;
        _bgmFadeCoroutine = StartCoroutine(FadeOutSourceRoutine(activeSource, fadeDuration));
    }

    // =========================================================================
    // AMBIENCE STREAMING
    // =========================================================================

    public void PlayAmbience(string soundId, float fadeDuration = 1.5f)
    {
        if (TryGetSound(soundId, out SoundDataSO data))
        {
            PlayAmbience(data, fadeDuration);
        }
    }

    public void PlayAmbience(SoundDataSO data, float fadeDuration = 1.5f)
    {
        if (data == null) return;
        AudioClip clip = data.GetRandomClip();
        if (clip == null) return;

        if (_ambienceSource.isPlaying && _ambienceSource.clip == clip)
            return;

        if (_ambienceFadeCoroutine != null)
            StopCoroutine(_ambienceFadeCoroutine);

        _ambienceFadeCoroutine = StartCoroutine(FadeInAmbienceRoutine(clip, data.Volume, data.MixerGroup ?? ambienceGroup, fadeDuration));
    }

    private IEnumerator FadeInAmbienceRoutine(AudioClip clip, float baseVol, AudioMixerGroup group, float duration)
    {
        float targetVol = baseVol * _ambienceVolume;

        if (_ambienceSource.isPlaying)
        {
            float curVol = _ambienceSource.volume;
            float elapsedOut = 0f;
            float halfDuration = Mathf.Max(0.01f, duration * 0.5f);
            while (elapsedOut < halfDuration)
            {
                elapsedOut += Time.unscaledDeltaTime;
                _ambienceSource.volume = Mathf.Lerp(curVol, 0f, elapsedOut / halfDuration);
                yield return null;
            }
            _ambienceSource.Stop();
        }

        _ambienceSource.clip = clip;
        _ambienceSource.outputAudioMixerGroup = group;
        _ambienceSource.volume = 0f;
        _ambienceSource.Play();

        float elapsedIn = 0f;
        while (elapsedIn < duration)
        {
            elapsedIn += Time.unscaledDeltaTime;
            _ambienceSource.volume = Mathf.Lerp(0f, targetVol, elapsedIn / duration);
            yield return null;
        }

        _ambienceSource.volume = targetVol;
    }

    public void StopAmbience(float fadeDuration = 1.0f)
    {
        if (_ambienceFadeCoroutine != null)
            StopCoroutine(_ambienceFadeCoroutine);

        _ambienceFadeCoroutine = StartCoroutine(FadeOutSourceRoutine(_ambienceSource, fadeDuration));
    }

    private IEnumerator FadeOutSourceRoutine(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying) yield break;

        float startVol = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.clip = null;
    }

    // =========================================================================
    // MIXER VOLUME CONTROLS & LOGARITHMIC CONVERSION
    // =========================================================================

    /// <summary>
    /// Converts a normalized linear volume (0.0 to 1.0) to decibels:
    /// dB = log10(max(linear, 0.0001)) * 20
    /// </summary>
    public static float LinearToDecibels(float linear)
    {
        return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }

    private void LoadAudioPreferences()
    {
        _masterVolume = PlayerPrefs.GetFloat(PrefMasterVol, 0.85f);
        _bgmVolume = PlayerPrefs.GetFloat(PrefBGMVol, 0.38f);
        _ambienceVolume = PlayerPrefs.GetFloat(PrefAmbienceVol, 0.22f);
        _sfxVolume = PlayerPrefs.GetFloat(PrefSFXVol, 0.70f);
        _uiVolume = PlayerPrefs.GetFloat(PrefUIVol, 0.60f);
        _isMuted = PlayerPrefs.GetInt(PrefIsMuted, 0) == 1;

        ApplyAllVolumes();
    }

    public void ApplyAllVolumes()
    {
        SetMixerVolume(masterParam, _isMuted ? 0f : _masterVolume);
        SetMixerVolume(bgmParam, _bgmVolume);
        SetMixerVolume(ambienceParam, _ambienceVolume);
        SetMixerVolume(sfxParam, _sfxVolume);
        SetMixerVolume(uiParam, _uiVolume);

        // Also scale active BGM / Ambience sources directly
        if (_bgmSourceA != null) _bgmSourceA.volume = (_isChannelAActive ? _bgmVolume : 0f);
        if (_bgmSourceB != null) _bgmSourceB.volume = (!_isChannelAActive ? _bgmVolume : 0f);
        if (_ambienceSource != null) _ambienceSource.volume = _ambienceVolume;
    }

    public void SetMasterVolume(float linear, bool save = true)
    {
        _masterVolume = Mathf.Clamp01(linear);
        if (!_isMuted)
            SetMixerVolume(masterParam, _masterVolume);

        if (save) PlayerPrefs.SetFloat(PrefMasterVol, _masterVolume);
        OnMasterVolumeChanged?.Invoke(_masterVolume);
    }

    public void SetBGMVolume(float linear, bool save = true)
    {
        _bgmVolume = Mathf.Clamp01(linear);
        SetMixerVolume(bgmParam, _bgmVolume);

        if (_bgmSourceA != null && _isChannelAActive) _bgmSourceA.volume = _bgmVolume;
        if (_bgmSourceB != null && !_isChannelAActive) _bgmSourceB.volume = _bgmVolume;

        if (save) PlayerPrefs.SetFloat(PrefBGMVol, _bgmVolume);
        OnBGMVolumeChanged?.Invoke(_bgmVolume);
    }

    public void SetAmbienceVolume(float linear, bool save = true)
    {
        _ambienceVolume = Mathf.Clamp01(linear);
        SetMixerVolume(ambienceParam, _ambienceVolume);
        if (_ambienceSource != null) _ambienceSource.volume = _ambienceVolume;

        if (save) PlayerPrefs.SetFloat(PrefAmbienceVol, _ambienceVolume);
        OnAmbienceVolumeChanged?.Invoke(_ambienceVolume);
    }

    public void SetSFXVolume(float linear, bool save = true)
    {
        _sfxVolume = Mathf.Clamp01(linear);
        SetMixerVolume(sfxParam, _sfxVolume);

        if (save) PlayerPrefs.SetFloat(PrefSFXVol, _sfxVolume);
        OnSFXVolumeChanged?.Invoke(_sfxVolume);
    }

    public void SetUIVolume(float linear, bool save = true)
    {
        _uiVolume = Mathf.Clamp01(linear);
        SetMixerVolume(uiParam, _uiVolume);

        if (save) PlayerPrefs.SetFloat(PrefUIVol, _uiVolume);
        OnUIVolumeChanged?.Invoke(_uiVolume);
    }

    public void SetMute(bool mute, bool save = true)
    {
        _isMuted = mute;
        SetMixerVolume(masterParam, _isMuted ? 0f : _masterVolume);

        if (save)
        {
            PlayerPrefs.SetInt(PrefIsMuted, _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
        OnMuteToggled?.Invoke(_isMuted);
    }

    public void ToggleMute()
    {
        SetMute(!_isMuted);
    }

    public void ResetToDefaults()
    {
        SetMasterVolume(0.85f);
        SetBGMVolume(0.38f);
        SetAmbienceVolume(0.22f);
        SetSFXVolume(0.70f);
        SetUIVolume(0.60f);
        SetMute(false);
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string parameterName, float linear)
    {
        if (masterMixer == null || string.IsNullOrWhiteSpace(parameterName))
            return;

        float dB = LinearToDecibels(linear);
        masterMixer.SetFloat(parameterName, dB);
    }

    /// <summary>Allows runtime registration of SoundDataSO objects.</summary>
    public void RegisterSound(SoundDataSO soundData)
    {
        if (soundData == null || string.IsNullOrWhiteSpace(soundData.SoundID)) return;
        _soundDict[soundData.SoundID] = soundData;
        if (!soundDatabase.Contains(soundData))
            soundDatabase.Add(soundData);
    }
}
