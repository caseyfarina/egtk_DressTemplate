using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Allows a 2D sprite to be clicked and dragged by the player.
/// Requires a Collider2D on the same GameObject.
/// When released over a valid DressUpSlot trigger, the slot calls NotifyDropped() to
/// prevent the item from snapping back to its origin.
/// 2D equivalent of InputClickDrag — use this for sprite-based games.
/// Common use: Dress-up clothing items, puzzle pieces, drag-to-sort mechanics.
/// </summary>
public class InputSpriteDrag : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Z position locked during drag — keep at 0 for standard 2D scenes")]
    [SerializeField] private float dragZ = 0f;

    [Tooltip("When true, the object's centre snaps directly under the cursor. When false, the grab offset is maintained.")]
    [SerializeField] private bool snapToCenter = false;

    [Header("Return Behaviour")]
    [Tooltip("When dropped outside a valid slot, snap back to the original position")]
    [SerializeField] private bool returnOnDrop = true;

    [Tooltip("Duration of the return-to-origin tween")]
    [SerializeField] private float returnDuration = 0.3f;

    [Header("Sorting")]
    [Tooltip("Sprite sorting order used while dragging (should be higher than other sprites so the item renders on top)")]
    [SerializeField] private int dragSortingOrder = 100;

    [Header("Events")]
    /// <summary>
    /// Fires when the player begins dragging this sprite
    /// </summary>
    public UnityEvent onDragStart;

    /// <summary>
    /// Fires when the player releases the mouse button, regardless of drop success
    /// </summary>
    public UnityEvent onDragEnd;

    /// <summary>
    /// Fires when the sprite is successfully accepted by a drop target (via NotifyDropped)
    /// </summary>
    public UnityEvent onDropped;

    /// <summary>
    /// Fires when the sprite finishes returning to its original position after a failed drop
    /// </summary>
    public UnityEvent onReturned;

    private bool isDragging = false;
    private bool wasDropped = false;
    private Vector3 originalPosition;
    private Vector2 grabOffset;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Tween returnTween;

    /// <summary>Returns true while this sprite is being dragged</summary>
    public bool IsDragging => isDragging;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("[InputSpriteDrag] No Camera.main found in scene.", this);

        originalPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalSortingOrder = spriteRenderer.sortingOrder;

        if (GetComponent<Collider2D>() == null)
            Debug.LogError("[InputSpriteDrag] Requires a Collider2D on this GameObject.", this);
    }

    private void Update()
    {
        if (mainCamera == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame && !isDragging)
            TryStartDrag(mouse.position.ReadValue());

        if (isDragging)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                EndDrag();
            else
                UpdateDrag(mouse.position.ReadValue());
        }
    }

    private void TryStartDrag(Vector2 screenPos)
    {
        Vector2 worldPos = ScreenToWorld2D(screenPos);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);

        if (hitCollider == null || hitCollider.gameObject != gameObject) return;

        // Cancel any in-progress return tween
        returnTween?.Kill();

        isDragging = true;
        wasDropped = false;
        grabOffset = snapToCenter ? Vector2.zero : (Vector2)transform.position - worldPos;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = dragSortingOrder;

        onDragStart.Invoke();
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        Vector2 worldPos = ScreenToWorld2D(screenPos);
        transform.position = new Vector3(worldPos.x + grabOffset.x, worldPos.y + grabOffset.y, dragZ);
    }

    private void EndDrag()
    {
        isDragging = false;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

        onDragEnd.Invoke();

        // Defer the return-to-origin check by one frame so that any DressUpSlot
        // whose Update() runs this frame can call NotifyDropped() before we decide to return.
        if (returnOnDrop)
            StartCoroutine(ReturnIfNotDropped());
    }

    private IEnumerator ReturnIfNotDropped()
    {
        yield return null;  // wait one frame
        if (!wasDropped)
            ReturnToOrigin();
    }

    /// <summary>
    /// Call this from a DressUpSlot (or any drop target) when it accepts this item.
    /// Prevents the item from returning to its origin position.
    /// </summary>
    public void NotifyDropped()
    {
        wasDropped = true;
        onDropped.Invoke();
    }

    /// <summary>
    /// Animate the sprite back to its stored origin position.
    /// Also callable from a UnityEvent (e.g. to bounce an item back when unequipped).
    /// </summary>
    public void ReturnToOrigin()
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

        returnTween?.Kill();
        returnTween = transform.DOMove(originalPosition, returnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => onReturned.Invoke());
    }

    /// <summary>
    /// Stores the current world position as the new origin.
    /// Call this after intentionally repositioning the item (e.g. laying it out on the clothing panel).
    /// </summary>
    public void SetNewOrigin()
    {
        originalPosition = transform.position;
    }

    private Vector2 ScreenToWorld2D(Vector2 screenPos)
    {
        // Use Abs of camera Z so the result lands on Z=0 world plane regardless of camera position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
        return new Vector2(worldPos.x, worldPos.y);
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            // Clean up silently — don't fire events on disable
            isDragging = false;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = originalSortingOrder;
        }
        returnTween?.Kill();
    }

    private void OnDestroy()
    {
        returnTween?.Kill();
    }
}
