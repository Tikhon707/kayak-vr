using UnityEngine;
using Crest;

public class CrestWaterProvider : MonoBehaviour, IWaterSurfaceProvider
{
    [Header("Crest Settings")]
    public float minSpatialLength = 1f;

    private SampleHeightHelper _heightHelper;
    private SampleFlowHelper _flowHelper;
    private bool _initialized = false;

    private void InitializeIfNeeded()
    {
        if (!_initialized)
        {
            _heightHelper = new SampleHeightHelper();
            _flowHelper = new SampleFlowHelper();
            _initialized = true;
        }
    }

    public bool GetWaterData(Vector3 position, out float waterHeight, out Vector3 waterVelocity)
    {
        InitializeIfNeeded();

        waterHeight = 0f;
        waterVelocity = Vector3.zero;

        if (OceanRenderer.Instance == null) return false;

        // Get height
        _heightHelper.Init(position, minSpatialLength);
        if (!_heightHelper.Sample(out waterHeight, out _, out Vector3 surfaceVel)) return false;

        // Get flow
        _flowHelper.Init(position, minSpatialLength);
        if (_flowHelper.Sample(out Vector2 flow2D))
        {
            surfaceVel += new Vector3(flow2D.x, 0f, flow2D.y);
        }

        waterVelocity = surfaceVel;
        return true;
    }
}
