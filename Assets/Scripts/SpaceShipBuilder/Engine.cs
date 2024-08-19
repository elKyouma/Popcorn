using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : ShipElement
{
    private Rigidbody2D playerBody;
    private GameObject flame;
    public Sprite spriteToDisplay;
    public List<KeyCode> keyCodes;

    void Update()
    {
        if(playerBody == null)
            playerBody = GetComponentInParent<Rigidbody2D>();

        foreach (KeyCode keyCode in keyCodes)
        {
            if (Input.GetKey(keyCode))
            {
                ApplyForce();
            }
            if (Input.GetKeyDown(keyCode))
            {
                ShowMagic();
            }
            if (Input.GetKeyUp(keyCode))
            {
                HideMagic();
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
        playerBody.AddForceAtPosition(transform.up, transform2D);
    }

    public override ShipElementType GetElementType() => ShipElementType.ENGINE;

    public override void OnDeath()
    {
    }
}