using System;
using UnityEngine;

public class Bottom : MonoBehaviour
{
    public static event Action MissedCube;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            MissedCube?.Invoke();
        }
    }
}
