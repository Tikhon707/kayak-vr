using UnityEngine;
using Crest;

[RequireComponent(typeof(Rigidbody))]
public class DoublePaddleSystem : MonoBehaviour
{
    [System.Serializable]
    public class Blade
    {
        public Transform bladeRoot;
        public Transform bladeTip;
        public AudioSource bladeAudio; // ������ ���� �� ������� BladeTip
        [HideInInspector] public Vector3 constrainedPosition;
    }

    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Paddle Transform")]
    public Transform doublePaddle;

    [Header("Blades Setup")]
    public Blade leftBlade;
    public Blade rightBlade;

    [Header("Ambient Sound (���������� ���)")]
    public AudioSource ambientWaterSource;
    public float ambientBaseVolume = 0.15f;
    public float ambientSpeedMultiplier = 0.12f;

    [Header("Audio Settings (���������)")]
    public AudioClip splashClip;
    public float volumeMultiplier = 1.3f;
    public float pitchMin = 0.85f;
    public float pitchMax = 1.15f;

    [Header("Hand Constraints")]
    public float minHandDistance = 0.4f;
    public float maxHandDistance = 1.5f;

    [Header("Physics Settings")]
    public float bladeDepthThreshold = -0.05f;
    public float maxEffectiveSpeed = 2.0f; // �������, ����� ���� ��� ������ ��� ������� �������
    public float forceMultiplier = 70f;
    public float recoveryDrag = 0.5f;
    public float minEfficiency = 0.1f;
    public float maxEfficiency = 1.0f;

    [Header("Collision Prevention")]
    public LayerMask blockingLayers;
    public LayerMask groundLayer;
    public float bladeCollisionRadius = 0.08f;
    public float shaftCollisionRadius = 0.03f;
    public int shaftCheckPoints = 5;

    [Header("Crest Integration")]
    public float minSpatialLength = 1f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private Rigidbody rb;
    private Vector3 lastLeftTip, lastRightTip;
    private bool leftInWater, rightInWater;
    private bool isPaddleActive = false;


    private SampleHeightHelper _leftHeightHelper, _rightHeightHelper;
    private SampleFlowHelper _leftFlowHelper, _rightFlowHelper;
    private bool _initialized = false;

    private Vector3 constrainedLeftPos, constrainedRightPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (leftBlade?.bladeTip != null) lastLeftTip = leftBlade.bladeTip.position;
        if (rightBlade?.bladeTip != null) lastRightTip = rightBlade.bladeTip.position;

