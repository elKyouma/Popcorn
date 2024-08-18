using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class EmptyBox : ShipElement
{
    public override ShipElementType GetElementType() => ShipElementType.EMPTY;

    bool visible = true;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            visible = !visible;
            if (visible)
                rend.color = baseColor;
            else
                rend.color = Color.clear;
        }    
    }

    public override void OnDeath()
    {
        Debug.LogError("StrangeCollision");
    }
}
