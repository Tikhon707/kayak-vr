using UnityEngine;

public class PoolWaterProvider : MonoBehaviour, IWaterSurfaceProvider
{
    [Header("Water Settings")]
    public float fixedWaterHeight = 0f;

    public bool GetWaterData(Vector3 position, out float waterHeight, out Vector3 waterVelocity)
    {
        waterHeight = fixedWaterHeight;
        waterVelocity = Vector3.zero; // Standing water, no currents
        return true;
    }
}
