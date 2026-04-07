using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GhostFrame
{
    public float timestamp;   
    public Vector3 position; 
    public Quaternion rotation; 
}

[System.Serializable]
public class GhostRun
{
    public float raceTime;
    public List<GhostFrame> frames = new List<GhostFrame>();
}