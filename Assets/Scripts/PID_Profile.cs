using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PID Profile", menuName = "Create PID profile")]
public class PID_Profile : ScriptableObject
{
    [Range(0, 10)]
    public float P = 1; 
    [Range(0, 10)]
    public float I = 1;
    [Range(0, 10)]
    public float D = 1;
}
