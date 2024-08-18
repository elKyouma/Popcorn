using UnityEngine;
using UnityEngine.UIElements;

public interface IShipElement
{
    enum ShipElementType
    {
        EMPTY,
        ENGINE,
        CORE,
        FULL
    }

    public ShipElementType GetElementType();
    public void SetCoords(Vector2Int coords);
    public Vector2Int GetCoords();
    public GameObject GetGameObject();
    public void SetBuilderRef(ShipBuilder builder);
}
