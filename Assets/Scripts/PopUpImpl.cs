using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpImpl : MonoBehaviour
{
    ShipBuilder shipBuilder;

    bool locked = false;

    public void Lock() => locked = true;
    public void Unlock() => locked = false;

    public void SetShipBuilderRef(ShipBuilder builder) => shipBuilder = builder;



    public void OnKeybindingChange(int id)
    {
        if (locked)
            return;

        var conf = shipBuilder.GetCurrentConfig();
        shipBuilder.ActiveElement.ToogleBinding(conf.possibleKeybindings[id]);
    }

    public void OnDestroyElement()
    {
        shipBuilder.DestroyCurrentElement();
    }

    public void OnUpgradeElement()
    {

    }
}
