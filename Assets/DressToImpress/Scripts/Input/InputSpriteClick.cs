using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Detects mouse clicks and hover events on 2D sprites using Physics2D raycasting.
/// Requires a Collider2D on the same GameObject.
/// 2D equivalent of InputMouseInteraction — use this for sprite-based games.
/// Common use: Clickable clothing items, interactive sprites, selection systems.
/// </summary>
public class InputSpriteClick : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Which mouse button to detect (0=Left, 1=Right, 2=Middle)")]
    [SerializeField] private int mouseButton = 0;
    [SerializeField] private bool enableHover = true;
    [SerializeField] private bool enableClick = true;

    [Header("Scale Feedback")]
    [Tooltip("Slightly enlarge the sprite when the cursor hovers over it")]
    [SerializeField] private bool scaleOnHover = true;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private float scaleAnimationDuration = 0.15f;

    [Header("Color Feedback")]
    [Tooltip("Tint the sprite when the cursor hovers over it")]
    [SerializeField] private bool colorOnHover = false;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f, 1f);

    [Header("Click Events")]
    /// <summary>
    /// Fires when the mouse button is pressed and released on the same sprite
    /// </summary>
    public UnityEvent onMouseClick;

    /// <summary>
    /// Fires when the mouse button is pressed down on this sprite
    /// </summary>
    public UnityEvent onMouseDown;

    /// <summary>
    /// Fires when the mouse button is released over this sprite
    /// </summary>
    public UnityEvent onMouseUp;

    [Header("Hover Events")]
    /// <summary>
    /// Fires when the mouse cursor first enters this sprite's collider
    /// </summary>
    public UnityEvent onMouseEnter;

    /// <summary>
    /// Fires when the mouse cursor leaves this sprite's collider
    /// </summary>
    public UnityEvent onMouseExit;

    /// <summary>
    /// Fires continuously each frame while the mouse cursor is over this sprite
    /// </summary>
    public UnityEvent onMouseHover;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private bool isHovering = false;
    private bool isMouseDown = false;
    private bool wasHitLastFrame = false;
    private Vector3 originalScale;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private Tween scaleTween;

    /// <summary>Returns true while the cursor is over this sprite</summary>
    public bool IsHovering => isHovering;

    private void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (GetComponent<Collider2D>() == null)
            Debug.LogWarning($"[InputSpriteClick] '{gameObject.name}' requires a Collider2D to detect clicks!", this);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));

        // OverlapPoint is the correct Physics2D API for "what is under this world-space point"
        // Physics2D.Raycast with zero direction is unreliable and should not be used here
        Collider2D hitCollider = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
        bool isHit = hitCollider != null && hitCollider.gameObject == gameObject;

        // --- Hover state machine ---
        if (enableHover)
        {
            if (isHit && !wasHitLastFrame)
            {
                isHovering = true;
                ApplyHoverEffects();
                onMouseEnter.Invoke();
                if (showDebugInfo) Debug.Log($"[InputSpriteClick] Enter: {gameObject.name}");
            }
            else if (!isHit && wasHitLastFrame)
            {
                isHovering = false;
                RemoveHoverEffects();
                onMouseExit.Invoke();
                if (showDebugInfo) Debug.Log($"[InputSpriteClick] Exit: {gameObject.name}");
            }

            if (isHit)
                onMouseHover.Invoke();
        }

        // --- Click state machine ---
        if (enableClick)
        {
            if (isHit && GetButtonDown())
            {
                isMouseDown = true;
                onMouseDown.Invoke();
                if (showDebugInfo) Debug.Log($"[InputSpriteClick] Down: {gameObject.name}");
            }

            if (GetButtonUp())
            {
                if (isHit)
                {
                    onMouseUp.Invoke();
                    if (isMouseDown)
                    {
                        onMouseClick.Invoke();
                        if (showDebugInfo) Debug.Log($"[InputSpriteClick] Click: {gameObject.name}");
                    }
                }
                isMouseDown = false;
            }
        }

        wasHitLastFrame = isHit;
    }

    private bool GetButtonDown()
    {
        return mouseButton switch
        {
            0 => Mouse.current.leftButton.wasPressedThisFrame,
            1 => Mouse.current.rightButton.wasPressedThisFrame,
            2 => Mouse.current.middleButton.wasPressedThisFrame,
            _ => Mouse.current.leftButton.wasPressedThisFrame
        };
    }

    private bool GetButtonUp()
    {
        return mouseButton switch
        {
            0 => Mouse.current.leftButton.wasReleasedThisFrame,
            1 => Mouse.current.rightButton.wasReleasedThisFrame,
            2 => Mouse.current.middleButton.wasReleasedThisFrame,
            _ => Mouse.current.leftButton.wasReleasedThisFrame
        };
    }

    private void ApplyHoverEffects()
    {
        if (scaleOnHover)
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(Vector3.Scale(originalScale, hoverScale), scaleAnimationDuration)
                .SetEase(Ease.OutBack);
        }

        if (colorOnHover && spriteRenderer != null)
            spriteRenderer.color = hoverColor;
    }

    private void RemoveHoverEffects()
    {
        if (scaleOnHover)
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(originalScale, scaleAnimationDuration)
                .SetEase(Ease.OutBack);
        }

        if (colorOnHover && spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Simulates a click — useful for wiring other events to trigger this object's click
    /// </summary>
    public void SimulateClick()
    {
        onMouseClick.Invoke();
        if (showDebugInfo) Debug.Log($"[InputSpriteClick] Simulated click: {gameObject.name}");
    }

    /// <summary>
    /// Enable both click and hover detection
    /// </summary>
    public void EnableInteraction()
    {
        enableClick = true;
        enableHover = true;
    }

    /// <summary>
    /// Disable all mouse interaction and remove hover effects
    /// </summary>
    public void DisableInteraction()
    {
        enableClick = false;
        enableHover = false;
        if (isHovering)
        {
            isHovering = false;
            RemoveHoverEffects();
        }
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            isHovering = false;
            RemoveHoverEffects();
        }
        wasHitLastFrame = false;
        isMouseDown = false;
        scaleTween?.Kill();
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
