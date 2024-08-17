using UnityEngine;

public interface IShipElement
{
    enum ShipElementType
    {
        Empty,
        Engine,
        Full
    }

    public ShipElementType GetElementType();
    public GameObject GetGameObject();
    public void SetBuilderRef(ShipBuilder builder);
}
