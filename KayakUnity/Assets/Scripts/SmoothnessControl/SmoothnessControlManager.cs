using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SmoothnessControlManager : MonoBehaviour, IScoreManager
{
    public GameObject cubesObj;

    public float missCubePenalty = 10f;
    public float timePenalty = 2f;
    public float initScore = 5000f;

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

    public int GetMissedScores()
    {
        return Mathf.Max(0, _missedCubes);
    }

    public float GetScore(float finishTime)
    {
        return initScore - (finishTime * timePenalty + missCubePenalty * _missedCubes);
    }

    private void MissCube()
    {
        _missedCubes++;
    }

    private void OnEnable()
    {
        Bottom.MissedCube += MissCube;
    }

    private void OnDisable()
    {
        Bottom.MissedCube -= MissCube;
    }
}