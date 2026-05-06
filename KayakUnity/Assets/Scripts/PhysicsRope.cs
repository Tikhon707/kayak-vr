using UnityEngine;

public class PhysicsRope : MonoBehaviour
{
    [Header("Точки крепления")]
    public Transform kayakAttachPoint;   // точка на корме каяка
    public Transform raftAttachPoint;    // точка на носу плота
    
    [Header("Параметры веревки")]
    public float maxRopeLength = 5f;     // максимальная длина
    public float ropeStrength = 10f;     // сила натяжения
    public float dampingRatio = 0.7f;    // демпфирование колебаний
    
    [Header("Визуал")]
    public float ropeWidth = 0.05f;
    public Color ropeColor = new Color(0.55f, 0.35f, 0.15f);
    [Range(0, 1)]
    public float sagAmount = 0.3f;       // провисание
    public int sagSegments = 8;          // сегментов для провисания
    
    private SpringJoint joint;
    private LineRenderer line;
    private Rigidbody kayakRb;
    private Rigidbody raftRb;
    
    void Start()
    {
        SetupLineRenderer();
        SetupSpringJoint();
    }
    
    void SetupLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = sagSegments + 1;
        line.startWidth = ropeWidth;
        line.endWidth = ropeWidth;
        
        // Простой материал для веревки
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = ropeColor;
        line.endColor = ropeColor;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }
    
    void SetupSpringJoint()
    {
        kayakRb = kayakAttachPoint.GetComponentInParent<Rigidbody>();
        raftRb = raftAttachPoint.GetComponentInParent<Rigidbody>();
        
        if (kayakRb == null || raftRb == null)
        {
            Debug.LogError("Точки крепления должны быть на объектах с Rigidbody!");
            return;
        }
        
        // Добавляем Joint на каяк
        joint = kayakRb.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = raftRb;
        
        // Конвертируем мировые позиции в локальные
        joint.anchor = kayakRb.transform.InverseTransformPoint(kayakAttachPoint.position);
        joint.connectedAnchor = raftRb.transform.InverseTransformPoint(raftAttachPoint.position);
        
        // Настройка физики
        joint.maxDistance = maxRopeLength;
        joint.minDistance = 0;
        joint.spring = ropeStrength * 100f;
        joint.damper = ropeStrength * 10f * dampingRatio;
        
        // Дополнительные настройки
        joint.tolerance = 0.01f;
        joint.autoConfigureConnectedAnchor = false;
        joint.enableCollision = false;  // чтобы Joint не вызывал лишние коллизии
    }
    
    void LateUpdate()
{
    if (kayakAttachPoint == null || raftAttachPoint == null) return;
    
    Vector3 startPoint = kayakAttachPoint.position;
    Vector3 endPoint = raftAttachPoint.position;
    
    // Обновляем Anchor если объекты двигаются
    if (joint != null)
    {
        joint.anchor = kayakRb.transform.InverseTransformPoint(startPoint);
        joint.connectedAnchor = raftRb.transform.InverseTransformPoint(endPoint);
    }
    
    // Обновляем визуал веревки с провисанием
    for (int i = 0; i <= sagSegments; i++)
    {
        float t = i / (float)sagSegments;
        Vector3 point = Vector3.Lerp(startPoint, endPoint, t);
        
        // Добавляем провисание с учетом расстояния
        float distance = Vector3.Distance(startPoint, endPoint);
        float currentSag = Mathf.Sin(t * Mathf.PI) * (sagAmount * distance / maxRopeLength);
        point.y -= Mathf.Max(0, currentSag);
        
        line.SetPosition(i, point);
    }
}
    
    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        if (kayakAttachPoint != null && raftAttachPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(kayakAttachPoint.position, raftAttachPoint.position);
            Gizmos.DrawSphere(kayakAttachPoint.position, 0.1f);
            Gizmos.DrawSphere(raftAttachPoint.position, 0.1f);
        }
    }
    
    // Автоматическое удаление Joint при уничтожении
    void OnDestroy()
    {
        if (joint != null)
            Destroy(joint);
    }
}