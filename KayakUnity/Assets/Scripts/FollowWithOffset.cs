using UnityEngine;

public class FollowWithOffset : MonoBehaviour
{
    public Transform target;               // Движущийся объект
    public Vector3 localOffset = new Vector3(0f, 0f, -5f); // Смещение в локальных координатах целевого объекта

    void LateUpdate() // Используем LateUpdate, чтобы следовать после движения цели
    {
        if (target == null) return;

        // Вычисляем желаемую позицию в мировых координатах
        Vector3 desiredPosition = target.TransformPoint(localOffset);

        // Применяем позицию
        transform.position = desiredPosition;

        // Копируем только поворот по оси Y
        Quaternion targetRotation = target.rotation;
        float targetYRotation = targetRotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
    }
}