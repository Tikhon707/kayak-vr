using UnityEngine;
using UnityEngine.InputSystem;

public class DebugScript : MonoBehaviour
{
    public float forceAmount = 10f;
    public ForceMode forceMode = ForceMode.Force;
    public bool useLocalDirection = true;  // относительно объекта
    public bool continuousForce = false;
    private Rigidbody rb;
    private Keyboard keyboard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        keyboard = Keyboard.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (keyboard.wKey.isPressed)
            ApplyForce(Vector3.forward);
        if (keyboard.sKey.isPressed)
            ApplyForce(Vector3.back);
        if (keyboard.dKey.isPressed)
            ApplyForce(Vector3.right);
        if (keyboard.aKey.isPressed)
            ApplyForce(Vector3.left);
    }
    
    void ApplyForce(Vector3 forceDirection)
    {
        Vector3 direction = useLocalDirection 
            ? transform.TransformDirection(forceDirection) 
            : forceDirection;
            
        rb.AddForce(direction.normalized * forceAmount, forceMode);
    }
    
   
}
