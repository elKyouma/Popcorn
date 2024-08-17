using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    private Transform playerTransform;
    private Rigidbody2D playerBody;
    private GameObject flame;
    public Sprite spriteToDisplay;
    public List<KeyCode> keyCodes;
    private void Start()
    {
        playerTransform = GetComponentInParent<Transform>();
        playerBody = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
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
        flame.transform.position = transform.position - transform.up;
        flame.transform.localScale = new Vector3(2, 2, 0);
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
        playerBody.GetComponent<Rigidbody2D>().AddForceAtPosition(playerTransform.up, transform2D);
    }
}