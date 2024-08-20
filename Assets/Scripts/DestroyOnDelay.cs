using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDelay : MonoBehaviour
{
    [SerializeField] private float delay;

    private void Start() => Destroy(gameObject, delay);

}
