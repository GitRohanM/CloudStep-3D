using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// User interface controller for the in-game Audio Settings panel.
/// Connects sliders and mute buttons to the AudioManager with live volume updates,
/// decibel conversion, percentage readouts, and PlayerPrefs persistence.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform cardRect;

    [Header("Master Controls")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private Button muteButton;
    [SerializeField] private TMP_Text muteButtonText;

    [Header("BGM / Music Controls")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmValueText;

    [Header("SFX Controls")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("UI Audio Controls")]
    [SerializeField] private Slider uiSlider;
    [SerializeField] private TMP_Text uiValueText;

    [Header("Navigation Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button defaultsButton;

    private void Awake()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();

        BindControls();
    }

    private void OnEnable()
    {
        RefreshUIFromManager();
        SubscribeToManager();
    }

    private void OnDisable()
    {
        UnsubscribeFromManager();
    }

    private void BindControls()
    {
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        if (uiSlider != null) uiSlider.onValueChanged.AddListener(OnUISliderChanged);

        if (muteButton != null) muteButton.onClick.AddListener(OnMuteButtonClicked);
        if (defaultsButton != null) defaultsButton.onClick.AddListener(OnDefaultsButtonClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
    }

    private void SubscribeToManager()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.OnMasterVolumeChanged += UpdateMasterDisplay;
        AudioManager.Instance.OnBGMVolumeChanged += UpdateBGMDisplay;
        AudioManager.Instance.OnSFXVolumeChanged += UpdateSFXDisplay;
        AudioManager.Instance.OnUIVolumeChanged += UpdateUIDisplay;
        AudioManager.Instance.OnMuteToggled += UpdateMuteDisplay;
    }

    private void UnsubscribeFromManager()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.OnMasterVolumeChanged -= UpdateMasterDisplay;
        AudioManager.Instance.OnBGMVolumeChanged -= UpdateBGMDisplay;
        AudioManager.Instance.OnSFXVolumeChanged -= UpdateSFXDisplay;
        AudioManager.Instance.OnUIVolumeChanged -= UpdateUIDisplay;
        AudioManager.Instance.OnMuteToggled -= UpdateMuteDisplay;
    }

    public void RefreshUIFromManager()
    {
        if (AudioManager.Instance == null) return;

        UpdateMasterDisplay(AudioManager.Instance.MasterVolume);
        UpdateBGMDisplay(AudioManager.Instance.BGMVolume);
        UpdateSFXDisplay(AudioManager.Instance.SFXVolume);
        UpdateUIDisplay(AudioManager.Instance.UIVolume);
        UpdateMuteDisplay(AudioManager.Instance.IsMuted);
    }

    private void OnMasterSliderChanged(float val)
    {
        AudioManager.Instance?.SetMasterVolume(val);
        UpdateMasterDisplay(val);
    }

    private void OnBGMSliderChanged(float val)
    {
        AudioManager.Instance?.SetBGMVolume(val);
        UpdateBGMDisplay(val);
    }

    private void OnSFXSliderChanged(float val)
    {
        AudioManager.Instance?.SetSFXVolume(val);
        UpdateSFXDisplay(val);
    }

    private void OnUISliderChanged(float val)
    {
        AudioManager.Instance?.SetUIVolume(val);
        UpdateUIDisplay(val);
    }

    private void OnMuteButtonClicked()
    {
        AudioManager.Instance?.ToggleMute();
    }

    private void OnDefaultsButtonClicked()
    {
        AudioManager.Instance?.ResetToDefaults();
        RefreshUIFromManager();
        AudioManager.Instance?.PlaySound("UI_Click");
    }

    private void UpdateMasterDisplay(float val)
    {
        if (masterSlider != null && !Mathf.Approximately(masterSlider.value, val))
            masterSlider.SetValueWithoutNotify(val);
        if (masterValueText != null)
            masterValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void UpdateBGMDisplay(float val)
    {
        if (bgmSlider != null && !Mathf.Approximately(bgmSlider.value, val))
            bgmSlider.SetValueWithoutNotify(val);
        if (bgmValueText != null)
            bgmValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void UpdateSFXDisplay(float val)
    {
        if (sfxSlider != null && !Mathf.Approximately(sfxSlider.value, val))
            sfxSlider.SetValueWithoutNotify(val);
        if (sfxValueText != null)
            sfxValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void UpdateUIDisplay(float val)
    {
        if (uiSlider != null && !Mathf.Approximately(uiSlider.value, val))
            uiSlider.SetValueWithoutNotify(val);
        if (uiValueText != null)
            uiValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void UpdateMuteDisplay(bool isMuted)
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "UNMUTE" : "MUTE";
            muteButtonText.color = isMuted ? new Color(1f, 0.4f, 0.4f) : Color.white;
        }
    }

    public void OpenSettings()
    {
        RefreshUIFromManager();
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.gameObject.SetActive(true);
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }

        if (cardRect != null)
            cardRect.localScale = Vector3.one;

        AudioManager.Instance?.PlaySound("UI_Pause");
    }

    public void CloseSettings()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        AudioManager.Instance?.PlaySound("UI_Resume");
    }
}
