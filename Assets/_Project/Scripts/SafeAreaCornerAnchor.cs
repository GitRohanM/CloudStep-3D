using UnityEngine;

/// <summary>Keeps a corner HUD widget inside the active display safe area.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaCornerAnchor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float horizontalPadding = 24f;
    [SerializeField, Min(0f)] private float verticalPadding = 20f;

    private RectTransform rectTransform;
    private Rect appliedSafeArea;
    private int appliedWidth;
    private int appliedHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void LateUpdate()
    {
        if (Screen.width != appliedWidth || Screen.height != appliedHeight || Screen.safeArea != appliedSafeArea)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        Vector2 topLeftAnchor = new Vector2(safeArea.xMin / Screen.width, safeArea.yMax / Screen.height);
        rectTransform.anchorMin = topLeftAnchor;
        rectTransform.anchorMax = topLeftAnchor;
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(horizontalPadding, -verticalPadding);

        appliedSafeArea = safeArea;
        appliedWidth = Screen.width;
        appliedHeight = Screen.height;
    }
}
