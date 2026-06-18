using UnityEngine;

public interface IWaterSurfaceProvider
{
    // Returns true if water data was successfully retrieved at the given position
    bool GetWaterData(UnityEngine.Vector3 position, out float waterHeight, out UnityEngine.Vector3 waterVelocity);
}
