using UnityEngine;

public interface IShipElement
{
    enum ShipElementType
    {
        EMPTY,
        ENGINE,
        FULL
    }

    public ShipElementType GetElementType();
    public void SetCoords(Vector2Int coords);
    public GameObject GetGameObject();
    public void SetBuilderRef(ShipBuilder builder);
}
