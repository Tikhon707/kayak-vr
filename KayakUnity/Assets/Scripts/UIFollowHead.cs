using UnityEngine;

public class UIFollowHead : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform headCamera;

    [Header("Parameters")]
    [Tooltip("How far the menu is from the player's face")]
    [SerializeField] private float distance = 2.0f;

    [Tooltip("Smoothing speed (lower value = slower movement)")]
    [SerializeField] private float smoothSpeed = 5.0f;

    [Header("Offsets")]
    [Tooltip("Vertical shift relative to eye level. Positive values move it up.")]
    [SerializeField] private float heightOffset = 0.0f;

    [Tooltip("Horizontal shift relative to the center of view. Positive = Right, Negative = Left.")]
    [SerializeField] private float horizontalOffset = 0.5f;

    // ! ADDED: Slider to control how aggressively the UI turns towards the player
    [Header("Rotation Blend")]
    [Tooltip("0 = Parallel to face, 1 = Perfectly angled towards eyes")]
    [Range(0f, 1f)]
    [SerializeField] private float lookAtBlend = 0.5f;

    void LateUpdate()
    {
        if (headCamera == null) return;

        // STEP 1: Determine forward direction, IGNORING vertical tilt (pitch)
        Vector3 forwardDirection = headCamera.forward;
        forwardDirection.y = 0; // Lock the horizon
        forwardDirection.Normalize();

        // Determine the right direction to apply horizontal offset
        Vector3 rightDirection = headCamera.right;
        rightDirection.y = 0; // Lock the horizon for the right vector too
        rightDirection.Normalize();

        // STEP 2: Calculate target position using forward and right vectors
        Vector3 targetPosition = headCamera.position + (forwardDirection * distance) + (rightDirection * horizontalOffset);

        // STEP 3: Apply manual height offset
        targetPosition.y = headCamera.position.y + heightOffset;

        // ! MODIFIED: STEP 4: Advanced Rotation with Blend

        // Rotation A: Flat against the screen (parallel to face)
        Quaternion parallelRotation = Quaternion.LookRotation(forwardDirection);

        // Rotation B: Aggressively angled towards the camera's center
        Vector3 lookDirection = targetPosition - headCamera.position;
        lookDirection.y = 0;
        Quaternion angledRotation = Quaternion.LookRotation(lookDirection);

        // ! ADDED: Blend between the two rotations based on the slider
        Quaternion targetRotation = Quaternion.Lerp(parallelRotation, angledRotation, lookAtBlend);

        // STEP 5: Smooth movement using unscaledDeltaTime
        float deltaTime = Time.unscaledDeltaTime;

        transform.position = Vector3.Lerp(transform.position, targetPosition, deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * smoothSpeed);
    }
}