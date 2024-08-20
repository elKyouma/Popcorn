using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EngineBlock : ShipElement
{
    private Rigidbody2D playerBody;
    private GameObject flame;
    private float power = 40;
    public Sprite spriteToDisplay;

    private void Start()
    {
        bindings.Add(KeyCode.W);
    }
    void Update()
    {
        if(playerBody == null)
            playerBody = GetComponentInParent<Rigidbody2D>();

        foreach (KeyCode keyCode in bindings)
        {
            if (Input.GetKeyDown(keyCode))
                ShowMagic();
            if (Input.GetKeyUp(keyCode))
                HideMagic();
            if (Input.GetKey(keyCode))
            {
                ApplyForce();
                break;
            }
        }
    }
    void ShowMagic()
    {
        flame = new GameObject("FIREEEE");
        SpriteRenderer spriteRenderer = flame.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteToDisplay;
        flame.transform.position = transform.position - transform.up * 0.34f;
        flame.transform.localScale = Vector3.one * 2;
        flame.transform.parent = transform;
        flame.transform.up = transform.up;
    }

    void HideMagic()
    {
        Destroy(flame);
    }
    void ApplyForce()
    {
        Vector2 transform2D = transform.position;
        playerBody.AddForceAtPosition(transform.up * power, transform2D);
    }

    public override ShipElementType GetElementType() => ShipElementType.ENGINE;

    public override void OnDeath() => builder.DestroyElement(coord);
}