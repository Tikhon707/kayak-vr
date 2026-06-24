using System;
using UnityEngine;

public abstract class BaseScoreManager : MonoBehaviour
{
    private void OnEnable()
    {
        RaceManager.OnRaceFinished += Finish;
        OnEnableCustom();
    }

    private void OnDisable()
    {
        RaceManager.OnRaceFinished -= Finish;
        OnDisableCustom();
    }

    protected virtual void OnEnableCustom()
    {
    }

    protected virtual void OnDisableCustom()
    {
    }

    public abstract void Finish(float time);
}