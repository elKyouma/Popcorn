using UnityEngine;

public interface IShipElement
{
    enum ShipElementType
    {
        Empty,
        Full
    }

    public ShipElementType GetElementType();
    public GameObject GetGameObject();
    public void SetBuilderRef(ShipBuilder builder);
}
