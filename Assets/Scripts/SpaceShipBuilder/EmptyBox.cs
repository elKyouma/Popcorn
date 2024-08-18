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

    Vector2Int coord = Vector2Int.zero;
    public ShipElementType GetElementType() => ShipElementType.EMPTY;

    GameObject IShipElement.GetGameObject() => gameObject;

    bool visible = true;

    public void SetCoords(Vector2Int coords) => coord = coords;
    public Vector2Int GetCoords() => coord;


    private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start()
    {
        rend.color = baseColor;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            visible = !visible;
            if (visible)
                rend.color = baseColor;
            else
                rend.color = Color.clear;
        }    
    }

    public void SetBuilderRef(ShipBuilder builder) => this.builder = builder;

    private void OnMouseEnter()
    {
        rend.color = highlightColor;
        builder.SetValidCoord(true);
        builder.ChangeActiveElement(coord);
    }

    private void OnMouseExit()
    {
        rend.color = baseColor;
        builder.SetValidCoord(false);
    }
}