        constrainedLeftPos = leftController != null ? leftController.position : Vector3.zero;
        constrainedRightPos = rightController != null ? rightController.position : Vector3.zero;
    }

    void Update()
    {
        // ���������� ���������� ������� ���������
        if (ambientWaterSource != null)
        {
            float boatSpeed = rb.linearVelocity.magnitude;
            // ���������: ������� + ����������� �� ��������
            ambientWaterSource.volume = Mathf.Clamp(ambientBaseVolume + (boatSpeed * ambientSpeedMultiplier), 0f, 1f);
            ambientWaterSource.pitch = Mathf.Lerp(0.9f, 1.1f, boatSpeed / 5f);
        }
    }

    void LateUpdate()
    {
        if (leftController == null || rightController == null || doublePaddle == null) return;

        // �������� ������������ ������������ � ������
        constrainedLeftPos = GetConstrainedPosition(leftController.position, constrainedLeftPos);
        constrainedRightPos = GetConstrainedPosition(rightController.position, constrainedRightPos);

        // �������� ����������� ������� ����� ������ �������
        if (CheckShaftCollision(constrainedLeftPos, constrainedRightPos))
        {
            constrainedLeftPos = leftBlade.constrainedPosition;
            constrainedRightPos = rightBlade.constrainedPosition;
        }

        leftBlade.constrainedPosition = constrainedLeftPos;
        rightBlade.constrainedPosition = constrainedRightPos;

        float handDistance = Vector3.Distance(constrainedLeftPos, constrainedRightPos);
        isPaddleActive = handDistance >= minHandDistance && handDistance <= maxHandDistance;

        // ���������� ���������� �����
        doublePaddle.position = (constrainedLeftPos + constrainedRightPos) * 0.5f;
        Vector3 forward = constrainedRightPos - constrainedLeftPos;
        if (forward.magnitude > 0.01f)
            doublePaddle.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    void FixedUpdate()
    {
        if (OceanRenderer.Instance == null) return; // �������� ������� ������� Crest [cite: 312]

        if (!_initialized)
        {
            _leftHeightHelper = new SampleHeightHelper();
            _rightHeightHelper = new SampleHeightHelper();
            _leftFlowHelper = new SampleFlowHelper();
            _rightFlowHelper = new SampleFlowHelper();
            _initialized = true;
        }

        if (!isPaddleActive)
        {
            leftInWater = false; rightInWater = false;
            StopBladeAudio(leftBlade); StopBladeAudio(rightBlade);
            return;
        }

        ProcessBlade(leftBlade, ref lastLeftTip, ref leftInWater, _leftHeightHelper, _leftFlowHelper);
        ProcessBlade(rightBlade, ref lastRightTip, ref rightInWater, _rightHeightHelper, _rightFlowHelper);
    }

    void ProcessBlade(Blade blade, ref Vector3 lastPos, ref bool wasInWater, SampleHeightHelper heightHelper, SampleFlowHelper flowHelper)
    {
        Vector3 current = blade.bladeTip.position;
        Vector3 velocity = (current - lastPos) / Time.fixedDeltaTime;
        lastPos = current;

        bool currentlyOnGround = false;

        // ���� ������� ������ ����� � ���������� ����
        if (Physics.CheckSphere(current, bladeCollisionRadius, blockingLayers)) 
        { 
            wasInWater = false; 
            RaycastHit hit;
            currentlyOnGround = Physics.SphereCast(current, bladeCollisionRadius, Vector3.down, out hit, 0.5f, groundLayer); 
            return; 
        }


        heightHelper.Init(current, minSpatialLength);
        if (!heightHelper.Sample(out float waterHeight, out _, out Vector3 waterVelocity)) return;

        flowHelper.Init(current, minSpatialLength);
        if (flowHelper.Sample(out Vector2 flow2D)) waterVelocity += new Vector3(flow2D.x, 0f, flow2D.y);

        bool currentlyInWater = current.y < waterHeight + bladeDepthThreshold;

        if (currentlyInWater || currentlyOnGround)
        {           
            Vector3 relVel = velocity - waterVelocity;
            float intensity = relVel.magnitude;

            // ������ ��������� ���������: $V = \sqrt{intensity / maxSpeed} \cdot multiplier$
            float normIntensity = Mathf.Clamp01(intensity / maxEffectiveSpeed);
            float calculatedVolume = Mathf.Sqrt(normIntensity) * volumeMultiplier;
            
            // ����: ������ �������� ��� �����
            if (!wasInWater && intensity > 0.35f && splashClip != null)
            {
                blade.bladeAudio.pitch = Random.Range(pitchMin, pitchMax);
                blade.bladeAudio.PlayOneShot(splashClip, calculatedVolume);
            }

            // ����: ���������� �������� ��� �������� ��� �����
            if (intensity > 0.15f)
            {
                if (!blade.bladeAudio.isPlaying) blade.bladeAudio.Play();
                blade.bladeAudio.volume = Mathf.Lerp(blade.bladeAudio.volume, calculatedVolume, Time.fixedDeltaTime * 8f);
            }
            if(intensity > 0.1f)
            {

                // ������: ���������� ���� ������
                Vector3 localVel = transform.InverseTransformDirection(relVel);
                float angleEff = Mathf.Lerp(minEfficiency, maxEfficiency, Mathf.Pow(Mathf.Abs(Vector3.Dot(blade.bladeRoot.up, Vector3.up)), 2));

                if (localVel.z < -0.1f) // ����� �����
                {
                    if(currentlyInWater)
                        rb.AddForceAtPosition(transform.forward * Mathf.Clamp(-localVel.z, 0f, maxEffectiveSpeed) * forceMultiplier * angleEff, current, ForceMode.Force);
                    else
                        rb.AddForceAtPosition(transform.forward * Mathf.Clamp(-localVel.z, 0f, maxEffectiveSpeed) * forceMultiplier * minEfficiency, current, ForceMode.Force);
                }
            }
            else
            {
                Vector3 localVel = transform.InverseTransformDirection(relVel);
                float angleEff = Mathf.Lerp(minEfficiency, maxEfficiency, Mathf.Pow(Mathf.Abs(Vector3.Dot(blade.bladeRoot.up, Vector3.up)), 2));

                if(currentlyInWater)
                    rb.AddForceAtPosition(waterVelocity * Mathf.Clamp(-localVel.z, 0f, maxEffectiveSpeed) * forceMultiplier * angleEff, current, ForceMode.Force);
            }
        }
        else if (wasInWater)
        {
            StopBladeAudio(blade);
        }
        wasInWater = currentlyInWater;
    }

    void StopBladeAudio(Blade blade)
    {
        if (blade.bladeAudio != null && blade.bladeAudio.isPlaying)
            blade.bladeAudio.Stop();
    }

    // ���� �������� ������ �������������� ����������� ������ ����
    Vector3 GetConstrainedPosition(Vector3 targetPos, Vector3 currentPos)
    {
        Vector3 direction = targetPos - currentPos;
        float distance = direction.magnitude;
        if (distance < 0.001f) return currentPos;

        if (Physics.SphereCast(currentPos, bladeCollisionRadius, direction.normalized, out RaycastHit hit, distance, blockingLayers))
            return currentPos + direction.normalized * Mathf.Max(0, hit.distance - bladeCollisionRadius * 0.5f);

        if (Physics.CheckSphere(targetPos, bladeCollisionRadius, blockingLayers)) return currentPos;
        return targetPos;
    }

    bool CheckShaftCollision(Vector3 leftPos, Vector3 rightPos)
    {
        for (int i = 0; i < shaftCheckPoints; i++)
        {
            float t = i / (float)(shaftCheckPoints - 1);
            Vector3 checkPoint = Vector3.Lerp(leftPos, rightPos, t);
            if (Physics.CheckSphere(checkPoint, shaftCollisionRadius, blockingLayers)) return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (leftController == null || rightController == null) return;
        Gizmos.color = isPaddleActive ? Color.green : Color.red;
        Gizmos.DrawLine(constrainedLeftPos, constrainedRightPos);
    }
}