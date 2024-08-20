using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBox : ShipElement
{

    public override ShipElementType GetElementType() => ShipElementType.CORE;

    public override void OnDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
