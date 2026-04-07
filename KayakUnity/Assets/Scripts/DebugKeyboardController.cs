using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DebugKeyboardController : MonoBehaviour
{
    [Header("Forces")]
    public float forwardForce = 150f;
    public float turnTorque = 60f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float forward = 0f;
        float turn = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)    forward += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)  forward -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  turn -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turn += 1f;

        if (forward != 0f)
            rb.AddForce(transform.forward * forward * forwardForce, ForceMode.Force);

        if (turn != 0f)
            rb.AddTorque(transform.up * turn * turnTorque, ForceMode.Force);
    }
}
