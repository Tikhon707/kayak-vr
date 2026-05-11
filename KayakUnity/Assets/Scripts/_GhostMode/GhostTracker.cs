using System.Collections.Generic;
using UnityEngine;

public class GhostTracker
{
    private readonly List<GhostFrame> frames;

    // Последний найденный индекс
    private int lastClosestIndex = 0;

    // Размер окна поиска
    private const int SearchWindow = 30;

    // Если игрок слишком далеко — делаем полный поиск
    private const float FallbackDistanceSqr = 25f * 25f;

    public GhostTracker(List<GhostFrame> frames)
    {
        this.frames = frames;
    }

    public (float closestTime, float minDistance) FindClosest(Vector3 playerPosition)
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogError("Frames empty!");
            return (-1f, -1f);
        }

        int bestIndex = lastClosestIndex;
        float bestSqrDist = float.MaxValue;

        //----------------------------------------
        // LOCAL SEARCH
        //----------------------------------------

        int start = Mathf.Max(0, lastClosestIndex - SearchWindow);
        int end = Mathf.Min(frames.Count - 1, lastClosestIndex + SearchWindow);

        for (int i = start; i <= end; i++)
        {
            float sqrDist =
                (frames[i].position - playerPosition).sqrMagnitude;

            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                bestIndex = i;
            }
        }

        //----------------------------------------
        // FALLBACK FULL SEARCH
        //----------------------------------------

        // Если вдруг игрок оказался слишком далеко
        // (телепорт / респавн / срез / старт гонки)
        if (bestSqrDist > FallbackDistanceSqr)
        {
            bestSqrDist = float.MaxValue;

            for (int i = 0; i < frames.Count; i++)
            {
                float sqrDist =
                    (frames[i].position - playerPosition).sqrMagnitude;

                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    bestIndex = i;
                }
            }
        }

        lastClosestIndex = bestIndex;

        return (
            frames[bestIndex].timestamp,
            Mathf.Sqrt(bestSqrDist)
        );
    }
}