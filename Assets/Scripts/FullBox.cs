using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IShipElement;

public class FullBox : MonoBehaviour, IShipElement
{
    [SerializeField] private Color baseColor, highlightColor;

    private SpriteRenderer rend;
    private ShipBuilder builder;

    Vector2Int coord = Vector2Int.zero;
    public ShipElementType GetElementType() => ShipElementType.FULL;

    public GameObject GetGameObject() => gameObject;

    private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start() => rend.color = baseColor;
    public void SetCoords(Vector2Int coords) => this.coord = coords;

    public void SetBuilderRef(ShipBuilder builder) => this.builder = builder;

    private void OnMouseEnter()
    {
        rend.color = highlightColor;
        builder.ChangeActiveElement(coord);
        builder.SetValidCoord(true);

    }

    private void OnMouseExit()
    {
        rend.color = baseColor;
        builder.SetValidCoord(false);
    }
}
