using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GhostFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class GhostData
{
    public float totalTime;
    public List<GhostFrame> frames = new List<GhostFrame>();
}
