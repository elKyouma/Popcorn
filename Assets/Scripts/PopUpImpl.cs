using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpImpl : MonoBehaviour
{
    ShipBuilder shipBuilder;

    public void SetShipBuilderRef(ShipBuilder builder) => shipBuilder = builder;

    public void OnKeybindingsOn()
    {
    }

    public void OnKeybindingsOff()
    {

    }

    public void OnDestroyElement()
    {
        shipBuilder.DestroyCurrentElement();
    }

    public void OnUpgradeElement()
    {

    }
}
