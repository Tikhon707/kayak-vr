using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class WaterDeformationSystem : MonoBehaviour
{
    public int resolution = 256; // 256 или 512
    public float decay = 0.96f; // Затухание за кадр
    public float maxHeight = 0.2f; // Макс. смещение по Y

    private RenderTexture currentRT;
    private RenderTexture previousRT;
    private Material accumulateMaterial;
    private Material brushMaterial;
    private Material clearMaterial;

    private CommandBuffer commandBuffer;

    void Start()
    {
        // Создаём текстуры
        currentRT = CreateRT();
        previousRT = CreateRT();

        // Загружаем материалы (см. шаги 2–3)
        brushMaterial = new Material(Shader.Find("Hidden/WaterBrush"));
        clearMaterial = new Material(Shader.Find("Hidden/WaterClear"));
        accumulateMaterial = new Material(Shader.Find("Hidden/WaterAccumulate"));

        // Назначаем в материал воды (ваш Shader Graph)
        var waterMat = GameObject.Find("WaterPlane").GetComponent<Renderer>().material;
        waterMat.SetTexture("_WaterHeightRT", currentRT);
        waterMat.SetFloat("_WaterMaxHeight", maxHeight);

        // Опционально: используем CommandBuffer вместо LateUpdate
    }

    RenderTexture CreateRT()
    {
        var rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;
        rt.Create();
        return rt;
    }

    public void AddDisturbance(Vector3 worldPos, float radius, float intensity)
    {
        // Сохраняем для обработки в LateUpdate
        disturbances.Add(new Disturbance { pos = worldPos, radius = radius, intensity = intensity });
    }

    private struct Disturbance
    {
        public Vector3 pos;
        public float radius;
        public float intensity;
    }
    private System.Collections.Generic.List<Disturbance> disturbances = new();

    void LateUpdate()
    {
        if (disturbances.Count == 0)
        {
            // Просто затухание
            Graphics.Blit(previousRT, currentRT, clearMaterial); // чистим
            Graphics.Blit(currentRT, previousRT); // swap
            return;
        }

        // 1. Затухание предыдущего состояния → temp
        RenderTexture temp = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.R8);
        accumulateMaterial.SetFloat("_Decay", decay);
        Graphics.Blit(previousRT, temp, accumulateMaterial);

        // 2. Рисуем новые возмущения поверх temp → currentRT
        Graphics.Blit(temp, currentRT, clearMaterial); // сначала очищаем currentRT

        foreach (var d in disturbances)
        {
            Vector2 uv = WorldToUV(d.pos);
            brushMaterial.SetVector("_Center", uv);
            brushMaterial.SetFloat("_Radius", d.radius / (resolution * 0.5f)); // нормализуем радиус
            brushMaterial.SetFloat("_Intensity", d.intensity);
            Graphics.Blit(null, currentRT, brushMaterial);
        }

        RenderTexture.ReleaseTemporary(temp);

        // 3. Swap
        (currentRT, previousRT) = (previousRT, currentRT);

        // Обновляем материал воды
        var waterMat = GameObject.Find("WaterPlane").GetComponent<Renderer>().material;
        waterMat.SetTexture("_WaterHeightRT", currentRT);

        disturbances.Clear();
    }

    // Предполагаем, что вода — плоскость XZ с центром в (0,0,0) и размером 100x100
    Vector2 WorldToUV(Vector3 worldPos)
    {
        float planeSize = 100f; // ← должно совпадать с размером вашей водной плоскости
        return new Vector2(
            (worldPos.x + planeSize * 0.5f) / planeSize,
            (worldPos.z + planeSize * 0.5f) / planeSize
        );
    }

    void OnDestroy()
    {
        if (currentRT) currentRT.Release();
        if (previousRT) previousRT.Release();
    }
}