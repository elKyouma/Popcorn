using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBox : ShipElement
{
    public override ShipElementType GetElementType() => ShipElementType.FULL;

    public override void OnDeath() => builder.DestroyElement(coord);

    public override void OnUpgrade()
    {
        maxHp *= 2;
        HP = maxHp;
    }
}
