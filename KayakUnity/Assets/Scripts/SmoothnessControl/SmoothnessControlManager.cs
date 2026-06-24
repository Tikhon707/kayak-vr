using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SmoothnessControlManager : BaseScoreManager
{
    public static event Action<float, int> OnRaceFinished;
    public GameObject cubesObj;

    public float missCubePenalty = 10f;
    public float timePenalty = 2f;
    public float initScore = 500f;

    private int _totalCubes;
    private int _missedCubes;

    void Start()
    {
        CountAllTiles();
    }

    void CountAllTiles()
    {
        if (cubesObj == null)
        {
            return;
        }

        var allTilemaps = cubesObj.GetComponentsInChildren<Tilemap>();

        _totalCubes = 0;

        foreach (var tilemap in allTilemaps)
        {
            var meshRenderers = tilemap.GetComponentsInChildren<MeshRenderer>();
            _totalCubes += meshRenderers.Length;
        }
    }

    public float GetScore(float finishTime)
    {
        return Mathf.Max(0, initScore - (finishTime * timePenalty + missCubePenalty * _missedCubes));
    }

    private void MissCube()
    {
        _missedCubes++;
    }

    protected override void OnEnableCustom()
    {
        Bottom.MissedCube += MissCube;
    }

    protected override void OnDisableCustom()
    {
        Bottom.MissedCube -= MissCube;
    }

    public override void Finish(float time)
    {
        OnRaceFinished?.Invoke(GetScore(time), _missedCubes);
    }
}