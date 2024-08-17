using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class Weapon : MonoBehaviour
{
    public KeyCode keyCode;
    public GameObject missile;
    private float nextFire = 0.0f;
    [SerializeField] float fireRate = 10f;
    [SerializeField] Image image;
    void Update()
    {
        if (Input.GetKey(keyCode) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            image.enabled = true;
            ShootMissile();
        }
        float procentage = (nextFire - Time.time) / fireRate;
        if  (procentage < 100)
        {
            image.fillAmount = procentage;
        }
        else
        {
            image.enabled = false;
        }
    }
    void ShootMissile()
    {
        Instantiate(missile, transform.position + transform.up , transform.rotation);
    }
}
