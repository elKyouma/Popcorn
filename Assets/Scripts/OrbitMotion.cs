using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMotion : MonoBehaviour
{
    [SerializeField] private Transform orbitingObject;
   // [SerializeField] private Ellipse orbitPath;

    private float orbitProgress = 0f;
    private float orbitPeriod = 3f;
    private bool orbitActive = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
