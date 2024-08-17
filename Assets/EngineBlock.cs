using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IShipElement;

public class EngineBlock : MonoBehaviour, IShipElement
{
    //[SerializeField] private Color baseColor, highlightColor;

    //private SpriteRenderer rend;
    private ShipBuilder builder;
    private Vector2Int coord;
    public ShipElementType GetElementType() => ShipElementType.Engine;
    public GameObject GetGameObject() => gameObject;

    //private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start()
    {
        //rend.color = baseColor;
        coord = new Vector2Int((int)transform.position.x, (int)transform.position.y);
    }

    public void SetBuilderRef(ShipBuilder builder) => this.builder = builder;

    private void OnMouseEnter()
    {
        //rend.color = highlightColor;
        builder.ChangeActiveElement(coord);
    }

    private void OnMouseExit()
    {
        //rend.color = baseColor;
        builder.ChangeActiveElement(coord);
    }
}