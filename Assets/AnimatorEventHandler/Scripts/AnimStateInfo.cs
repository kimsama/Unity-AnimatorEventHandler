using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class AnimStateInfo : ScriptableObject
{
    public string stateName = string.Empty;
    public float transitionDuration = 0.1f;
    public int layer = -1;
    public float normalizedTime = float.NegativeInfinity;

    void OnEnable()
    {

    }
}
