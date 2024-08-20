using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class FullBox : ShipElement
{
    public override ShipElementType GetElementType() => ShipElementType.FULL;

    public override void OnDeath()
    {
        builder.DestroyElement(coord);
    }
}
