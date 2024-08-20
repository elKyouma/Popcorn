using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UIElements;

public class OrbitMotion : MonoBehaviour
{
    [SerializeField] private List<GameObject> orbitingObjects = new List<GameObject>();
    private List<Orbiter> orbiterList = new List<Orbiter>();
    private bool orbitActive = true;
    private System.Random random = new System.Random();
    void Start()
    {
        if (!orbitingObjects.Any())
        {
            orbitActive = false;
            return;
        }

        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            Orbiter orbiter = new Orbiter();
            orbiter.obj = Instantiate(orbitingObjects[i], new Vector3(), new Quaternion(), transform);
            Vector2 orbitPos = orbiter.orbitPath.Evaluate(new System.Random().Next(100) / 100f);
            orbiter.obj.transform.localPosition = new Vector3(orbitPos.x, orbitPos.y, 0);
            orbiter.orbitProgress = new System.Random().Next(100) / 100f;
            orbiter.ChangeOrbitSpeed(new System.Random().Next(100) / 100f);
            StartCoroutine(AnimateOrbit(orbiter));
            orbiterList.Add(orbiter);
        }
    }

    IEnumerator AnimateOrbit(Orbiter orbiter)
    {
        if (orbiter.orbitPeriod < 0.01f)
        {
            orbiter.orbitPeriod = 0.01f;
        }
        float orbitSpeed = 1f / orbiter.orbitPeriod;
        while (orbitActive)
        {
            orbiter.orbitProgress += Time.deltaTime * orbitSpeed * 0.1f;
            orbiter.orbitProgress %= 1f;
            orbiter.SetOrbitingObjectPosition();
            yield return null;
        }
    }

    [System.Serializable]
    private class Orbiter
    {

        public GameObject obj;
        public float orbitProgress;
        public float orbitSpeed;
        public float orbitPeriod = 3f;
        public Ellipse orbitPath = new Ellipse(3, 4);
        public void SetOrbitingObjectPosition()
        {
            Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
            obj.transform.localPosition = new Vector3(orbitPos.x, orbitPos.y, 0);
        }
        public void ChangeOrbitSpeed(float speed) { orbitSpeed = speed; }
    }
}
