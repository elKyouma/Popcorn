using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMotion : MonoBehaviour
{
    [SerializeField] private Transform orbitingObject;
    [SerializeField] public Ellipse orbitPath = new Ellipse(2,3);

    private float orbitProgress = 0f;
    [SerializeField] private float orbitPeriod = 3f;
    private bool orbitActive = true;
    void Start()
    {
        if (orbitingObject == null)
        {
            orbitActive = false;
            return;
        }
        SetOrbitingObjectPosition();
        StartCoroutine(AnimateOrbit());
    }

    void SetOrbitingObjectPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        orbitingObject.localPosition = new Vector3(orbitPos.x, orbitPos.y, 0);
    }

    IEnumerator AnimateOrbit()
    {
        if(orbitPeriod < 0.01f)
        {
            orbitPeriod = 0.01f;
        }
        float orbitSpeed = 1f / orbitPeriod;
        while ( orbitActive )
        {
            orbitProgress += Time.deltaTime * orbitSpeed * 0.1f;
            orbitProgress %= 1f;
            SetOrbitingObjectPosition();
            yield return null;
        }
    }
}
