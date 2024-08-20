using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpImpl : MonoBehaviour
{
    ShipBuilder shipBuilder;
    PopUpFiller filler;
    bool locked = false;

    private void Awake() => filler = GetComponent<PopUpFiller>();

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

    public void OnDestroyElement() => shipBuilder.TryDestroyCurrentElement();
    public void OnUpgradeElement() => shipBuilder.UpgradeCurrentElement();
}
