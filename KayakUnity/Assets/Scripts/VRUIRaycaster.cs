using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VRUIRaycaster : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private InputActionReference triggerAction; // Trigger контроллера
    [SerializeField] private float rayLength = 10f;
    [SerializeField] private LayerMask uiLayer;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        if (triggerAction != null)
            triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        if (triggerAction != null)
            triggerAction.action.Disable();
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        // Обновляем линию
        if (_lineRenderer != null)
        {
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, transform.position + transform.forward * rayLength);
        }

        // Raycast по UI
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, uiLayer))
        {
            // Укорачиваем линию до точки попадания
            if (_lineRenderer != null)
                _lineRenderer.SetPosition(1, hit.point);

            // Клик по триггеру
            if (triggerAction != null && triggerAction.action.WasPressedThisFrame())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = hit.textureCoord * new Vector2(Screen.width, Screen.height)
                };

                // Находим кнопку и кликаем
                GameObject hitObject = hit.collider.gameObject;
                ExecuteEvents.Execute(hitObject, pointerData, ExecuteEvents.pointerClickHandler);

                // Ищем кнопку у родителей если не нашли
                ExecuteEvents.ExecuteHierarchy(hitObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}