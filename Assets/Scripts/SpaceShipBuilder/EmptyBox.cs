using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBox : ShipElement
{
    public override ShipElementType GetElementType() => ShipElementType.EMPTY;


    private void Start()
    {
        if (builder.BuildMode)
            rend.color = baseColor;
        else
            rend.color = Color.clear;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            if (builder.BuildMode)
                rend.color = baseColor;
            else
                rend.color = Color.clear;
        }    
    }

    public override void OnDeath() => Debug.LogError("StrangeCollision");
    public override void OnUpgrade() => Debug.LogError("StrangeUpgrade");
}
