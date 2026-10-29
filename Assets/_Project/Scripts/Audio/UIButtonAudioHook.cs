using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Lightweight, decoupled UI audio hook for Unity UI Buttons.
/// Automatically binds to Button.onClick and IPointerEnter to trigger click and hover sound events
/// using unscaled time (functions seamlessly even during paused menus).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonAudioHook : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [Header("Sound IDs")]
    [SerializeField] private string clickSoundId = "UI_Click";
    [SerializeField] private string hoverSoundId = "UI_Hover";

    [Header("Optional Direct References")]
    [SerializeField] private SoundDataSO clickSound;
    [SerializeField] private SoundDataSO hoverSound;

    [Header("Volume Multipliers")]
    [SerializeField, Range(0f, 1f)] private float clickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.65f;

    private Button _button;
    private float _lastHoverTime = float.NegativeInfinity;
    private const float HoverCooldown = 0.05f;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Support gamepad / keyboard UI selection navigation
        PlayHoverSound();
    }

    private void OnClick()
    {
        if (clickSound != null)
        {
            AudioManager.Instance?.PlaySound(clickSound, clickVolume);
        }
        else if (!string.IsNullOrWhiteSpace(clickSoundId))
        {
            AudioManager.Instance?.PlaySound(clickSoundId, clickVolume);
        }
    }

    private void PlayHoverSound()
    {
        if (_button != null && !_button.interactable)
            return;

        if (Time.unscaledTime - _lastHoverTime < HoverCooldown)
            return;

        _lastHoverTime = Time.unscaledTime;

        if (hoverSound != null)
        {
            AudioManager.Instance?.PlaySound(hoverSound, hoverVolume);
        }
        else if (!string.IsNullOrWhiteSpace(hoverSoundId))
        {
            AudioManager.Instance?.PlaySound(hoverSoundId, hoverVolume);
        }
    }
}
