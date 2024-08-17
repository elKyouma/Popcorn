using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using static IShipElement;

public class EmptyBox : MonoBehaviour, IShipElement
{
    [SerializeField] private Color baseColor, highlightColor;

    private SpriteRenderer rend;
    private ShipBuilder builder;

    Vector2Int coord;
    ShipElementType IShipElement.GetElementType() => ShipElementType.Empty;
    GameObject IShipElement.GetGameObject() => gameObject;

    private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start()
    {
        rend.color = baseColor;
        coord = new Vector2Int((int)transform.position.x, (int) transform.position.y);
    }

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
