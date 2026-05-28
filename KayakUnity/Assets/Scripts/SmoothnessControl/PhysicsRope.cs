using UnityEngine;

public class PhysicsRope : MonoBehaviour
{
    [Header("Точки крепления")]
    public Transform kayakAttachPoint;
    public Transform raftAttachPoint;

    [Header("Параметры веревки")]
    public float maxRopeLength = 5f;
    public float ropeStrength = 10f;
    public float dampingRatio = 0.7f;

    [Header("Визуал")]
    public float ropeWidth = 0.05f;
    public Color ropeColor = new Color(0.55f, 0.35f, 0.15f);

    [Range(0, 1)]
    public float sagAmount = 0.3f;

    public int sagSegments = 8;

    private SpringJoint joint;
    private LineRenderer line;

    private Rigidbody kayakRb;
    private Rigidbody raftRb;

    void Start()
    {
        SetupRigidbodies();
        SetupLineRenderer();
        SetupSpringJoint();
    }

    void SetupRigidbodies()
    {
        kayakRb = kayakAttachPoint.GetComponentInParent<Rigidbody>();
        raftRb = raftAttachPoint.GetComponentInParent<Rigidbody>();

        if (kayakRb == null || raftRb == null)
        {
            Debug.LogError("Не найден Rigidbody!");
            return;
        }

        // Стабильнее коллизии
        kayakRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        raftRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Плавнее физика
        kayakRb.interpolation = RigidbodyInterpolation.Interpolate;
        raftRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void SetupLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();

        line.positionCount = sagSegments + 1;

        line.startWidth = ropeWidth;
        line.endWidth = ropeWidth;

        line.material = new Material(Shader.Find("Sprites/Default"));

        line.startColor = ropeColor;
        line.endColor = ropeColor;

        line.useWorldSpace = true;

        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    void SetupSpringJoint()
    {
        if (kayakRb == null || raftRb == null)
            return;

        joint = kayakRb.gameObject.AddComponent<SpringJoint>();

        joint.connectedBody = raftRb;

        // Anchor задаются ОДИН раз
        joint.anchor =
            kayakRb.transform.InverseTransformPoint(kayakAttachPoint.position);

        joint.connectedAnchor =
            raftRb.transform.InverseTransformPoint(raftAttachPoint.position);

        // Ограничение длины
        joint.maxDistance = maxRopeLength;
        joint.minDistance = 0f;

        // Более мягкая пружина
        joint.spring = ropeStrength * 20f;
        joint.damper = ropeStrength * 5f * dampingRatio;

        joint.tolerance = 0.01f;

        joint.autoConfigureConnectedAnchor = false;

        // ВАЖНО:
        // разрешаем коллизии между связанными объектами
        joint.enableCollision = true;
    }

    void LateUpdate()
    {
        if (kayakAttachPoint == null || raftAttachPoint == null)
            return;

        DrawRope();
    }

    void DrawRope()
    {
        Vector3 startPoint = kayakAttachPoint.position;
        Vector3 endPoint = raftAttachPoint.position;

        float distance = Vector3.Distance(startPoint, endPoint);

        for (int i = 0; i <= sagSegments; i++)
        {
            float t = i / (float)sagSegments;

            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);

            // Провисание
            float sag =
                Mathf.Sin(t * Mathf.PI) *
                (sagAmount * distance / maxRopeLength);

            point.y -= Mathf.Max(0f, sag);

            line.SetPosition(i, point);
        }
    }

    void OnDrawGizmos()
    {
        if (kayakAttachPoint != null && raftAttachPoint != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(
                kayakAttachPoint.position,
                raftAttachPoint.position
            );

            Gizmos.DrawSphere(kayakAttachPoint.position, 0.1f);
            Gizmos.DrawSphere(raftAttachPoint.position, 0.1f);
        }
    }

    void OnDestroy()
    {
        if (joint != null)
        {
            Destroy(joint);
        }
    }
}